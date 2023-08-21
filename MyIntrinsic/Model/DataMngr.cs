namespace MyIntrinsic.Model;
public class DataMngr
{
    public string ErrorMessage { get; set; } = String.Empty;
    public List<Charger> Chargers { get; set; } = new();
    public Customer Customer { get; protected set; } = new();
           
    public DataMngr()
    {
            
    }

    /// <summary>
    /// Non-async version of StoreCustomerInformationInSecureStorageAsync
    /// </summary>
    public async Task UpdateCustomerInformationAsync(Customer customer)
    {
        //do nothing if no changes
        if(!Customer.Equals(customer))
        {
            Customer = customer;
            await SetCustomerInformationInSecureStorageAsync(); 
        }    
    }

    public async Task SetCustomerInformationInSecureStorageAsync()
    {
        var json = String.Empty; 
        if(Customer is not null) 
        {
            json = JsonSerializer.Serialize(Customer);
        }

        await StoreItemInSecureStorageAsync("active_customer", json);
    }

    public async Task ClearCustomerInformationFromSecureStorage()
    {
        Customer = new Customer(); 
        var json = JsonSerializer.Serialize(Customer);

        await StoreItemInSecureStorageAsync("active_customer", json);
    }

    public async Task ClearLoginCredentialsFromSecureStorage()
    {
        var credentials = new LoginCredentials();
        var json = JsonSerializer.Serialize(Customer);

        await StoreItemInSecureStorageAsync("login_credentials", json);
    }

    public async Task GetCustomerInformationFromSecureStorageAsync()
    {
        string customerJson = string.Empty;
        var customer = new Customer();

        customerJson = await GetItemFromSecureStorageAsync("active_customer");

        //If customer is empty string, it was not in secure storage
        if (!customerJson.Equals(String.Empty))
        {
            try
            {
                customer = customerJson is not null ? JsonSerializer.Deserialize<Customer>(json: customerJson) : new Customer();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing customer json: {ex.Message}");
                ErrorMessage = ex.Message;
            }
        }

        Customer = customer is not null ? customer : new Customer();
    }

    public async Task SetLoginInformationInSecureStorageAsync(LoginCredentials loginCredentials)
    {
        await SetLoginInformationInSecureStorageAsync(loginCredentials.username, loginCredentials.password); 
    }

    public async Task SetLoginInformationInSecureStorageAsync(string username, string password)
    {
        var json = String.Empty;
        json = JsonSerializer.Serialize(new LoginCredentials(username, password));

        await StoreItemInSecureStorageAsync("login_credentials", json);
    }

    public async Task<LoginCredentials?> GetLoginInformationFromSecureStorageAsync()
    {
        string loginInfoJson = string.Empty;
        var loginCredentials = new LoginCredentials();

        loginInfoJson = await GetItemFromSecureStorageAsync("login_credentials");

        //If empty string, credentials were not in secure storage
        if (!loginInfoJson.Equals(String.Empty))
        {
            try
            {
                loginCredentials = loginInfoJson is not null ? JsonSerializer.Deserialize<LoginCredentials>(json: loginInfoJson) : new LoginCredentials();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing login credentials json: {ex.Message}");
                ErrorMessage = ex.Message;
            }
        }
               
        return loginCredentials;
    }

    //Clear customer information from local storage and data store
    public async Task LogoutAsync()
    {
        await ClearLoginCredentialsFromSecureStorage();
        await ClearCustomerInformationFromSecureStorage();
        Chargers.Clear();
    }

    async Task<string> GetItemFromSecureStorageAsync(string id)
    {
        var content = string.Empty;
        try
        {
            //Values will be null if keys doe not exist. 
            content = await SecureStorage.Default.GetAsync(id);
            content = content is not null ? content : String.Empty; 
        }
        catch (Exception ex)
        {
            //assume storage has become corrupted if exception is thrown
            Debug.WriteLine($"Exception reading json for id: {id} from secure storage: {ex.Message}");
            ErrorMessage = ex.Message; 
            SecureStorage.Default.Remove(id);

            //set default values
            await SecureStorage.Default.SetAsync(id, string.Empty);
        }

        return content;
    }

    async Task StoreItemInSecureStorageAsync(string id, string json)
    {
        try
        {
            SecureStorage.Default.Remove(id);
            await SecureStorage.Default.SetAsync(id, json);
        }
        catch (Exception ex)
        {
            //assume storage has become corrupted if exception is thrown
            Debug.WriteLine($"Exception writing json to secure storage for id: {id}: {ex.Message}");
            ErrorMessage = ex.Message;
        }
    }
}