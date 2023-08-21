namespace MyIntrinsic;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
        Routing.RegisterRoute(nameof(AddChargerView), typeof(AddChargerView));
    }

    private void OnMenuItemClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//LoginPage?action=logout");
    }
}
