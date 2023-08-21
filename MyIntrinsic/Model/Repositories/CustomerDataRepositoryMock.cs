
namespace MyIntrinsic.Model;
public class CustomerDataRepositoryMock : ICustomerDataRepository
{
    public CustomerDataRepositoryMock(DataMngr dataMngr)
    {
        _dataMngr = dataMngr;
    }

    DataMngr _dataMngr; 

    public async Task<(bool, string)> GetCustomerInfoAsync()
    {
        Customer customer = new Customer()
        {
            FirstName = "mike",
            LastName = "nasers",
            Email = "mnasers@intrinsicpower.com",
            PhoneNumber = "+17347806549",
            Address = "Home Address",
            AccountName = "ACOM25646",
            PostalCode = "95758",
            Password = "password1234567"
        };

        await Task.Delay(1000);
        await _dataMngr.UpdateCustomerInformationAsync(customer);
        return (true, "Ok"); 
    }

    public async Task<(bool, string)> UpdateCustomerInfoAsync(Customer customer)
    {
        await Task.Delay(1500);
        await _dataMngr.UpdateCustomerInformationAsync(customer);
        return (true, "Ok");
    }
}