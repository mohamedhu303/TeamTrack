using TeamTrack.Models;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user);
}
