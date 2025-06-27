namespace Wra10Core2023.Models
{
    public class LoginResult
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Token { get; set; }
    }

    
    public class LoginModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
    public class User
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? IP { get; set; }
    }
}
