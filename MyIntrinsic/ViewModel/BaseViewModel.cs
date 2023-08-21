namespace MyIntrinsic.ViewModel;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    string title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool isRefreshing;

    public bool IsNotBusy => !IsBusy && !IsRefreshing;

    protected async Task<bool> RequestLocationPermissionAsync()
    {
        bool result = false;

        if (DeviceInfo.Platform != DevicePlatform.Android)
            return true;

        var status = PermissionStatus.Unknown;

        status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
            return true;

        if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
        {
            await Shell.Current.DisplayAlert("Permission required", "Location permission is required in order to scan for charger stations.", "OK");
        }

        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            await Shell.Current.DisplayAlert("Permission required",
                "Location permission is required in order to scan for charger stations. " +
                "We do not store or use your location information.", "OK");
        }
        else
        {
            result = true; 
        }

        return result; 
    }
}