using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

namespace MyIntrinsic.ViewModel;


/// <summary>
/// ViewModel for login page.
/// </summary>
public partial class LoginViewModel : ObservableValidator
{
    /// <summary>
    /// Initializes a new instance for the <see cref="LoginViewModel" /> class.
    /// </summary>
    public LoginViewModel(ILoginService loginService, CustomerDataService customerDataService, DataMngr dataMngr)
    {
        this.Password = String.Empty;
        this.Username = String.Empty;

        PasswordError = "Password should have a length of at least 8 characters and contain the following: At least one uppercase and one lowercase character. At least one number and one special character.";
        UsernameError = "Username should be at least 6 characters in length";

        this._loginService = loginService;
        this._dataMngr = dataMngr;
        this._customerDataService = customerDataService;

        IsLoginEnabled = false;
        IsBusy = false;
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
    private CustomerDataService _customerDataService;
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
                //Do not autologin on Windows due to bug in MAUI
                //if (DeviceInfo.Platform != DevicePlatform.WinUI)
                //{
                await LoginAsync();
                //}
            }
        }
    }

    [RelayCommand]
    /// <summary>
    /// Invoked when the Login button is clicked.
    /// </summary>
    async Task LoginAsync()
    {
        if (IsLoginEnabled)
        {
            //double check for null
            Username = Username is not null ? Username : String.Empty;
            Password = Password is not null ? Password : String.Empty;

            IsBusy = true; 
            var resultTuple = await _loginService.LoginAsync(Username, Password);
            
            //check for successful login
            if (resultTuple.Item1 == true)
            {
                await _dataMngr.SetLoginInformationInSecureStorageAsync(Username, Password);
                IsLoginEnabled = false;

                resultTuple = await _customerDataService.GetCustomerInfoAsync();
                if (resultTuple.Item1 == false)
                {
                    await LogoutAsync();
                    IsBusy = false;
                    await Task.Delay(5000);
                    await Shell.Current.DisplayAlert("Login error", "Unable to load customer data. Try logging in again", "OK");
                }
                else //login successful navigate to main page
                {
                    try
                    {
                        var fullName = $"{_dataMngr.Customer.FirstName} {_dataMngr.Customer.LastName}";
                        await Shell.Current.GoToAsync($"AddChargerView?fullName={fullName}");
                    }
                    catch (Exception ex)
                    {
                        await Shell.Current.DisplayAlert("Navigation Error", ex.Message, "OK");
                    }
                }
            }
            else
            {
                await Task.Delay(5000); //Give view time to load
                IsBusy = false;
                await Shell.Current.CurrentPage.DisplayAlert("Login Error", resultTuple.Item2, "OK");
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
}
