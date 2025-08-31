namespace AutomationLetterWriting.DTOs
{
    public class RegisterDto
    {
        public string UserName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
