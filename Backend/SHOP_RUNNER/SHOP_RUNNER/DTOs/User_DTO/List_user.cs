namespace SHOP_RUNNER.DTOs.User_DTO
{
    public class List_user
    {
        public int Id { get; set; }

        public string Fullname { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Role { get; set; } = null!;

        public bool IsActived { get; set; }
    }
}
