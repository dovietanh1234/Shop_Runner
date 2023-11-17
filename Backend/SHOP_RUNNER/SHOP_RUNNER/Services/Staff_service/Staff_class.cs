using SHOP_RUNNER.DTOs.User_DTO;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.Staff_model;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;

namespace SHOP_RUNNER.Services.Staff_service
{
    public class Staff_class : IStaff_repo
    {

        private readonly RunningShopContext _context;

        public Staff_class(RunningShopContext context)
        {
            _context = context;
        }

        // tạo tk cho staff ( thêm sửa xoá tài khoản staff ) -> quyền admin sẽ tạo tài khoản
        // đăng nhập được bằng tài khoản -> login bthg 


        // staff -> toggle user account
        // -> toggle product

        public async Task<DTO_staff_regis> Register_account(Staff_register request)
        {

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);


            string random_n = await RandomString();

            string email2 = $"staff{random_n}@gmail.com";

            var user_n = new Entities.User
            {
                Fullname = request.FullName,
                Email = email2,
                Role = "STAFF",
                PasswordSalt = passwordSalt,  //Encoding.UTF8.GetBytes("fghjktyuifvn");
                PasswordHash = passwordHash,
                IsVerified = true
            };

            _context.Users.Add(user_n);
            _context.SaveChanges();

            return new DTO_staff_regis()
            {
                email = email2,
                password = request.Password
            };
        }

        public int change_password(Staff_changePass request)
        {
            var user = _context.Users.FirstOrDefault( u => u.Id == request.Id );

            if(user == null)
            {
                return 404;
            }

            if (user.IsVerified == false)
            {
                return 403;
            }

            // so sanh password cu xem co dung ko:

            if (!VerifyPasswordHash(request.Password_old, user.PasswordHash, user.PasswordSalt)) // this method return false -> change true and run condition
            {
                return 401;
            }

            CreatePasswordHash(request.Password_new, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            _context.SaveChanges();
            return 200;
        }

        public List<List_user> list_user()
        {
            var User = _context.Users.Where( u => u.Role == "USER").ToList();

            List<List_user> Dto = new List<List_user>();

            foreach (var user in User)
            {
                Dto.Add(new List_user()
                {
                    Id = user.Id,
                    Fullname = user.Fullname,
                    Email = user.Email,
                    Role = user.Role,
                    IsActived = user.IsVerified
                });
            }

            return Dto;

        }

        public int toggleUser(int userId)
        {
            var user = _context.Users.FirstOrDefault( u => u.Id == userId );

            if (user == null)
            {
                return 404;
            }

            // nếu là role là staff or admin thì ko thể tắt được:
            if ( user.Role == "STAFF" || user.Role == "Admin" )
            {
                return 404;
            }

            if ( user.IsVerified == true )
            {
                user.IsVerified = false;
            }
            else
            {
                user.IsVerified = true;
            }

            _context.SaveChanges();
            return 200;


        }

        // toggle san pham da lam ben kia:


        











        private static async Task<string> RandomString(int length = 6)
        {
            Random random = new Random();
            string character = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            // task chỉ sử dụng trong trường hợp liên quan đến việc tính toán một việc nhất định ...
            // hoặc là những việc liên quan đến độ trễ của DB nhưng ko nhớ ví dụ 
            Task<string> task = Task.Run(() => new string(Enumerable.Repeat(character, length).Select(s => s[random.Next(s.Length)]).ToArray()));

            string result = await task;

            return result;
        }







        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
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

// 403 forbid
// 404 ko tim thay
// 401 not authentication