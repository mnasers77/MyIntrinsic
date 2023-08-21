using System.Runtime.InteropServices;

namespace MyIntrinsic.ViewModel;

public partial class SelectChargerViewModel : BaseViewModel 
{
    public ObservableCollection<string> WiFiNetworks { get; } = new ObservableCollection<string>();

    public SelectChargerViewModel(WiFiService wifiService, ProvisioningService provisioningService, ILogger<SelectChargerViewModel> logger)
    {
        _logger = logger;
        _wifiService = wifiService;
        _provisioningService = provisioningService;

        IsBusy = false;
      
#if WINDOWS
        Title = "Configuration Page";
#else
        Title = "Configuration Page";
#endif
    }
    
    ILogger<SelectChargerViewModel> _logger;
    WiFiService _wifiService;
    ProvisioningService _provisioningService;

    [ObservableProperty]
    string activityText = "Scanning for Chargers";

    [ObservableProperty]
    string connectedWifiSsid = string.Empty;

    [ObservableProperty]
    string connectedWifiPassword = string.Empty;

    [ObservableProperty]
    string selectedNetwork = string.Empty;

    [ObservableProperty]
    string selectedNetworkPassword = string.Empty;

    [ObservableProperty]
    string deviceName = string.Empty;

    //Refresh from RefreshView
    [RelayCommand]
    public async Task RefreshWiFiNetworksAsync()
    {
        if (IsBusy == true) return;
        
        await GetChargerWiFiNetworksAsync();
        IsRefreshing = false; 
    }

    //Refresh from Activity Indicator
    [RelayCommand]
    public async Task UpdateWiFiNetworksAsync()
    {
        if ((IsBusy == true) && (IsRefreshing == true)) return;
        IsBusy = true;
        await GetChargerWiFiNetworksAsync();
        IsBusy = false;
    }


    public async Task GetChargerWiFiNetworksAsync()
    {
        ActivityText = "Scanning for Chargers";
        WiFiNetworks.Clear();

        var networks = await _wifiService.GetDetectedWiFiNetworkSSIDsAsync();//filter for intrinsic IOT devices

        networks = networks.FindAll(n => n.ToLower().Contains("simplelink") || n.ToLower().Contains("intrinsic"));

        if (networks.Any())
        {
            foreach (var adapter in networks)
            {
                WiFiNetworks.Add(adapter);
            }
        }

        //WiFiNetworks.Add("MySimpleLink-97548");
        //WiFiNetworks.Add("MySimpleLink-48103");
    }

    public async Task GetConnectedWiFiNetworkAsync()
    {
        ConnectedWifiSsid = String.Empty;
        
        if (_wifiService is null) return;

        //if(!await RequestLocationPermissionAsync()) return;

        ConnectedWifiSsid = await _wifiService.GetConnectedWiFiNetworkSSIDAsync();
    }

    [RelayCommand]
    async Task ContinueSetupAsync()
    {
        if (string.IsNullOrEmpty(SelectedNetwork))
            return;

        await Shell.Current.GoToAsync($"ConfigureChargerView?ssid={SelectedNetwork}");
    }

    //[RelayCommand]
    async Task<(bool, string)> ConnectWiFiNetworkAsync()
    {
        bool result = false;
        string resultMessage = string.Empty;

        _logger.LogInformation("ConnectWiFiNetwork called");

        if ((String.IsNullOrEmpty(SelectedNetwork)) || (_wifiService is null))
            return (result, resultMessage);

        //IsBusy = true;

        var currentSelectedNetwork = await _wifiService.GetConnectedWiFiNetworkSSIDAsync();
        if (SelectedNetwork.Equals(currentSelectedNetwork))
        {
            result = true;
            resultMessage = $"Already connected to {SelectedNetwork}";
        }
        else
        {
            (result, resultMessage) = await _wifiService.ConnectToWiFiNetworkAsync(SelectedNetwork, SelectedNetworkPassword);
        }
        
        return (result, resultMessage);

        //IsBusy = false;

        //if (result == 0)
        //{
        //    await Shell.Current.GoToAsync($"{nameof(ConfigureCharger)}", true /*animate*/,
        //        new Dictionary<string, object>
        //        {
        //        {"SelectedNetwork", SelectedNetwork }
        //        });
        //}
        //else //could not connect to WiFi network
        //{
        //    await Shell.Current.DisplayAlert("Error", $"Unable to connect to selected Charger: {SelectedNetwork}", "OK");
        //}
    }

