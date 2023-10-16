using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.Email;
using SHOP_RUNNER.Models.Users;
using SHOP_RUNNER.Services.EmailService;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {

            try
            {
                if ( _context.Users.Any(u => u.Email == request.Email) )
                    {
                        return BadRequest("user already exist ");
                    }


                //hanlde otp send otp throught EMAIL for user:



 // create hashPassword throught user's password:
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
             await _context.SaveChangesAsync();

            // return Ok(new { message = passwordHash, message1 = passwordSalt });
            return Ok("create user success");
            }
            catch (Exception ex)
            {
                return BadRequest( ex.Message );

            }
        }




        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {

            try
            {
                var User_new = await _context.Users.FirstOrDefaultAsync(user => user.Email == request.Email);

                if (User_new == null)
                {
                    return BadRequest("user not found");
                }

                if (!VerifyPasswordHash(request.Password, User_new.PasswordHash, User_new.PasswordSalt)) // this method return false -> change true and run condition
                {
                    return BadRequest("password is not correct");
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


        [HttpPost("log_out")]
        public async Task<IActionResult> logout(int id)
        {

            try
            {
            // XOA REFRESHTOKEN TRONG DB:
            var user = await _context.Users.SingleOrDefaultAsync(user => user.Id == id);


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
                Secure = true
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

      //  [HttpPost("send-mail")]
      //  public IActionResult testMail(EmailModel request)
      //  {
        //    try
          //  {
                // lấy IP của người dùng:
           //      var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
           //     if (string.IsNullOrEmpty(ipAddress))
            //     {
                   //  ipAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
               //   }

                /*
                  “::1”, tương đương với 127.0.0.1 trong IPv4.
                    Nếu bạn muốn kiểm tra xem API có thể lấy được địa chỉ IP thực sự của client qua internet hay không sd postman
                 */

                // send email:
                // _emailService.SendEmail(request);

          //      return Ok(ipAddress);

       //     }catch(Exception ex)
       //     {
              //  return BadRequest(ex.Message);
       //     }
           
       // }

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
                new Claim("Id", user.Id.ToString()),
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
            expires: DateTime.Now.AddDays(1),
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

    }
}
