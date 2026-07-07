using System.Threading.Tasks;

namespace AuthService.Ldap
{
    public interface ILdapAuthenticationService
    {
        Task<LdapUserInfo?> AuthenticateAsync(string username, string password);
    }
}