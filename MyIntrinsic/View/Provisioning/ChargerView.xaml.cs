namespace MyIntrinsic.Provisioning.View;

[QueryProperty(nameof(ChargerSSID), "ssid")]
public partial class ConfigureChargerView : ContentPage
{
    public string ChargerSSID;  
    
    public ConfigureChargerView(ConfigureChargerViewModel configureChargerViewModel)
    {
        _viewModel = configureChargerViewModel;
        InitializeComponent();
    }

    ConfigureChargerViewModel _viewModel;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.SSID = ChargerSSID; 
        BindingContext = _viewModel;
    }
}