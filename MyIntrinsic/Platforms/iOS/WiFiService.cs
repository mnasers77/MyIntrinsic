using Microsoft.Extensions.Logging;

namespace MyIntrinsic.Services
{
    public partial class WiFiService
    {
        public WiFiService(ILogger<WiFiService> logger)
        {
            _logger = logger;
        }

        public partial Task<string> GetConnectedWiFiNetworkSSIDAsync()
        {
            throw new NotImplementedException();
        }

        public partial Task<bool> IsWiFiEnabledAsync()
        {
            throw new NotImplementedException();
        }

        public partial Task<List<string>> GetDetectedWiFiNetworkSSIDsAsync()
        {
            var networkSSIDs = new List<string>();
            return Task.FromResult(networkSSIDs);
        }

        public partial Task<(bool, string)> ConnectToWiFiNetworkAsync(string ssid, string password)
        {
            throw new NotImplementedException();
        }

        public partial Task<bool> DisconnectFromWiFiNetworkAsync(string ssid)
        {
            throw new NotImplementedException();
        }
    }
}