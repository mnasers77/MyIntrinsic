namespace MyIntrinsic.Provisioning.View;

//[QueryProperty(nameof(ChargerSSID), "ssid")]
public partial class ConfigureChargerView : ContentPage
{
	public string ChargerSSID {get; set;}
    ConfigureChargerViewModel _viewModel;
    public string FullName { get; set; }
    public ConfigureChargerView(ConfigureChargerViewModel configChargerViewModel)
    {
        _viewModel = configChargerViewModel;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
		//ChargerSSID = ssid; 
        _viewModel.SSID = ChargerSSID;
    }
}

