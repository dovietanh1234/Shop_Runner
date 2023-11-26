using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SHOP_RUNNER.DTOs.Category_DTO;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.Email;
using SHOP_RUNNER.Models.Users;
using SHOP_RUNNER.Services.EmailService;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.WebRequestMethods;

namespace SHOP_RUNNER.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly RunningShopContext _context;

        private readonly IConfiguration _configuration;

        private readonly IEmailService _emailService;
        public UsersController(RunningShopContext context, IConfiguration configuration, IEmailService emailService) {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

     

        [HttpPost("register")]
        [EnableRateLimiting("fixedWindow")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            try
            {
                if ( _context.Users.Any(u => u.Email == request.Email  ) )
                    {
                        return BadRequest("user already exist");
                    }


                // PHAN 1: hanlde otp send otp throught EMAIL for user:
                //B1 lay ip client:
                var isAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if ( string.IsNullOrEmpty(isAddress) )
                {
                    isAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                }

                //B2: Create otp 
                string otp = create_otp();

                //B5 bổ sung -> gửi email cho client:
                _emailService.sentOtp(request.Email, otp);

                //B3 hash otp 
                CreateOtpHash(otp, out byte[] otp_hash, out byte[] otp_hash_salt);

                //B4 save otp_hash, otp_hash_salt in DB:
                var new_otp = new Otp
                {
                    IpClient = isAddress,
                    Email = request.Email,
                    Otphash = otp_hash,
                    OtphashSalt = otp_hash_salt,
                    OtpSpamNumber = 1,
                    OtpSpam = DateTime.Now.AddDays(1), // 1 ngay se chi duoc gui 5 cai otp 
                    LimitTimeToSendOtp = DateTime.Now.AddMinutes(1) // 2 phut se het han otp
                };


                //PHAN 2: create hashPassword throught user's password:
                // "out" is mean: we will take the values likes THAM CHIẾU ( khi một biến thay đổi thì giá trị ở chỗ khác cx bị thay đổi => tham chiếu "out" )

                
                CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var user_n = new Entities.User
            {
                Fullname = request.FullName,
                Email = request.Email,
                Role = "USER",
                PasswordSalt = passwordSalt,  //Encoding.UTF8.GetBytes("fghjktyuifvn");
                PasswordHash = passwordHash
            };
                _context.Users.Add(user_n);
                _context.Otps.Add(new_otp);
             
             await _context.SaveChangesAsync();

            // return Ok(new { message = passwordHash, message1 = passwordSalt });
            return Ok("Please check otp in your email");
            }
            catch (Exception ex)
            {
                return BadRequest( ex.Message );

            }
        }

        // verify otp để enable account:
        [HttpPost("verifyOtp")]
        [EnableRateLimiting("fixedWindow")]
        public async Task<IActionResult> verify_otp(string otp, string email)
        {
            // B1 lấy ip của client 
           
            /*
              var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            }
             */

            // B2 xác nhận IP có tồn tại ko?
            var new_otp_1 = _context.Otps.Where(o =>  o.Email == email  )
                                        .OrderByDescending(o => o.Id)
                                        .FirstOrDefault();

            if (new_otp_1 == null)
            {
                return BadRequest("please register account");
            }

            //B3 xác thực otp xem có đúng không?
            if (!VerifyOtpHash( otp, new_otp_1.Otphash, new_otp_1.OtphashSalt))
            {
                return BadRequest("sorry your otp is incorrect");
            }

            //B4 check thời gian otp hết hạn chưa (2'):
            if (new_otp_1.LimitTimeToSendOtp < DateTime.Now)
            {
                return BadRequest("otp exipre");
            }

            //B5 enable account
            var user = _context.Users.FirstOrDefault(u => u.Email == new_otp_1.Email);
            if (user != null)
            {
                user.IsVerified = true;
                await _context.SaveChangesAsync();
            }

            //B6 xoá hết các trường otp theo email trong bảng otp:
            var OtpDelete = _context.Otps.Where(O => O.Email == new_otp_1.Email);
            _context.Otps.RemoveRange(OtpDelete);
            _context.SaveChanges();

            return Ok("your account is enable");
        }

        /*
         những dòng gạch xanh warning là vì ta chưa check xem nó có null hay ko?
         */

        [HttpPost("send_again_otp")]
        public async Task<IActionResult> sent_againt_otp(string email)
        {
            // B1 tạo một biến chứa số lượng limit request otp/day
            int limit = 5;

             byte[] otp_hash;
             byte[] otp_hash_salt;

            //B2 lấy ip address
            
            /*
             var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            }
             */

            //B3 lấy ra cái otp cũ mới nhất đc lưu trong DB:
            var old_otp = _context.Otps.Where(o =>  o.Email == email )
                .OrderByDescending(o => o.Id)
                .FirstOrDefault();

            var user1 = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user1 == null && old_otp == null)
            {
                return BadRequest("your account is not exist, please register account");
            }

            if ( user1.IsVerified == true )
            {
                return BadRequest("your account was enable");
            }

            // B4 Check số lần truy cập lấy mã otp trong 1 ngày đã vượt quá 5 lần chưa?
            if (old_otp.OtpSpamNumber > limit )
            {
                // check: nếu đã qua một ngày cho phép gửi 5 lần otp nữa 
                if (old_otp.OtpSpam < DateTime.Now)
                {
                    old_otp.OtpSpam = DateTime.Now.AddDays(1);
                    old_otp.OtpSpamNumber = 1;
                    // Tạo otp mới và gửi luôn cho client trong email:
                    string otp_newest = create_otp();
                    _emailService.sentOtp( old_otp.Email, otp_newest );
                    // lưu lại otp mới trong bảng otp:
                    CreateOtpHash(otp_newest, out otp_hash, out otp_hash_salt);
                    old_otp.Otphash = otp_hash;
                    old_otp.OtphashSalt = otp_hash_salt;
                    old_otp.LimitTimeToSendOtp = DateTime.Now.AddMinutes(1);
                    await _context.SaveChangesAsync();
                    return Ok("Otp sent again your email");
                }
                else
                {
                    // check: nếu chưa qua một ngày -> đang spam email
                    return BadRequest("the number you sent otp is limit please wait to 24 hour next");
                }
            }

            // B5: vì cách 2' ta ms đc gửi otp 1 lần -> check xem đã qua 2' chưa:
            if ( old_otp.LimitTimeToSendOtp > DateTime.Now )
            {
                return BadRequest("Please slow operation, you need wait 1 minutes to sent email ");
            }

            //B6 Cho gửi Otp - tạo otp có hiệu lực trong vòng 2':
            string otp_crea = create_otp();
            _emailService.sentOtp(old_otp.Email, otp_crea);
            CreateOtpHash( otp_crea, out otp_hash, out otp_hash_salt);
            old_otp.Otphash = otp_hash;
            old_otp.OtphashSalt = otp_hash_salt;
            old_otp.OtpSpamNumber = old_otp.OtpSpamNumber + 1;
            old_otp.LimitTimeToSendOtp = DateTime.Now.AddMinutes(2);
            _context.SaveChanges();
            return Ok("Otp sent again your email");
        }
        /*
         2 biến out byte[], out byte[] được khai báo trong hai khối if khác nhau, 
        nhưng chúng vẫn nằm trong cùng một phạm vi hàm, 
        nên bạn không thể khai báo lại chúng!
        -> ta sẽ chỉ có thể khai báo ở đầu hàm: byte[] variable
        và sử dụng out variable khi cần ...
         */

        [HttpPost("Login")]
        [EnableRateLimiting("fixedWindow")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {

            try
            {
                var User_new = await _context.Users.FirstOrDefaultAsync(user => user.Email == request.Email);

                if (User_new == null)
                {
                    return BadRequest("email or password is not correct");
                }

                if (User_new.IsVerified == false)
                {
                    return BadRequest("Please resend the otp code and check it in your email to activate your account");
                }

                if (!VerifyPasswordHash(request.Password, User_new.PasswordHash, User_new.PasswordSalt)) // this method return false -> change true and run condition
                {
                    return BadRequest("email or password is not correct");
                }

                // Hàm Verify là để xử lý cái này!
                //  if ( User.VerifiedAt == null )   -> ta co the trien khai logic ham verify thanh OTP 
                //{
                //  return BadRequest("not verify"); // ham verify nay chi chay 1 lan duy nhat khi ta moi tao tai khoan
                //}

                string token = CreateToken(User_new);

                var refreshToken = GenerateRefreshToken();

                SetRefreshToken(refreshToken, User_new); // set refreshToken vao http only cookie

                return Ok(new
                {
                    user = new
                    {
                        id = User_new.Id,
                        fullname = User_new.Fullname,
                        email = User_new.Email,
                        role = User_new.Role,
                    },
                    Access_token = token
                });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        /*
         the ideal is:
        upon registration u send email to the user with this token  -> or even better the complete URL anywhere for client take their token and send on parameter...
         */

        /*  // lan dau tien khi dang nhap ta se phai verify token -> lan sau dang nhap ko phai verify!
          [HttpPost("verify")]
          public async Task<IActionResult> Verify(string token)
          {
              var User = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

              if (User == null)
              {
                  return BadRequest("invalid token");
              }

              // when we check token we will update when time verified token 
             User.VerifiedAt = DateTime.UtcNow;
              await _context.SaveChangesAsync();

              return Ok("User verified! ");

          }

          */


        [HttpPost("refresh-token")]
        [EnableRateLimiting("fixedWindow")]
        public async Task<IActionResult> RefreshToken(int id)
        {


            try
            {
                // lấy user trong db
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);

                // lấy cookie trong request...
                var refreshToken = Request.Cookies["refreshToken"];

                if (refreshToken == null)
                {
                    return Unauthorized("refresh token empty");
                }

                if (user.RefreshToken != refreshToken)
                {
                    return Unauthorized("invalid refresh token");
                }
                else if (user.TokenExpired < DateTime.Now) // ta se so sanh voi time da cap cho refreshToken
                {

                    return Unauthorized("token expired");

                }

                string token = CreateToken(user);

                var newRefreshToken = GenerateRefreshToken();

                SetRefreshToken(newRefreshToken, user);

                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        


        // chưa triển khai
        [HttpPost("forgot-password")]
        public async Task<IActionResult> forgotPassword(string email)
        {

            try
            {
                var User = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (User == null)
            {
                return BadRequest("password sent to email");
            }

            /*
             send the email new password or resetToken through gmail
            SEND EMAIL:
             */
            
            // when we check token we will update when time verified token 
            User.PasswordResetToken = CreateRandomToken();
            User.ResetTokenExpires = DateTime.Now.AddDays(1); // reset token cho 1 day
            await _context.SaveChangesAsync();

            return Ok("password & resetToken sent to email ");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {

            try
            {
            // thay vi gui ma passwordresettoken -> ta se gui 1 doan ma otp
            var User = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);

            // neu dung ma otp -> cho nguoi dung tao mat khau moi!
            if (User == null || User.ResetTokenExpires < DateTime.Now)
            {
                return BadRequest("Invalid token");
            }


            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            User.PasswordHash = passwordHash;
            User.PasswordSalt = passwordSalt;
            User.PasswordResetToken = null;
            User.ResetTokenExpires = null;


            await _context.SaveChangesAsync();

            return Ok("password successfully reset ");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        /*
           [HttpGet, Authorize(Roles = "User")]
        [Route("get-cart")]
        public IActionResult get_cart(int userId)
        {
            try
            {
                
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if (!identity.IsAuthenticated)
                {
                    return Unauthorized();
                }
                // TA DA CAU HINH LAI ClaimTypes.NameIdentifier -> khi thuc hien cau hinh ACCESS TOKEN co truong "ClaimTypes.NameIdentifier"
                var u_id = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // neu ko co tra ve ngoai le chu ko loi
                int User_1 = Convert.ToInt32(u_id);

                if (update.Id != User_1)
                {
                    return Forbid("you are not permission");
                }
         */


        [HttpPost("log_out"), Authorize(Roles = "USER, Admin, STAFF")]
        [EnableRateLimiting("fixedWindow")]
        public async Task<IActionResult> logout(int id)
        {

            try
            {

                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if (!identity.IsAuthenticated)
                {
                    return Unauthorized();
                }
                // TA DA CAU HINH LAI ClaimTypes.NameIdentifier -> khi thuc hien cau hinh ACCESS TOKEN co truong "ClaimTypes.NameIdentifier"
                var u_id = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // neu ko co tra ve ngoai le chu ko loi
                int User_1 = Convert.ToInt32(u_id);

                if (id != User_1)
                {
                    return Forbid("you are not permission");
                }

                // XOA REFRESHTOKEN TRONG DB:
                var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == id);
                
                if (user == null)
                {
                    return Forbid("userId is not exist");
                }

            // XOA REFREH TOKEN TRONG COOKIE:
            user.RefreshToken = null;
            user.TokenCreated = null;
            user.TokenExpired = null;
            _context.SaveChanges();


            //Cách 1: nếu xoá 1 data in cookie the này chỉ xoá cookie từ phản hồi ... ko phải từ yêu cầu ( nếu ta truy cập cookie trong 1 vùng sau khi gọi delete cookie vẫn sẽ có sẵn cho đến khi req over )
            // Response.Cookies.Delete("refreshToken");

            //cách2 replace 1 cookie mới ... 
            var cookieOption = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-1),
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = false
            };

            Response.Cookies.Append("refreshToken", "", cookieOption);

            return Ok( new
            {
                status = 200,
                message = "Logout success"
            } );
            }catch(Exception ex) {
                return BadRequest(ex.Message);
            }
        }


        /*  [HttpPost("send-mail")]
        public IActionResult testMail(EmailModel request)
        {
            try
            {
                // lấy IP của người dùng:
                var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (string.IsNullOrEmpty(ipAddress))
                 {
                     ipAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                  }

                
                 // “::1”, tương đương với 127.0.0.1 trong IPv4.
                 //   Nếu bạn muốn kiểm tra xem API có thể lấy được địa chỉ IP thực sự của client qua internet hay không sd postman
                 

                // send email:
                 _emailService.SendEmail(request);

               return Ok("sent email success");

           }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }*/


        

        [HttpPost, Authorize(Roles = "USER")]
        [EnableRateLimiting("fixedWindow")]
        [Route("update-profile")]
        public IActionResult update_profile(Profile update)
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if (!identity.IsAuthenticated)
                {
                    return Unauthorized();
                }
                // TA DA CAU HINH LAI ClaimTypes.NameIdentifier -> khi thuc hien cau hinh ACCESS TOKEN co truong "ClaimTypes.NameIdentifier"
                var u_id = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // neu ko co tra ve ngoai le chu ko loi
                int User_1 = Convert.ToInt32(u_id);

                if (update.Id != User_1)
                {
                    return Forbid("you are not permission");
                }
                var user = _context.Users.FirstOrDefault( u => u.Id == update.Id);

                if (user == null)
                {
                    return NotFound();
                }
                if ( user.IsVerified == false || user.Role == "STAFF" || user.Role == "Admin")
                {
                    return Forbid();
                }
                

                user.Avatar = "https://cdn.pixabay.com/photo/2013/07/13/12/07/avatar-159236_640.png";
                user.Tel = update.tel;
                user.Address = update.address;
                user.City = update.city;

                _context.SaveChanges();

                return Ok("update success");

            }
            catch ( Exception ex )
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet, Authorize(Roles = "USER")]
        [EnableRateLimiting("fixedWindow")]
        [Route("get-profile")]
        public IActionResult get_profile(int userId)
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if (!identity.IsAuthenticated)
                {
                    return Unauthorized();
                }
                // TA DA CAU HINH LAI ClaimTypes.NameIdentifier -> khi thuc hien cau hinh ACCESS TOKEN co truong "ClaimTypes.NameIdentifier"
                var u_id = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // neu ko co tra ve ngoai le chu ko loi
                int User_1 = Convert.ToInt32(u_id);

                if (userId != User_1)
                {
                    return Forbid("you are not permission");
                }
                var user = _context.Users.FirstOrDefault(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound();
                }
                if (user.IsVerified == false || user.Role == "STAFF" || user.Role == "Admin")
                {
                    return Forbid();
                }

                return Ok( new DTOs.User_DTO.profile()
                {
                    Fullname = user.Fullname,
                    Email = user.Email,
                    tel= user.Tel == null?"": user.Tel,
                    address = user.Address == null ? "" : user.Address,
                    city = user.City == null ? "" : user.City
                });

            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        // CÁC FUNCTION VIẾT Ở ĐÂY:

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken()
            {

                refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                TokenCreated = DateTime.Now,
                TokenExpired = DateTime.Now.AddDays(7),

            };
            return refreshToken;

        }

        private void SetRefreshToken(RefreshToken new_refreshToken, Entities.User User)
        {
            // set options cho https only cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = new_refreshToken.TokenExpired
            };

            // append data into the cookie in object req & append the options for cookies:
            Response.Cookies.Append("refreshToken", new_refreshToken.refreshToken, cookieOptions);

            User.RefreshToken = new_refreshToken.refreshToken;
            User.TokenCreated = new_refreshToken.TokenCreated;
            User.TokenExpired = new_refreshToken.TokenExpired;
            _context.SaveChanges();
            // sau buoc nay cac request sau se -> lay refreshToken trong cookie vao check!

        }


        private string CreateToken(Entities.User user)
        {
            // TẠO YÊU CẦU CLAIM (ĐK)
            List<Claim> claims_user = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // DOAN NAY CAN SUA VI NO QUAN TRONG -> TA SE XAC THUC NGUOI DUNG 
                new Claim( ClaimTypes.Email, user.Email),
                new Claim( ClaimTypes.Role, user.Role ),
                
            };

            // TẠO SECRET-KEY
            // take string scretkey in file appsetting.json:
            var secretKeyByte = Encoding.UTF8.GetBytes(_configuration.GetSection("ConnectionStrings:SecretKey").Value);
            //use type "bytes" to built a secret key 
            var scretKey = new SymmetricSecurityKey(secretKeyByte);

            // algorithms encode jwt:
            var cred = new SigningCredentials(scretKey, SecurityAlgorithms.HmacSha512Signature);

            //TẠO TOKEN
            // tải thư viện: System.IdentifyModel.Tokens.Jwt;
            var new_token = new JwtSecurityToken(
            claims: claims_user,
            expires: DateTime.Now.AddHours(10), // ALTER TIME EXPIRE
            signingCredentials: cred
            );

            // CÓ TOKEN -> TAO JWT:
            var jwt = new JwtSecurityTokenHandler().WriteToken(new_token);

            return jwt;
        }




        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using ( var hmac = new HMACSHA512() )
            {
                // key will random generate key
                // hmac.Key -> take a 124 bytes OVERPASS the Bytes(64) in DB so we will cut the sring to take 64 bytes or edit column passwordSalt in DB has type Bytes(64)
                passwordSalt = hmac.Key;
              // passwordSalt = hmac.Key.Take(64).ToArray();
                // hash password
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }


        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                // computed hash
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                // check the new connected hash and password are the same 

                // sequeceEqual sẽ nhận vào mảng byte
                return computedHash.SequenceEqual(passwordHash);
            }
        }


        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        private string create_otp()
        {
            Random random = new Random();
            int number = random.Next(10000, 100000);
            string randomString = number.ToString();
            return randomString;
        }

       private void CreateOtpHash(string otp, out byte[] otp_hash, out byte[] otp_hash_salt)
        {
            using (var hmac = new HMACSHA512())
            {
                otp_hash_salt = hmac.Key;
                otp_hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(otp));
            }
        }


        private bool VerifyOtpHash(string otp,  byte[] Otphash, byte[] OtphashSalt)
        {
            using (var hmac = new HMACSHA512(OtphashSalt))
            {
                var computedHash = hmac.ComputeHash( Encoding.UTF8.GetBytes(otp) );
                return computedHash.SequenceEqual(Otphash);
            }
        }

















    }
}
