using AndroidX.Core.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Microsoft.Extensions.Logging;
using static Javax.Crypto.Spec.PSource;

namespace MyIntrinsic.Services
{
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1422 // Validate platform compatibility
    public partial class WiFiService
    {
        WifiManager _wifiManager;
        NetworkCallback _networkCallback = null; //set to null to prevent callbacks before they are wanted

        public WiFiService(ILogger<WiFiService> logger)
        {
            _wifiManager = (WifiManager)Android.App.Application.Context
                        .GetSystemService(Context.WifiService);
            
            _logger = logger;
        }

        //public partial Task<string> GetConnectedWiFiNetworkSSIDAsync()
        //{
        //    var ssid = string.Empty;
        //    WifiInfo wifiInfo = null;

        //    if (_wifiManager is not null)
        //    {
        //        wifiInfo = _wifiManager.ConnectionInfo;

        //        if ((wifiInfo != null) && (wifiInfo.SupplicantState == SupplicantState.Completed))
        //        {
        //            ssid = wifiInfo.SSID.TrimStart(' ', '/', '"').TrimEnd('/', '"');
        //        }
        //    }

        //    return Task.FromResult(ssid);
        //}

        public partial async Task<string> GetConnectedWiFiNetworkSSIDAsync()
        {
            NetworkCallbackFlags flagIncludeLocationInfo = NetworkCallbackFlags.IncludeLocationInfo;
            var networkCallback = new ConnectionInfoNetworkCallback((int)flagIncludeLocationInfo);

            var wifiInfo = await networkCallback.GetConnectionInfoAsync();
            var ssid = wifiInfo.SSID.TrimStart(' ', '/', '"').TrimEnd('/', '"');

            return ssid;
        }

        public partial Task<bool> IsWiFiEnabledAsync()
        {
            var enabled = false;
            if (_wifiManager is not null)
            {
                enabled = _wifiManager.IsWifiEnabled;
            }
            return Task.FromResult(enabled);
        }

        public partial async Task<List<string>> GetDetectedWiFiNetworkSSIDsAsync()
        {
            List<string> availableNetworks = new List<string>();

            //Check for ACCESS_FINE_LOCATION
            //ActivityCompat.RequestPermissions(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity,
            //    new String[] { Android.Manifest.Permission.AccessFineLocation }, 1);

            var wifiReceiver = new WiFiReceiver(_wifiManager);

            //Register the Broadcast receiver for the scan results
            Android.App.Application.Context.RegisterReceiver(wifiReceiver, new IntentFilter(WifiManager.ScanResultsAvailableAction));

            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource(20000);
            Task scanTimeoutTask = Task.Delay(System.Threading.Timeout.Infinite, cts.Token);
            Task<List<string>> scanForWiFiTask = wifiReceiver.ScanAsync();

            //Wait until scanning is done or timeout task finishes
            Task completedTask = await Task.WhenAny(scanTimeoutTask, scanForWiFiTask);
            if (completedTask == scanForWiFiTask)
            {
                availableNetworks = scanForWiFiTask.Result;
            }
           
            //Remove registration of the Broadcast receiver
            Android.App.Application.Context.UnregisterReceiver(wifiReceiver);

            return availableNetworks;
        }

        public partial async Task<(bool, string)> ConnectToWiFiNetworkAsync(string ssid, string password)
        {
            var connectedAP = await GetConnectedWiFiNetworkSSIDAsync();
            bool result = false;
            string resultMessage = string.Empty;

            //already connected to desired network
            if (connectedAP == ssid)
            {
                return (true, $"Already connected to WiFi network with SSID: {ssid}");
            }

            result = await ConnectWiFiNetworkRequestNetworkMethodAsync(ssid, password);

            return (result, resultMessage);
        }

        public async Task<bool> ConnectWiFiNetworkRequestNetworkMethodAsync(string ssid, string password)
        {
            var isConnected = false;

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
            {

                try
                {
                    WifiNetworkSpecifier specifier = null; 
                    //Network specifier object used to request a Wi-Fi network. Apps should use the WifiNetworkSpecifier.Builder class to create an instance.
                    if(String.IsNullOrEmpty(password)) 
                    {
                        specifier = new WifiNetworkSpecifier.Builder().SetSsid(ssid).Build();
                    }
                    else
                    {
                        specifier = new WifiNetworkSpecifier.Builder().SetSsid(ssid).SetWpa2Passphrase(password).Build();
                    }

                    //used to request a network via ConnectivityManager RequestNetwork
                    var request = new NetworkRequest.Builder()
                        .AddTransportType(TransportType.Wifi)
                        .RemoveCapability(NetCapability.Internet) //Internet not required
                        .AddCapability(NetCapability.NotRestricted)
                        .AddCapability(NetCapability.Trusted)
                        .SetNetworkSpecifier(specifier)
                        .Build();

                    _networkCallback = new NetworkCallback();

                    isConnected = await _networkCallback.RequestNetwork(request);
                    var connectedSSID = await GetConnectedWiFiNetworkSSIDAsync();
                    if(connectedSSID != ssid)
                    {
                        isConnected = false;
                    }   

                }
                catch (Exception e)
                {
                    _logger?.Log(LogLevel.Error, e.Message);
                    isConnected = false;
                }
            }

            return isConnected;
        }

