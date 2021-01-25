using System.Security.Claims;

namespace CometaCloudS01.TokenAuthentication
{
    public interface ITokenManager
    {
        bool Authenticate(string username, string password);
        string NewToken();
        ClaimsPrincipal VerifyToken(string token);
    }
}