using System.Net.Http.Json;
using MyIntrinsic.Services.Interfaces;

namespace MyIntrinsic.Services;

public class ChargerService
{
    IChargerRepository _chargerRespository;
    
    public ChargerService(IChargerRepository repository)
    {
        _chargerRespository = repository;
    }

    public async Task<(bool, string)> GetAllChargersAsync()
    {
        var resultTuple = await _chargerRespository.GetAllChargersAsync();
        
        return resultTuple;
    }

    public async Task<Charger?> GetChargerByIdAsync(string id)
    {
        var charger = await _chargerRespository.GetChargerByIdAsync(id);
       
        return charger;
    }

    public async Task<(bool, string)> AddItemAsync(Charger charger)
    {
        var resultTuple = await _chargerRespository.AddChargerAsync(charger);
                
        return resultTuple;
    }

    public async Task<(bool, string)> DeleteItemAsync(string id)
    {
        var resultTuple = await _chargerRespository.DeleteChargerAsync(id);
       
        return resultTuple;
    }
    
    public async Task<(bool, string)> UpdateItemAsync(Charger charger)
    {
        var resultTuple = await _chargerRespository.UpdateChargerAsync(charger);
      
        return resultTuple;
    }
}
