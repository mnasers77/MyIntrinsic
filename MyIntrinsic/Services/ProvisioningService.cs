using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;


namespace MyIntrinsic.Services
{
    public class ProvisioningService
    {
        public const string ConfirmationNotStarted = "0";
        public const string SSIDNotFound = "1";
        public const string ConnectionFailed = "2";
        public const string IPNotAcquired = "3";
        public const string FeedbackFailed = "4";
        public const string ConfirmationSuccess = "5";

        private static readonly Dictionary<string, string> ConfigurationStatusDict = new Dictionary<string, string>()
        {
            {"0", "Confirmation not started"},
            {"1", "SSID not found"},
            {"2", "Connection failed"},
            {"3", "IP not acquired"},
            {"4", "Feedback failed"},
            {"5", "Confirmation success"}
        };

        string _baseUrl = "http://mysimplelink.net";

        //Provisioning API endpoints
        string addWlanProfileUrl = "/api/1/wlan/profile_add"; //POST
        string postConfirmationRequest = "/api/1/wlan/confirm_req"; //POST
        string getConfirmationResult = "/param_cfg_result.txt"; //GET

        //Device settings API endpoints
        string getDeviceVersion = "/param_product_version.txt"; //GET
        string getDeviceNameUrl = "/param_device_name.txt"; //GET
        string postDeviceNameUrl = "/api/1/netapp/set_urn"; //POST
        string postRescanUrl = "/api/1/wlan/en_ap_scan"; //POST
        string getNetworksUrl = "/netlist.txt"; //GET

        int DELAY_AFTER_RESCAN_NETWORKS_BEFORE_FETCHING_NETWORKS = 20000;

        public ProvisioningService(ILogger<ProvisioningService> logger, WiFiService wifiService)
        {
            _logger = logger;
            _wifiService = wifiService;
        }

        ILogger<ProvisioningService> _logger;
        WiFiService _wifiService;
        
        public static async Task<byte[]> PingAsync(string ipAddress)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            //int timeout = 2000;
            PingReply reply = await pingSender.SendPingAsync(ipAddress);
            if (reply.Status == IPStatus.Success)
            {
                Console.WriteLine("Address: {0}", reply.Address.ToString());
                Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                Console.WriteLine("Time to live: {0}", reply.Options?.Ttl);
                Console.WriteLine("Don't fragment: {0}", reply.Options?.DontFragment);
                Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
            }

            return reply.Buffer;
        }

        //Connect to the Wifi network of the device
        public async Task<(bool, string)> ConnectDeviceWifiNetworkAsync(string ssid, string password)
        {
            bool result = false;
            string resultMessage = string.Empty;

            if (_wifiService is null) return (result, resultMessage);

            var currentSelectedNetwork = await _wifiService.GetConnectedWiFiNetworkSSIDAsync();
            if (ssid.Equals(currentSelectedNetwork))
            {
                result = true;
                resultMessage = $"Already connected to {ssid}";
            }
            else
            {
                (result, resultMessage) = await _wifiService.ConnectToWiFiNetworkAsync(ssid, password);
            }

            return (result, resultMessage);
        }

        //Returns list of Wifi networks detected by the charger.  
        public async Task<string> GetWiFiNetworkstDetectedByDeviceAsync(bool rescan = true)
        {
            var deviceNetworks = new List<string>();
            var contentString = string.Empty;
            var result = true;

            //have the device rescan for networks
            if (rescan)
            {
                result = await StartRescanNetworksOnDeviceAsync();
                if (result)
                {
                    //wait for the device to finish scanning
                    await Task.Delay(DELAY_AFTER_RESCAN_NETWORKS_BEFORE_FETCHING_NETWORKS);
                }
            }

            if (result == true)
            {
                contentString = await GetScanResultsFromDeviceAsync();
            }

            return contentString;
        }

        //Returns friendly name of device.
        //Must be connected to the charger's wifi network to call this method.
        public async Task<string> GetConfirmationResultAsync(string baseUrl = "")
        {
            var deviceConfigurationString = string.Empty;

            //if no base url is passed in, use the default (mysimplelink.net)
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = _baseUrl;
            }

