using MyIntrinsic.Services.Networking;

namespace MyIntrinsic.Services;
public class LoginServiceMock : ILoginService
{   
    public string AuthToken { get => "eyJraWQiOiJic0taaVJGazdkT1kyMzhMckxoUHNFY3RIdFhKTG83SXphV1NkSkJkU3h3PSIsImFsZyI6IlJTMjU2In0.eyJzdWIiOiJhM2Q2NDdmZS1jYjU5LTQ2YzUtOTNlYS03OWZhOTJlYWQ0MzYiLCJpc3MiOiJodHRwczpcL1wvY29nbml0by1pZHAudXMtZWFzdC0xLmFtYXpvbmF3cy5jb21cL3VzLWVhc3QtMV9KS2g2UU5QY04iLCJjbGllbnRfaWQiOiI0OXNqMDVva3JyNGJnMnJybDJvdGU2OGI3YiIsIm9yaWdpbl9qdGkiOiIyMGJmNGFjMi04ZmZhLTQ0YjMtOTA0Zi0yZTc1OWY3YmVhNTQiLCJldmVudF9pZCI6IjFiZmVhM2MyLTAxYTMtNDAxMi1iMjcxLWNjY2I0MGMxMTE3MSIsInRva2VuX3VzZSI6ImFjY2VzcyIsInNjb3BlIjoiYXdzLmNvZ25pdG8uc2lnbmluLnVzZXIuYWRtaW4iLCJhdXRoX3RpbWUiOjE2NTcwMzg2NTYsImV4cCI6MTY1NzA0MjI1NiwiaWF0IjoxNjU3MDM4NjU2LCJqdGkiOiJiMDFmYzVjMi01NDY1LTQzZmEtYTAxMC00ZjYwYjIyNTVmZmYiLCJ1c2VybmFtZSI6Im1uYXplcnMifQ.FyEAFi9Fg0lDFadVSvvM3f7YgcRhHr-EipIZOgB0jJSJ5fypSFQzjI39izHrGUDntQEhe0Z4UhwmmTahExGQARwnOhWMIl1ZBFHZFxkv7OhlTFK4I44mTkIV7__ag0AGNYYzbBL3lCnGiCDMx89w2WozXyUGpQTFef2qyJkxJq3I-cMqyIrQBoYL7N2LGYN5KONKVyKT5odgUrpZFHRM25jicRtr2QL12TpUavUH2FPfMXZe43ocN2bdOmY6LSMB4s3kaCbwkVj9Vdlwz9wyLTT7LWyRJ-RfWFpDm4N9kxX1c1-nrZmxsYM75CcyKTrjsQUg3Up_08QAYmD7LqtwIw"; }
    public LoginServiceMock(DataMngr dataMngr)
    {
        _dataMngr = dataMngr;
    }

    DataMngr _dataMngr; 
    public async Task<(bool, string)> LoginAsync(string username, string password)
    {
        Customer customer = new Customer()
        {
             FirstName = "mike",
             LastName = "nasers",
             Username = username,
             Email = "mnasers@intrinsicpower.com",
             PhoneNumber = "+17347806549",
             Address = "Home Address",
             AccountName = "ACOM25646",
             PostalCode = "95758",
             Password = password
        };

        await Task.Delay(2500);
        await _dataMngr.UpdateCustomerInformationAsync(customer);
        
        return (true, "Success");
    }

    public async Task<(bool, string)> LogoutAsync()
    {
        await Task.Delay(1000); 
        await _dataMngr.ClearCustomerInformationFromSecureStorage(); 
        return (true, "Success");
    }

    public Task<Token> RefreshTokenAsync()
    {
        return Task.FromResult(new Token() { Scheme = "authorizationToken", AccessToken = AuthToken });
    }
}