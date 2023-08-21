using Microsoft.Maui.ApplicationModel.Communication;
using System.Text.Json.Serialization;

namespace MyIntrinsic.Model;

public class Customer
{
    public Customer()
    {
        Username = String.Empty;
        FirstName = String.Empty;
        LastName = String.Empty;
        Email = String.Empty;      
        PhoneNumber = String.Empty;
        Address = String.Empty;
        Country = String.Empty;
        PostalCode = String.Empty;        
        AccountName = String.Empty;
        Password = String.Empty;
        UserId = String.Empty;
        UserProfile = String.Empty;
        CreatedAt = String.Empty;
        IsSubscribedToUpdates = false; 
        Status = true; 
    }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string LastName { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }
    
    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; }

    [JsonPropertyName("account_name")]
    public string AccountName { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("UserId")]
    public string UserId { get; set; }

    [JsonPropertyName("user_profile")]
    public string UserProfile { get; set; }

    [JsonPropertyName("is_subscribed_to_updates")]
    public bool IsSubscribedToUpdates { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("status")]
    public bool Status { get; set; }

    public override bool Equals(object? obj)
    {
        var customer = obj as Customer;

        if (customer is null)
        {
            return false;
        }

        if( (FirstName != customer.FirstName) || (LastName != customer.LastName) || (Password != customer.Password)
        || (Username != customer.Username) || (Email != customer.Email) || (PhoneNumber != customer.PhoneNumber) || (Address != customer.Address)
        || (AccountName != customer.AccountName) || (PostalCode != customer.PostalCode) )
        {
            return false;
        }
        else
        {
            return true; 
        }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode(); 
    }
}

public class LoginCredentials
{
    public LoginCredentials()
    {
        username = String.Empty;
        password = String.Empty; 
    }
    public LoginCredentials(string _username, string _password)
    {
        username = _username;
        password = _password;
    }

    public string username { get; set; }
    public string password { get; set; }
}

public class GetCustomerResponse
{
    public GetCustomerResponse()
    {
        error = false;
        success = false;
        data = new Customer();
    }

    public bool error { get; set; }
    public bool success { get; set; }
    public Customer data { get; set; }
}


