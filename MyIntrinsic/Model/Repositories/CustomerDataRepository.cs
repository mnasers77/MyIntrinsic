namespace MyIntrinsic.Model;
public class CustomerDataRepository : ICustomerDataRepository
{
    string getCustomerInfoUrl = "/Prod/user"; //GET
    string updateCustomerInfoUrl = "/Prod/user_info"; //PUT

    public CustomerDataRepository(DataMngr dataMngr, HttpClient httpClient)
    {
        _dataMngr = dataMngr;
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri("https://9f6mnvq2ph.execute-api.us-east-1.amazonaws.com");
        //_httpClient.Timeout = new TimeSpan(0, 0, 30);
        _httpClient.DefaultRequestHeaders.Clear();
    }

    HttpClient _httpClient;
    DataMngr _dataMngr;

    public async Task<(bool, string)> GetCustomerInfoAsync()
    {
        var response = await _httpClient.GetAsync(getCustomerInfoUrl);
        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            //this will need to be fixed on the server side eventually
            responseString = responseString.Replace("\"is_subscribed_to_updates\": null", "\"is_subscribed_to_updates\": false");
            var customerResponse = JsonSerializer.Deserialize<GetCustomerResponse>(responseString);
            //var customerResponse = await response.Content.ReadFromJsonAsync<GetCustomerResponse>();
            var customer = customerResponse is not null ? customerResponse.data : new Customer(); 
            await _dataMngr.UpdateCustomerInformationAsync(customer);
        }
        else
        {
            await _dataMngr.UpdateCustomerInformationAsync(new Customer());
            Debug.WriteLine(response.StatusCode.ToString());
        }

        return (response.IsSuccessStatusCode, response.StatusCode.ToString()); 
    }

    public async Task<(bool, string)> UpdateCustomerInfoAsync(Customer customer)
    {
        if(customer is null)
        {
            return (false, "Invalid Data"); 
        }

        var json = JsonSerializer.Serialize(customer);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PutAsync(updateCustomerInfoUrl, stringContent);
        if (response.IsSuccessStatusCode)
        {
            await _dataMngr.UpdateCustomerInformationAsync(customer);
        }

        return (response.IsSuccessStatusCode, response.StatusCode.ToString());
    }
}