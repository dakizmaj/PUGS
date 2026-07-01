using AuthService.Dtos;
using AuthService.Models;

namespace AuthService.Mapping
{
    public static class UserMapper
    {
        public static UserResponseDto ToResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }
    }
}