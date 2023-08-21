
namespace MyIntrinsic.Model; 

public interface IChargerRepository
{
    Task<(bool, string)> GetAllChargersAsync();
    Task<(bool, string)> AddChargerAsync(Charger Charger);
    Task<(bool, string)> UpdateChargerAsync(Charger Charger);
    Task<(bool, string)> DeleteChargerAsync(string id);
    Task<Charger?> GetChargerByIdAsync(string id);
}
