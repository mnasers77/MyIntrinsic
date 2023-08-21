using MyIntrinsic.Services.Networking;

namespace MyIntrinsic.Services;
public class LoginService : ILoginService
{
    public string AuthToken { get => _authToken; }
    public LoginService(DataMngr dataMngr, IConnectivity connectivity, IHttpClientFactory clientFactory)
    {
        _dataMngr = dataMngr;
        _httpClientFactory = clientFactory;
        _connectivity = connectivity;   
        //_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    DataMngr _dataMngr;
    IConnectivity _connectivity;
    IHttpClientFactory _httpClientFactory;

    public string _authToken = String.Empty;
    public string _refreshToken = String.Empty;
    public DateTime _loginTime = DateTime.MinValue;

    string loginUrl = "https://9f6mnvq2ph.execute-api.us-east-1.amazonaws.com/Prod/signin"; //POST
    
    public async Task<(bool, string)> LoginAsync(string username, string password)
    {
        var result = false; 
        var resultMessage = string.Empty;

        if(_connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            result = false;
            resultMessage = "Internet not available";
            return (result, resultMessage);
        }

        var url = new Uri(loginUrl);

        var loginCredentials = new LoginCredentials() {username = username, password = password }; 

        var json = JsonSerializer.Serialize(loginCredentials);
        
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.PostAsync(url, stringContent);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var responseJson = JsonSerializer.Deserialize<LoginResponse>(content);
                if (responseJson is not null)
                {
                    _authToken = responseJson.data.AuthenticationResult.IdToken;
                    _refreshToken = responseJson.data.AuthenticationResult.RefreshToken;
                    _loginTime = DateTime.Now;
                }
            }
            else
            {
                Debug.WriteLine(response.StatusCode.ToString());
            }

            result = response.IsSuccessStatusCode;
            resultMessage = response.StatusCode.ToString(); 

        }
        catch(Exception ex)
        {
            result = false; 
            resultMessage = ex.Message;

            _authToken = string.Empty;
            _refreshToken = string.Empty;
            _loginTime = DateTime.MinValue;
        }

        return (result, resultMessage);
    }

    public async Task<(bool, string)> LogoutAsync()
    {
        await Task.Delay(1000);
        await _dataMngr?.LogoutAsync();
        return (true, "Success");
    }

    public async Task<Token> RefreshTokenAsync()
    {
        var loginCredentials = await _dataMngr.GetLoginInformationFromSecureStorageAsync();
        var resultTuple = await LoginAsync(loginCredentials.username, loginCredentials.password);

        return new Token() { Scheme = "authorizationToken", AccessToken = _authToken };
    }
}