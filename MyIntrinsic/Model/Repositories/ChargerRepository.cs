using System.Net.Http.Json;

namespace MyIntrinsic.Model;

public class ChargerRepository : IChargerRepository
{
    //string getAllEvseDataUrl = "https://9f6mnvq2ph.execute-api.us-east-1.amazonaws.com/Prod/evse"; //GET
    //string insertEvseDataUrl = "https://9f6mnvq2ph.execute-api.us-east-1.amazonaws.com/Prod/evse_v2"; //POST
    //string updateEvseDataUrl = "https://9f6mnvq2ph.execute-api.us-east-1.amazonaws.com/Prod/update_data"; //PUT
    //string deleteEvseDataUrl = "https://9f6mnvq2ph.execute-api.us-east-1.amazonaws.com/Prod/evse/ABCD9"; //DEL

    string getAllEvseDataUrl = "/Prod/evse"; //GET
    string insertEvseDataUrl = "/Prod/evse_v2"; //POST
    string updateEvseDataUrl = "/Prod/update_data"; //PUT
    string deleteEvseDataUrl = "/Prod/evse/ABCD9"; //DEL

    public ChargerRepository(DataMngr dataMngr, IHttpClientFactory clientFactory)
    {
        //_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _dataMngr = dataMngr;
    }

    DataMngr _dataMngr;
    IHttpClientFactory _httpClientFactory;

    public async Task<(bool, string)> GetAllChargersAsync()
    {
        var url = new Uri(getAllEvseDataUrl);
        List<Charger>? chargers = new(); 

        //In memory caching
        if(_dataMngr.Chargers.Count > 0)
        {
            return (true, "Ok"); 
        }

        var httpClient = _httpClientFactory.CreateClient("AwsClient");
        HttpResponseMessage response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            chargers = await response.Content.ReadFromJsonAsync<List<Charger>>();
            var content = await response.Content.ReadAsStringAsync();
            if (content is not null)
            {
                chargers = System.Text.Json.JsonSerializer.Deserialize<List<Charger>>(content);
            }
            chargers = chargers is not null ? chargers : new List<Charger>();
        }

        _dataMngr.Chargers = chargers;

        return (response.IsSuccessStatusCode, response.StatusCode.ToString());
    }

    public async Task<Charger?> GetChargerByIdAsync(string serialNum)
    {
        Charger? charger = null;
        bool result = false;
        string resultMessage = string.Empty; 
        
        (result, resultMessage) = await GetAllChargersAsync();
        if(result == true)
        {
            foreach (var item in _dataMngr.Chargers)
            {
                if (item.SerialNumber == serialNum)
                {
                    charger = item;
                }
            }
        }

        return charger;    
    }

    public async Task<(bool, string)> AddChargerAsync(Charger Charger)
    {
        var url = new Uri(insertEvseDataUrl);

        var json = JsonSerializer.Serialize(Charger);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient("AwsClient");
        HttpResponseMessage response = await httpClient.PostAsync(url, content);

        return (response.IsSuccessStatusCode, response.StatusCode.ToString());
    }

    public async Task<(bool, string)> UpdateChargerAsync(Charger Charger)
    {
        var url = new Uri(updateEvseDataUrl);

        var json = JsonSerializer.Serialize(Charger);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient("AwsClient");
        HttpResponseMessage response = await httpClient.PostAsync(url, content);

        return (response.IsSuccessStatusCode, response.StatusCode.ToString());
    }

    public async Task<(bool, string)> DeleteChargerAsync(string id)
    {
        var url = new Uri($"{deleteEvseDataUrl}/{id}");

        var httpClient = _httpClientFactory.CreateClient("AwsClient");
        HttpResponseMessage response = await httpClient.DeleteAsync(url);

        return (response.IsSuccessStatusCode, response.StatusCode.ToString());
    }
}
