namespace MyIntrinsic.Provisioning.View;

public partial class SelectChargerView : ContentPage
{
    SelectChargerViewModel _viewModel;
    public SelectChargerView(SelectChargerViewModel setupChargerViewModel)
    {
        _viewModel = setupChargerViewModel;
        BindingContext = _viewModel;
        InitializeComponent();

        connectButton.IsEnabled = true;
        refreshButton.IsEnabled = true;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
#if WINDOWS
        await _viewModel.UpdateWiFiNetworksAsync(); 
#else
        _viewModel.IsRefreshing = true;  
#endif
        base.OnNavigatedTo(args);
    }

    private void Refresh_ToolbarItem_Clicked(object sender, EventArgs e)
    {
        
    }
}

