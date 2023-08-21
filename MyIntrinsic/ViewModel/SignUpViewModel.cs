using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

namespace MyIntrinsic.ViewModel;


/// <summary>
/// ViewModel for login page.
/// </summary>
public partial class SignUpViewModel : ObservableValidator
{
    Command LoginCommand { get; set; }


    /// <summary>
    /// Initializes a new instance for the <see cref="LoginViewModel" /> class.
    /// </summary>
    public SignUpViewModel(ILoginService loginService, DataMngr dataMngr)
    {
        this.Password = String.Empty;
        this.Username = String.Empty;

        PasswordError = "Password should have a length of at least 8 characters and contain the following: At least one uppercase and one lowercase character. At least one number and one special character.";
        UsernameError = "Username should be at least 6 characters in length";

        this._loginService = loginService;
        this._dataMngr = dataMngr;
        
        IsLoginEnabled = true;
        IsBusy = false;

        LoginCommand = new Command(LoginAsync); 
    }

    //[Required]
    //[MinLength(6, ErrorMessage = "Username length must be at least 6 characters")]
    //public string? Username { get => _username; set => SetProperty(ref _username, value, true); }

    [ObservableProperty]
    string? _username;

    [ObservableProperty]
    string? _password;

    [ObservableProperty]
    string? _usernameError;

    [ObservableProperty]
    bool _isUsernameValid;

    [ObservableProperty]
    bool _isBusy;

    [ObservableProperty]
    bool _isPasswordValid;

    [ObservableProperty]
    string? _passwordError;

    //[Required]
    //[MinLength(8, ErrorMessage = "Password length must be at least 8 characters")]
    //public string? Password 
    //{ get => password;
    //    set
    //    {
    //        SetProperty(ref password, value, true);
    //        ValidateProperty(Password, ); 
    //    }
    //}

    bool _isLoginEnabled;
   
    public bool IsLoginEnabled
    {
        get => _isLoginEnabled;
        set
        {
            if (value != _isLoginEnabled)
            {
                _isLoginEnabled = value;
                OnPropertyChanged("IsLoginEnabled");
            }
        }
    }

    private ILoginService _loginService;
    private DataMngr _dataMngr;

    /// <summary>
    /// check the validation
    /// </summary>
    /// <returns>returns bool value</returns>
    //public void ValidateEntryFields()
    //{
    //    IsLoginEnabled = !HasErrors;

    //    //no errors with input
    //    if (!HasErrors)
    //    {
    //        IsUsernameValid = true;
    //        IsPasswordValid = true;
    //        UsernameError = String.Empty;
    //        PasswordError = String.Empty;
    //    }
    //    else
    //    {
    //        if (Username is null)
    //        {
    //            Username = string.Empty;
    //        }

    //        if (Password is null)
    //        {
    //            Password = string.Empty;
    //        }

    //        UsernameError = String.Join(Environment.NewLine, GetErrors(nameof(Username)).Select(e => e.ErrorMessage));
    //        PasswordError = String.Join(Environment.NewLine, GetErrors(nameof(Password)).Select(e => e.ErrorMessage));

    //        IsUsernameValid = UsernameError.Equals(string.Empty) ? true : false;
    //        IsPasswordValid = PasswordError.Equals(string.Empty) ? true : false;
    //    }
    //}

    //private bool ValidateUsername()
    //{
    //    var isValid = false;
    //    UsernameError = "Username is not valid!";

    //    if ((Username is not null) && (!String.IsNullOrWhiteSpace(Username)))
    //    {
    //        UsernameError = String.Empty;
    //        isValid = true;
    //    }

    //    return isValid;
    //}

    /// <summary>
    /// Initialize customer information or log out
    /// </summary>
    /// <returns></returns>
    //public async Task InitCustomerInformation()
    //{
    //    var customerInfo = await _dataMngr.GetCustomerInformationFromSecureStorageAsync();

    //    if (customerInfo is not null)
    //    {
    //        Username = customerInfo.Username;
    //        Password = customerInfo.Password;

    //        //auto login if user info is present
    //        if ((!Username.Equals(String.Empty)) && (!Password.Equals(String.Empty)))
    //        {
    //            IsLoginEnabled = true;
    //        }
    //    }
    //}

