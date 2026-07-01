namespace AuthService.Dtos
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserResponseDto User { get; set; } = new();
    }
}