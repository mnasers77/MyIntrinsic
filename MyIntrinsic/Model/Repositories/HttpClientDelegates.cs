using System.Net;

namespace MyIntrinsic.Services.Networking
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        public AuthenticationDelegatingHandler(ILoginService loginService, DataMngr dataMngr)
        {
            _loginService = loginService;
            _dataMngr = dataMngr;
        }

        private readonly ILoginService _loginService;
        private readonly DataMngr _dataMngr;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = GetTokenAsync();
            //request.Headers.Authorization = new AuthenticationHeaderValue(token.Scheme, token.AccessToken);

            request.Headers.Add(token.Scheme, token.AccessToken);
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                token = await RefreshTokenAsync();
                //request.Headers.Authorization = new AuthenticationHeaderValue(token.Scheme, token.AccessToken);

                if (string.IsNullOrEmpty(token.AccessToken))
                {
                    response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                else
                { 
                    request.Headers.Add(token.Scheme, token.AccessToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }

        private Token GetTokenAsync()
        {
            var scheme = "authorizationToken";
            var access_token = _loginService.AuthToken; 

            var token = new Token { Scheme = scheme, AccessToken = access_token };
            return token;
        }

        private async Task<Token> RefreshTokenAsync()
        {
            var token = await _loginService.RefreshTokenAsync();
            return token;
        }
    }

    public class Token 
    {
        public string Scheme { get; set; }
        public string AccessToken { get; set; }
    }

}