            var url = $"{baseUrl}{getConfirmationResult}";

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Successfully recevied device configuration");
                        deviceConfigurationString = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        _logger.LogInformation($"Failed to get device configuration result. Status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get device configuration result");
                }
            }

            return deviceConfigurationString;
        }

        //Returns friendly name of device.
        //Must be connected to the charger's wifi network to call this method.
        public async Task<string> GetDeviceNameAsync()
        {
            var deviceName = string.Empty;

            var url = $"{_baseUrl}{getDeviceNameUrl}";

            //using (var httpClient = new HttpClient(new AndroidMessageHandler()))
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Successfully recevied device name");
                        deviceName = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        _logger.LogInformation($"Failed to get device name. Status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get device name");
                }
            }

            return deviceName;
        }

        //Send profile information about the home Wifi network to the device.
        //Must be connected to the charger's wifi network to call this method.
        public async Task<bool> AddWlanProfileToDeviceAsync(string ssid, string password, SecurityType securityType, string priority)
        {
            bool result = false;
                
            var securityTypeString = ((int)securityType).ToString();


            var url = $"{_baseUrl}{addWlanProfileUrl}";

            using (var httpClient = new HttpClient())
            {
                var form = new Dictionary<string, string>()
                {
                    {"__SL_P_P.A", ssid}, //ssid of home network
                    {"__SL_P_P.B", securityTypeString },   //security type
                    {"__SL_P_P.C", password}, //password
                    {"__SL_P_P.D", priority}   //priority
                };

                var content = new FormUrlEncodedContent(form);

                var response = await httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    result = true;
                    _logger.LogInformation("Successfully transmitted wlan information networks");
                }
                else
                {
                    result = false;
                    _logger.LogInformation($"Failed to send wlan information. Status code: {response.StatusCode}");
                }
            }

            return result;
        }

        //POST request for charger to attempt to connect to the home Wifi network.
        //Must be connected to the charger's wifi network to call this method.
        public async Task<bool> StartConfigurationStageAsync()
        {
            bool result = false;

            var url = $"{_baseUrl}{postConfirmationRequest}";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(url, null);
                if (response.IsSuccessStatusCode)
                {
                    result = true;
                    _logger.LogInformation("Successfully started configuration stage");
                }
                else
                {
                    result = false;
                    _logger.LogInformation($"Failed to start configuration stage. Status code: {response.StatusCode}");
                }
            }

            return result;
        }

        //Sets the friendly name of the device (such as Garage 1)
        //Must be connected to the charger's wifi network to call this method.
        public async Task<bool> SetDeviceNameAsync(string name)
        {
            bool result = false;
            var url = $"{_baseUrl}{postDeviceNameUrl}";

            using (var httpClient = new HttpClient())
            {
                var form = new Dictionary<string, string>()
                {
                    {"__SL_P_S.B", name}, //friendly name of device
                };

                var content = new FormUrlEncodedContent(form);

                var response = await httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    result = true;
                    _logger.LogInformation("Successfully transmitted new name information networks");
                }
                else
                {
                    result = false;
                    _logger.LogInformation($"Failed to send new name information. Status code: {response.StatusCode}");
                }
            }

            return result;
        }

        //Initiates rescan of Wifi networks detected by the charger. 
        //Must be connected to the charger's wifi network to call this method.
        async Task<bool> StartRescanNetworksOnDeviceAsync()
        {
            bool result = false;

            var url = $"{_baseUrl}{postRescanUrl}";

            using (var httpClient = new HttpClient())
            {
                var form = new Dictionary<string, string>()
                {
                    {"__SL_P_SC1", "10"}, //time between scan cycles
                    {"__SL_P_SC2", "1"}   //number of scan cycles
                };

                var content = new FormUrlEncodedContent(form);

                var response = await httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    result = true;
                    _logger.LogInformation("Successfully rescanned networks");
                }
                else
                {
                    result = false;
                    _logger.LogInformation($"Failed to start network rescan. Status code: {response.StatusCode}");
                }
            }

            return result;
        }

        //Returns list of Wifi networks detected by the charger during its last scan.
        //Must be connected to the charger's wifi network to call this method.
        async Task<string> GetScanResultsFromDeviceAsync()
        {
            var contentString = string.Empty;

            var url = $"{_baseUrl}{getNetworksUrl}";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully rescanned networks");
                    contentString = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    _logger.LogInformation($"Failed to get device networks. Status code: {response.StatusCode}");
                }
            }

            return contentString;
        }

        public async Task<string> ReceiveUdpBroadcastFromDeviceAsync(CancellationToken cancellationToken)
        {
            var message = string.Empty;
            using (var udpClient = new UdpClient(1501))
            {
                udpClient.EnableBroadcast = true;

                try
                {
                    var result = await udpClient.ReceiveAsync(cancellationToken);
                    // The received message is in result.Buffer. You can convert it to a string with UTF8 encoding:
                    message = Encoding.UTF8.GetString(result.Buffer);

                    var messageTokens = message.Split(',');
                    // To get the sender's IP address and port:
                    var senderIp = result.RemoteEndPoint.Address;
                    var senderPort = result.RemoteEndPoint.Port;
                }
                catch (TaskCanceledException)
                {
                    _logger.LogError($"Read UDP Broadcast task cancelled exception");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Read UDP Broadcast task exception: {ex.Message}");
                }
            }

            return message;
        }

        public async Task<string> ReceiveMdnsBroadcastAsync(CancellationToken cancellationToken)
        {
            string resultString = string.Empty;
            UdpClient client = null;

            try
            {
                client = new UdpClient(5354);
                var multicastAddress = IPAddress.Parse("224.0.0.251");
                //client.JoinMulticastGroup(multicastAddress);
                client.MulticastLoopback = true;

                var mdnsResult = await client.ReceiveAsync(cancellationToken);
                var dataBytes = mdnsResult.Buffer;
                if (dataBytes != null && dataBytes.Length > 0)
                {
                    resultString = Encoding.UTF8.GetString(dataBytes);
                }
            }
            catch(TaskCanceledException)
            {
                _logger.LogInformation("ReceiveMdnsBroadcast Task cancelled");
                resultString = string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError($"UdpClient exception: {ex.Message}");
                resultString = string.Empty; 
            }
            
            return resultString;
        }

        public async Task<ChargerInfo> DetermineDeviceIPAysnc()
        {
            var deviceInfo = new ChargerInfo();
            string resultString = string.Empty;
            var cts = new CancellationTokenSource();
            var ct = cts.Token; 

            var timeoutTask = Task.Delay(40000, ct);
            var receiveMdnsTask = ReceiveMdnsBroadcastAsync(ct);
            var receiveUdpBroadcastTask = ReceiveUdpBroadcastFromDeviceAsync(ct); 

            var completedTask = await Task.WhenAny(timeoutTask, receiveMdnsTask, receiveUdpBroadcastTask);
            {
                if (completedTask == receiveUdpBroadcastTask)
                {
                    resultString = receiveUdpBroadcastTask.Result; 
                    var messageTokens = resultString.Split(',');
                    deviceInfo.IpAddress = messageTokens[0];
                    deviceInfo.DeviceName = messageTokens[1];
                }
                else if (completedTask == receiveMdnsTask)
                {
                    resultString = receiveMdnsTask.Result;
                }

                cts.Cancel(); //cancel the other tasks. 
            }

            return deviceInfo;
        }
    }

    public enum SecurityType
    {
        OPEN,
        WEP,
        WEP_SHARED,
        WPA2,
        WPA3
    }

    public class ChargerInfo
    {
        public ChargerInfo()
        {
            IpAddress = string.Empty; 
            DeviceName = string.Empty;  
        }
        public string IpAddress { get; set; }
        public string DeviceName { get; set; }
    }
}