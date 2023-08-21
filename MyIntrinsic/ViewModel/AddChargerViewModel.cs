
using MyIntrinsic.Services;

namespace MyIntrinsic.ViewModel;

public partial class AddChargerViewModel : BaseViewModel
{
    public AddChargerViewModel(ILogger<AddChargerViewModel> logger)
    {
        _logger = logger;

        Title = "Intrinsic Power";
    }

    ILogger _logger;

    [RelayCommand]
    async Task SelectChargerAsync()
    {
        await Shell.Current.GoToAsync($"//{nameof(SelectChargerView)}", true /*animate*/);
    }
}