    [RelayCommand]
    async Task DoConfigurationAsync()
    {
        bool result = false;
        string resultMessage = string.Empty;

        //var homeNetworkSsid = "ATT-WIFI-263x"; //"Nishizumi99"
        //var homeNetworkPassword = "32J69x53"; //"noisyKayak343"
        var homeNetworkSsid = "Nishizumi99";
        var homeNetworkPassword = "noisyKayak343";
        var connectedNetworkSsid = homeNetworkSsid; 

        if (IsBusy) return;

        IsBusy = true;

        ActivityText = "Connecting to Charger WiFi"; 
        (result, resultMessage) = await ConnectWiFiNetworkAsync();

        //if result is true we are connected to charger's WiFi network
        if (result == true)
        {
            connectedNetworkSsid = SelectedNetwork; 
            //var vciPage = await _provisioningService.GetVciStatusWebsiteAsync(); 
            //ProvisioningService.Ping("mysimplelink.net");
            //await ProvisioningService.PingAsync("10.123.45.1");
            //ProvisioningService.Ping("192.168.20.1");
            ActivityText = "Reading charger name";
            await Task.Delay(2000);
            DeviceName = await _provisioningService.GetDeviceNameAsync();
            ActivityText = $"Charger name is {DeviceName}";
            await Task.Delay(2000);
            ActivityText = $"Setting charger name to Home Charger 2";
            await _provisioningService.SetDeviceNameAsync("Home Charger 2");
            DeviceName = await _provisioningService.GetDeviceNameAsync();
            ActivityText = $"Charger name set to {DeviceName}";
            await Task.Delay(2500);

            //var chargers = await _provisioningService.GetWiFiNetworkstDetectedByDeviceAsync(); 
            //await _wifiService.DisconnectFromWiFiNetworkAsync(SelectedNetwork);

            ActivityText = $"Reading charger provisioning status";
            await Task.Delay(2000);
            var configResult = await _provisioningService.GetConfirmationResultAsync(); 
            ActivityText = $"Charger provisioning status is {configResult}";
            await Task.Delay(2000);

            ActivityText = $"Adding WLAN profile for network: {connectedNetworkSsid}";
            await Task.Delay(2000);
            var addProfileResult = await _provisioningService.AddWlanProfileToDeviceAsync(homeNetworkSsid, homeNetworkPassword, SecurityType.WPA2, "15");
            if (addProfileResult == true)
            {
                ActivityText = $"WLAN profile added to charger. Starting configuration";
                await Task.Delay(2000);
                await _provisioningService.StartConfigurationStageAsync();
                
                //disconnect from charger network and wait to reconnect to home network
                await _wifiService.DisconnectFromWiFiNetworkAsync(SelectedNetwork);
                while (!connectedNetworkSsid.Equals(homeNetworkSsid))
                {
                    await Task.Delay(2000); //give time to reconnect to home WiFi network
                    connectedNetworkSsid = await _wifiService.GetConnectedWiFiNetworkSSIDAsync();
                }
                SelectedNetwork = string.Empty;

                ActivityText = $"Charger configuration started. Reading charger IP address";
                await Task.Delay(1500);
                var deviceInfo = await _provisioningService.DetermineDeviceIPAysnc();

                if (deviceInfo.IpAddress != string.Empty)
                {
                    ActivityText = "Reading charger provisioning status";

                    var ipAddress = $"http://{ deviceInfo.IpAddress}";
                    configResult = await _provisioningService.GetConfirmationResultAsync(ipAddress);
                    ActivityText = $"Charger provisioning status is {configResult}";
                    await Task.Delay(2000);
                    if(configResult.Equals(ProvisioningService.ConfirmationSuccess))
                    {
                        ActivityText = $"Charger provisioning successful!";
                    }
                    else
                    {
                        ActivityText = $"Unable to read charger provisioning status. Provisioning Failed";  
                    }
                }
                else
                {
                    ActivityText = $"Unable to read charger IP address. Provisioning Failed";
                }
                await Task.Delay(2000);
            }
        }
        else
        {
            ActivityText = "Charger connection failed";
            await Task.Delay(2500); 
            result = false; 
        }

        IsBusy = false; 
    }
}
