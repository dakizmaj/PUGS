using System;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AuthService.Ldap
{
    public class LdapAuthenticationService : ILdapAuthenticationService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _usersOu;

        public LdapAuthenticationService(IConfiguration configuration)
        {
            var ldapSettings = configuration.GetSection("Ldap");
            _host = ldapSettings["Host"] ?? "localhost";
            _port = int.Parse(ldapSettings["Port"] ?? "389");
            _usersOu = ldapSettings["UsersOu"] ?? "ou=users,dc=pugs,dc=local";
        }

        public Task<LdapUserInfo?> AuthenticateAsync(string username, string password)
        {
            return Task.Run(() =>
            {
                var userDn = $"uid={username},{_usersOu}";

                try
                {
                    using var connection = new LdapConnection(new LdapDirectoryIdentifier(_host, _port));
                    connection.AuthType = AuthType.Basic;
                    connection.SessionOptions.ProtocolVersion = 3;

                    // Bind sa kredencijalima korisnika - ovo JESTE provera lozinke.
                    // Ako lozinka nije tacna, Bind() baca izuzetak.
                    connection.Credential = new NetworkCredential(userDn, password);
                    connection.Bind();

                    // Ako bind uspe, izvuci podatke o korisniku
                    var searchRequest = new SearchRequest(
                        userDn,
                        "(objectClass=inetOrgPerson)",
                        SearchScope.Base,
                        "cn", "mail", "uid"
                    );

                    var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);

                    if (searchResponse.Entries.Count == 0)
                        return null;

                    var entry = searchResponse.Entries[0];

                    return new LdapUserInfo
                    {
                        Uid = entry.Attributes["uid"]?[0]?.ToString() ?? username,
                        CommonName = entry.Attributes["cn"]?[0]?.ToString() ?? username,
                        Email = entry.Attributes["mail"]?[0]?.ToString() ?? $"{username}@pugs.local"
                    };
                }
                catch (LdapException)
                {
                    // Pogresna lozinka ili korisnik ne postoji - bind ne uspeva
                    return null;
                }
            });
        }
    }
}