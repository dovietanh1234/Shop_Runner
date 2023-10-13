using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.Users;
using System.IdentityModel.Tokens.Jwt;
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
        public UsersController(RunningShopContext context, IConfiguration configuration) {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            
            
            if( _context.Users.Any(u => u.Email == request.Email) )
            {
                return BadRequest("user already exist ");
            }


 // create hashPassword throught user's password:
            // "out" is mean: we will take the values likes THAM CHIẾU ( khi một biến thay đổi thì giá trị ở chỗ khác cx bị thay đổi => tham chiếu "out" )
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);



            var user_n = new Entities.User
            {
                Fullname = request.FullName,
                Email = request.Email,
                Role = "USER",
                PasswordSalt = passwordSalt,  //Encoding.UTF8.GetBytes("fghjktyuifvn");
                PasswordHash = passwordHash,
                VerificationToken = CreateRandomToken()
            };

             _context.Users.Add(user_n);
             await _context.SaveChangesAsync();

            // return Ok(new { message = passwordHash, message1 = passwordSalt });
            return Ok("create user success");

        }











        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var User = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (User == null)
            {
                return BadRequest("user not found");
            }

            if ( !VerifyPasswordHash(request.Password, User.PasswordHash, User.PasswordSalt)) // this method return false -> change true and run condition
            {
                return BadRequest("password is not correct");
            }

            if ( User.VerifiedAt == null )
            {
                return BadRequest("not verify"); // ham verify nay chi chay 1 lan duy nhat khi ta moi tao tai khoan
            }

            string token = CreateToken(User);

            return Ok(token);

        }



        /*
         the ideal is:
        upon registration u send email to the user with this token  -> or even better the complete URL anywhere for client take their token and send on parameter...
         */

        // lan dau tien khi dang nhap ta se phai verify token -> lan sau dang nhap ko phai verify!
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






        [HttpPost("forgot-password")]
        public async Task<IActionResult> forgotPassword(string email)
        {
            var User = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (User == null)
            {
                return BadRequest("user not found");
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



        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
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
