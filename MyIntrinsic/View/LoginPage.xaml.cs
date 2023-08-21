using CommunityToolkit.Maui.Converters;

namespace MyIntrinsic.View;

[QueryProperty(nameof(Action), "action")]
public partial class LoginPage : ContentPage
{
    LoginViewModel viewModel;
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginPage" /> class.
    /// </summary>
    public LoginPage(LoginViewModel viewModel)
    {
        this.InitializeComponent();
        this.viewModel = viewModel;
        this.BindingContext = viewModel;

        Action = Action is not null ? Action : "login"; 
    }

    public string Action { get; set; }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    protected override async void OnAppearing()
    {
        if (Action.Equals("logout"))
        {
            await viewModel.LogoutAsync(); 
        }
        else //see if login information is saved in local storage
        {
            await viewModel.InitLoginInformation();
        }
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        viewModel.IsUsernameValid = usernameValidator.IsValid;
        viewModel.IsLoginEnabled = (usernameValidator.IsValid && passwordValidator.IsValid);
        //loginButton.IsEnabled = viewModel.IsLoginEnabled; //need to set in code behind due to maui errors
    }

    private void PasswordEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        viewModel.IsPasswordValid = passwordValidator.IsValid; 
        viewModel.IsLoginEnabled = (usernameValidator.IsValid && passwordValidator.IsValid);
        //loginButton.IsEnabled = viewModel.IsLoginEnabled; //need to set in code behind due to maui errors
    }
}