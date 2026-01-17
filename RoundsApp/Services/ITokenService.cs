using RoundsApp.Models;

namespace RoundsApp.Services;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user);
}
