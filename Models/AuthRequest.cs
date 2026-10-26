namespace RestfulBookerTests.Models
{
    public class AuthRequest
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string token { get; set; } = string.Empty;
    }
}