    /// <summary>
    /// Initialize customer login information
    /// </summary>
    /// <returns></returns>
    public async Task InitLoginInformation()
    {
        IsLoginEnabled = false;

        var loginInfo = await _dataMngr.GetLoginInformationFromSecureStorageAsync();

        if (loginInfo is not null)
        {
            Username = loginInfo.username;
            Password = loginInfo.password;

            //auto login if user info is present
            if ((!Username.Equals(String.Empty)) && (!Password.Equals(String.Empty)))
            {
                IsLoginEnabled = true;
                //Do not autologin on Windows due to bug in MAUI
                //if (DeviceInfo.Platform != DevicePlatform.WinUI)
                //{
                //await LoginAsync();
                //}
            }
        }
    }

    /// <summary>
    /// Invoked when the Login button is clicked.
    /// </summary>
    async void LoginAsync()
    {
        if (IsLoginEnabled)
        {
            //double check for null
            Username = Username is not null ? Username : String.Empty;
            Password = Password is not null ? Password : String.Empty;

            IsBusy = true; 
            var resultTuple = await _loginService.LoginAsync(Username, Password);
            IsBusy = false; 

            //check for successful login
            if (resultTuple.Item1 == true)
            {
                await _dataMngr.SetLoginInformationInSecureStorageAsync(Username, Password);
                IsLoginEnabled = false;

                try
                {
                    await Shell.Current.GoToAsync("//AboutPage");
                }
                catch(Exception ex)
                {
                    await Shell.Current.DisplayAlert("Shell Error", ex.Message, "OK");
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Login Error!", resultTuple.Item2, "OK");
            }
        }
    }

    /// <summary>
    /// Invoked when the Logout is invoked
    /// </summary>
    public async Task LogoutAsync()
    {
        IsBusy = true; 
        await _loginService.LogoutAsync();
        IsBusy = false; 

        //reset these values
        Password = String.Empty;
        Username = String.Empty;
        IsLoginEnabled = false;
        await _dataMngr.SetLoginInformationInSecureStorageAsync(Username, Password);
    }

    /// <summary>
    /// Invoked when the Sign Up button is clicked.
    /// </summary>
    /// <param name="obj">The Object</param>
    [RelayCommand]
    async Task SignUpAsync(object obj)
    {
        await Shell.Current.DisplayAlert("Alert", "Sign up button pressed", "OK");
    }

    /// <summary>
    /// Invoked when the Forgot Password button is clicked.
    /// </summary>
    /// <param name="obj">The Object</param>
    [RelayCommand]
    async Task ForgotPasswordAsync(object obj)
    {
        await Shell.Current.DisplayAlert("Alert", "Forgot password pressed", "OK");
    }

    /// <summary>
    /// Invoked when the close button is clicked.
    /// </summary>
    /// <param name="obj">The Object</param>
    [RelayCommand]
    async Task CloseAsync(object obj)
    {
        await Shell.Current.DisplayAlert("Alert", "Close button pressed", "OK");
    }

    void PasswordValidation()
    {
        if( (Password is null) || (Password.Length < 7) )
        {
            PasswordError = ""; 
        }
        //// Match lower case
        //this.newPasswordHasLowerCase = /[a - z] /.test(newPassword);
        //// Match upper case
        //this.newPasswordHasUpperCase = /[A - Z] /.test(newPassword);
        //// numeric
        //this.newPasswordHasNumber = /\d /.test(newPassword);
        //// Match special character
        //this.newPasswordHasSpecialCharacter = /[^a - zA - Z0 - 9] /.test(newPassword);
        //// Check length
        //this.newPasswordLengthIsValid = newPassword.length > 7;

        //const numberOfMatches = (this.newPasswordHasLowerCase ? 1 : 0) + (this.newPasswordHasUpperCase ? 1 : 0) + (this.newPasswordHasNumber ? 1 : 0) + (this.newPasswordHasSpecialCharacter ? 1 : 0);

        //this.validPassword = this.newPasswordLengthIsValid && numberOfMatches > 2;
    }
}