        public partial async Task<bool> DisconnectFromWiFiNetworkAsync(string ssid)
        {
            var result = false;
            var connectedSSID = string.Empty;

            if (ssid != string.Empty)
            {
                connectedSSID = await GetConnectedWiFiNetworkSSIDAsync();
                if (connectedSSID != ssid) //not connected to network with passed in SSID
                {
                    result = true;
                }
                else
                {
                    result = _networkCallback?.UnregisterNetwork() ?? false;
                    _networkCallback = null;
                }
            }

            return result;
        }
    }
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1422 // Validate platform compatibility

#pragma warning disable CA1422 // Validate platform compatibility
    class WiFiReceiver : BroadcastReceiver
    {
        private WifiManager _wifiMngr;
        private TaskCompletionSource<List<string>> tcs;

        public WiFiReceiver(WifiManager wifiMngr)
        {
            this._wifiMngr = wifiMngr;
        }

        public Task<List<string>> ScanAsync()
        {
            tcs = new TaskCompletionSource<List<string>>();
            
            _wifiMngr.StartScan();

            return tcs.Task;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            List<string> wifiNetworks = new List<string>();
            IList<ScanResult> scanResults = _wifiMngr.ScanResults;

            foreach (ScanResult scanResult in scanResults)
            {
                wifiNetworks.Add(scanResult.Ssid);
            }

            tcs?.SetResult(wifiNetworks);
        }
    }
#pragma warning restore CA1422 // Validate platform compatibility

#pragma warning disable CA1416 // Validate platform compatibility
    
    //Class to receive callbacks from ConnectivityManager.RequestNetwork
    //Encapsulates ConnectivityManager 
    public class NetworkCallback : ConnectivityManager.NetworkCallback
    {
        public Action<Network> NetworkAvailable { get; set; }
        public Action<WifiInfo> NetworkCapabilitiesChanged { get; set; }
        private TaskCompletionSource<bool> _requestNetworkTcs;
        
        ConnectivityManager _connectivityManager;

        public NetworkCallback()
        {
            _connectivityManager = (ConnectivityManager)Android.App.Application.Context.GetSystemService(Context.ConnectivityService);
        }

        //Initiate a request to connect to a network
        public Task<bool> RequestNetwork(NetworkRequest request)
        {
            _requestNetworkTcs = new TaskCompletionSource<bool>();

            _connectivityManager?.RequestNetwork(request, this);

            return _requestNetworkTcs.Task;
        }

        //Used when disconnecting from a boun network
        public bool UnregisterNetwork()
        {
            var result = false;

            try
            {
                _connectivityManager?.BindProcessToNetwork(null); //Needed so app can get back on the internet
                _connectivityManager?.UnregisterNetworkCallback(this);
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        public override void OnCapabilitiesChanged(Network network, NetworkCapabilities networkCapabilities)
        {
            base.OnCapabilitiesChanged(network, networkCapabilities);
            WifiInfo wifiInfo = (WifiInfo)networkCapabilities.TransportInfo;

            NetworkCapabilitiesChanged?.Invoke(wifiInfo);
        }

        public override void OnAvailable(Network network)
        {
            base.OnAvailable(network);
            _connectivityManager.BindProcessToNetwork(network); //needed so httpclient will use this network when communicating to charger
            NetworkAvailable?.Invoke(network);
            _requestNetworkTcs?.SetResult(true);
            _requestNetworkTcs = null;
        }

        public override void OnUnavailable()
        {
            base.OnUnavailable();
            _requestNetworkTcs?.SetResult(false);
            _requestNetworkTcs = null; 
        }
    }
#pragma warning restore CA1416 // Validate platform compatibility

#pragma warning disable CA1416 // Validate platform compatibility

    //Class to receive callbacks from ConnectivityManager.RegisterNetworkCallback
    //Encapsulates a ConnectivityManager 
    public class ConnectionInfoNetworkCallback : ConnectivityManager.NetworkCallback
    {
        private TaskCompletionSource<WifiInfo> _requestNetworkCapabilitiesTcs; //used to get SSID of connected network

        ConnectivityManager _connectivityManager;

        public ConnectionInfoNetworkCallback(int flags) : base(flags)
        {
            _connectivityManager = (ConnectivityManager)Android.App.Application.Context.GetSystemService(Context.ConnectivityService);
        }

        public Task<WifiInfo> GetConnectionInfoAsync()
        {
            var networkRequest = new NetworkRequest.Builder()
                        .AddTransportType(TransportType.Wifi).Build();

            _requestNetworkCapabilitiesTcs = new TaskCompletionSource<WifiInfo>();

            _connectivityManager?.RegisterNetworkCallback(networkRequest, this);

            return _requestNetworkCapabilitiesTcs.Task;
        }

        public override void OnCapabilitiesChanged(Network network, NetworkCapabilities networkCapabilities)
        {
            base.OnCapabilitiesChanged(network, networkCapabilities);
            WifiInfo wifiInfo = (WifiInfo)networkCapabilities.TransportInfo;

            //unregister callback or it will keep getting called and the tcs will raise an exception
            _connectivityManager?.UnregisterNetworkCallback(this);

            _requestNetworkCapabilitiesTcs?.SetResult(wifiInfo);
        }
    }
#pragma warning restore CA1416 // Validate platform compatibility
}