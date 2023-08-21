
using MyIntrinsic.Services;

namespace MyIntrinsic.ViewModel;

public partial class ConfigureChargerViewModel : BaseViewModel 
{
    public ConfigureChargerViewModel(WiFiService wifiService, ILogger<ConfigureChargerViewModel> logger)
    {
        _logger = logger;
        _wifiService = wifiService;

        Title = "Step 2: Configure Charger";

        SSID = String.Empty; 
    }

    ILogger _logger;
    WiFiService _wifiService;

    [ObservableProperty]
    string sSID;

    [ObservableProperty]
    string password;

    [ObservableProperty]
    string chargerPassword;

    [RelayCommand]
    async Task StartConfigurationAsync()
    {
        if (IsBusy)
            return; 

        IsBusy = true;
        //await UpdateWiFiNetworksAsync();
        IsBusy = false; 
    }

    [RelayCommand]
    public async Task GetConnectedWiFiNetworkAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        var ssid = await _wifiService.GetConnectedWiFiNetworkSSIDAsync();
        IsBusy = false;
    }

    [RelayCommand]
    public async Task SelectWiFiNetworkAsync(string ssid)
    {
        SSID = ssid; 
    }

    [RelayCommand]
    async Task ConnectWiFiNetworkAsync()
    {
        bool result = false;
        string resultMessage = string.Empty; 

        if( (SSID is null) || (_wifiService is null) )
            return;

        IsBusy = true;

        var currentSSID = await _wifiService.GetConnectedWiFiNetworkSSIDAsync(); 
        if(SSID.Equals(currentSSID))
        {
            result = true;
            resultMessage = $"Already connected to {SSID}";
        }
        else
        {
            (result, resultMessage) = await _wifiService.ConnectToWiFiNetworkAsync(SSID, Password);
        }

        IsBusy = false;

        //if (result == 0)
        //{
        //    await Shell.Current.GoToAsync($"{nameof(ConfigureCharger)}", true /*animate*/,
        //        new Dictionary<string, object>
        //        {
        //        {"SSID", SSID }
        //        });
        //}
        //else //could not connect to WiFi network
        //{
        //    await Shell.Current.DisplayAlert("Error", $"Unable to connect to selected Charger: {SSID}", "OK");
        //}
    }
}
