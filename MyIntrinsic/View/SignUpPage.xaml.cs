using CommunityToolkit.Maui.Converters;

namespace MyIntrinsic.View;

[QueryProperty(nameof(Action), "action")]
public partial class SignUpPage : ContentPage
{
    SignUpViewModel viewModel;
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginPage" /> class.
    /// </summary>
    public SignUpPage(SignUpViewModel viewModel)
    {
        this.InitializeComponent();
        passwordValidator.RegexPattern = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$";
        //passwordValidator.RegexPattern = "^/d.{8,32}$";
        //passwordValidator.RegexOptions = 
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
        await viewModel.InitLoginInformation();
        if (Action.Equals("login"))
        {
            loginButton.IsEnabled = viewModel.IsLoginEnabled;
        }
        else if (Action.Equals("logout"))
        {
            await viewModel.LogoutAsync(); 
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