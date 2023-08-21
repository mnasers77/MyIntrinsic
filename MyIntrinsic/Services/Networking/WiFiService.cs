namespace MyIntrinsic.Services
{
    public partial class WiFiService
    {
        ILogger<WiFiService> _logger;

        public partial Task<bool> IsWiFiEnabledAsync();
        public partial Task<List<string>> GetDetectedWiFiNetworkSSIDsAsync();
        public partial Task<string> GetConnectedWiFiNetworkSSIDAsync(); 
        public partial Task<(bool, string)> ConnectToWiFiNetworkAsync(string ssid, string password);
        public partial Task<bool> DisconnectFromWiFiNetworkAsync(string ssid);
    }
}