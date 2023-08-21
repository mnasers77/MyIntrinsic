namespace MyIntrinsic.Provisioning.View;

[QueryProperty(nameof(FullName), "fullName")]
public partial class AddChargerView : ContentPage
{
    AddChargerViewModel _viewModel;
    public string FullName { get; set; }
    public AddChargerView(AddChargerViewModel addChargerViewModel)
    {
        _viewModel = addChargerViewModel;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        fullNameLabel.Text = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(FullName);
    }
}

