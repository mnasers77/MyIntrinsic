
namespace MyIntrinsic.Services;
public interface ICustomerDataRepository
{
    Task<(bool, string)> GetCustomerInfoAsync();
    Task<(bool, string)> UpdateCustomerInfoAsync(Customer customer);
}