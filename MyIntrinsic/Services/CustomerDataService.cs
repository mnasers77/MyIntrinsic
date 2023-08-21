namespace MyIntrinsic.Services;
public class CustomerDataService
{
    public CustomerDataService(DataMngr dataMngr, ICustomerDataRepository customerDataRepository)
    {
        _dataMngr = dataMngr;
        _customerDataRepository = customerDataRepository;
    }

    DataMngr _dataMngr;
    ICustomerDataRepository _customerDataRepository;

    public async Task<(bool, string)> GetCustomerInfoAsync()
    {
        var responseTuple = await _customerDataRepository.GetCustomerInfoAsync(); 

        return responseTuple; 
    }

    public async Task<(bool, string)> UpdateCustomerInfoAsync(Customer customer)
    {
        var responseTuple = await _customerDataRepository.UpdateCustomerInfoAsync(customer);

        return responseTuple;
    }
}