
//Windows implementation of WiFiService

using Microsoft.Extensions.Logging;
using Windows.Devices.Radios;
using Windows.Devices.WiFi;
using Windows.Security.Credentials;

namespace MyIntrinsic.Services
{
    public partial class WiFiService
    {
        public WiFiService(ILogger<WiFiService> logger)
        {
            _logger = logger;
        }

        WiFiAdapter? _wifiAdapter = null;
        List<WiFiAdapter> _wifiAdapters = new List<WiFiAdapter>();

        public partial async Task<bool> IsWiFiEnabledAsync()
        {
            bool result = false;

            var radios = await Radio.GetRadiosAsync();

            foreach (var radio in radios)
            {
                result |= radio.State == RadioState.On;
            }

            return result;
        }

        public partial async Task<List<string>> GetDetectedWiFiNetworkSSIDsAsync()
        {
            List<string> networkSSIDs = new List<string>();

            //Get wifi adapter
            var adapter = await GetWiFiAdapterAsync();

            if (adapter == null)
            {
                return networkSSIDs;
            }

#pragma warning disable CA1416 // Validate platform compatibility
            await adapter.ScanAsync();
#pragma warning restore CA1416 // Validate platform compatibility

            foreach (var network in adapter.NetworkReport.AvailableNetworks)
            {
                if ((!networkSSIDs.Contains(network.Ssid)) && (network.Ssid != String.Empty))
                {
                    networkSSIDs.Add(network.Ssid);
                }
            }

            return networkSSIDs;
        }

        public partial async Task<string> GetConnectedWiFiNetworkSSIDAsync()
        {
            var ssid = string.Empty;
            WiFiAdapter? wifiAdapter = null;

            var adapters = await GetWiFiAdaptersAsync();
            if (adapters.Count > 0)
            {
                wifiAdapter = adapters[0];
            }

            if (wifiAdapter != null)
            {
                var connectedProfile = await wifiAdapter.NetworkAdapter.GetConnectedProfileAsync();
                if (connectedProfile != null && connectedProfile.IsWlanConnectionProfile && connectedProfile.WlanConnectionProfileDetails != null)
                {
                    ssid = connectedProfile.WlanConnectionProfileDetails.GetConnectedSsid();
                    var security = connectedProfile.NetworkSecuritySettings.NetworkAuthenticationType;
                }
            }

            return ssid;
        }

        public partial async Task<(bool, string)> ConnectToWiFiNetworkAsync(string ssid, string password)
        {
            bool result = false;
            string resultMessage = string.Empty;
            WiFiConnectionResult connectionResult = null; 

            if (ssid == String.Empty)
            {
                return (result, resultMessage);
            }

            var connectedSSID = await GetConnectedWiFiNetworkSSIDAsync();

            //See if already connected to WiFi network
            if (connectedSSID == ssid)
            {
                result = true;
                resultMessage = $"Already connected to network {ssid}";
                return (result, resultMessage);
            }

            var wifiAdapter = await GetWiFiAdapterAsync();
            if (wifiAdapter != null)
            {
                //Find network profile with SSID
                var network = await GetWiFiAvailableNetworkAsync(ssid);
                if (network != null)
                {
                    try
                    {

                        //try to connect without password
                        if (String.IsNullOrEmpty(password))
                        {
                            connectionResult = await wifiAdapter.ConnectAsync(network, WiFiReconnectionKind.Manual);
                        }
                        else
                        {
                            var passwordCredential = new PasswordCredential();
                            passwordCredential.Password = password;
                            connectionResult = await wifiAdapter.ConnectAsync(network, WiFiReconnectionKind.Manual, passwordCredential);
                        }

                        if (connectionResult.ConnectionStatus == WiFiConnectionStatus.Success)
                        {
                            result = true;
                            resultMessage = "success";
                        }
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        resultMessage = ex.Message;
                        _logger?.LogError($"Exception connecting to wifi network {ssid}: {ex.Message}");
                    }
                }
            }

            return (result, resultMessage);
        }

        public partial async Task<bool> DisconnectFromWiFiNetworkAsync(string ssid)
        {
            var result = false;

            //if ssid is not empty, check to see if we are already connected
            if (!ssid.Equals(string.Empty))
            {
                var connectedSSID = await GetConnectedWiFiNetworkSSIDAsync();
                result = true;
                if (connectedSSID == ssid)
                {
                    return result;
                }
            }

            var wifiAdapter = await GetWiFiAdapterAsync();

            if (wifiAdapter != null)
            {
                try
                {
                    wifiAdapter.Disconnect();
                    result = true;
                }
                catch (Exception ex)
                {
                    result = false;
                    _logger?.LogError($"Exception disconnecting from wifi network: {ex.Message}");
                }
            }

            return result;
        }

        //Get network profile with passed in ssid for passed in adapter
        async Task<WiFiAvailableNetwork> GetWiFiAvailableNetworkAsync(string ssid)
        {
            WiFiAvailableNetwork result = null;

            var wifiAdapter = await GetWiFiAdapterAsync();

            if ((wifiAdapter != null) && (ssid != String.Empty))
            {
                foreach (var network in wifiAdapter.NetworkReport.AvailableNetworks)
                {
                    if (network.Ssid == ssid)
                    {
                        result = network;
                    }
                }
            }

            return result;
        }

        async Task<List<WiFiAdapter>> GetWiFiAdaptersAsync()
        {
            if (_wifiAdapters.Count == 0)
            {
                try
                {
                    //Find all wifi adapters 
                    var adapters = await WiFiAdapter.FindAllAdaptersAsync();
                    if (adapters != null)
                    {
                        _wifiAdapters = adapters.ToList<WiFiAdapter>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception returning WiFi adapters: {ex.Message}");
                }
            }

            return _wifiAdapters;
        }

        async Task<WiFiAdapter> GetWiFiAdapterAsync()
        {
            if (_wifiAdapter == null)
            {
                var adapters = await GetWiFiAdaptersAsync();
                if (adapters.Count > 0)
                {
                    _wifiAdapter = adapters[0];
                }
            }

            return _wifiAdapter;
        }


    }
}
