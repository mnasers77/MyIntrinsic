using MyIntrinsic.Services.Networking;

namespace MyIntrinsic.Services;

public interface ILoginService
{
    string AuthToken { get;}
    public Task<(bool, string)> LoginAsync(string username, string password);
    public Task<(bool, string)> LogoutAsync();
    public Task<Token> RefreshTokenAsync();
}
