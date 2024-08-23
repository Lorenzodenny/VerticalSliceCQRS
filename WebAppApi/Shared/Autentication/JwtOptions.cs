using Microsoft.IdentityModel.Tokens;
using System.Text;

public class JwtOptions
{
    public string Issuer { get; set; } // Quello che emette il JWT
    public string Audience { get; set; } // IL pubblico a cui è destinato il JWT
    public SymmetricSecurityKey SecurityKey { get; set; } // la chiave segreta che definisce la sicurezza del JWT

    public void Configure(IConfiguration configuration)
    {
        Issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("L'emittente deve essere impostato nella configurazione.");
        Audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Il ricevente deve essere impostato nella configurazione.");
        string key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("La chiave deve essere impostata nella configurazione.");
        SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

}
