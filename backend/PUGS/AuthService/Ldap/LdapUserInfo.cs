namespace AuthService.Ldap
{
    public class LdapUserInfo
    {
        public string Uid { get; set; } = string.Empty;
        public string CommonName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}