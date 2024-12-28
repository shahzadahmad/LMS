namespace LMS.Application.DTOs
{
    public class CreateUserDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public List<RoleDTO> Roles { get; set; }
    }
}
