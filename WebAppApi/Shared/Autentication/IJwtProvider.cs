using System.Security.Claims;

public interface IJwtProvider
{
    string GenerateToken(IEnumerable<Claim> claims, DateTime expiry);
}