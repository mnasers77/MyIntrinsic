namespace MyIntrinsic.Model;

public class LoginResponse
{
    public bool error { get; set; }
    public bool success { get; set; }
    public string message { get; set; }
    public Data data { get; set; }
}

public class Data
{
    public Challengeparameters ChallengeParameters { get; set; }
    public Authenticationresult AuthenticationResult { get; set; }
    public Responsemetadata ResponseMetadata { get; set; }
}

public class Challengeparameters
{
}

public class Authenticationresult
{
    public string AccessToken { get; set; }
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; }
    public string RefreshToken { get; set; }
    public string IdToken { get; set; }
}

public class Responsemetadata
{
    public string RequestId { get; set; }
    public int HTTPStatusCode { get; set; }
    public Httpheaders HTTPHeaders { get; set; }
    public int RetryAttempts { get; set; }
}

public class Httpheaders
{
    public string date { get; set; }
    public string contenttype { get; set; }
    public string contentlength { get; set; }
    public string connection { get; set; }
    public string xamznrequestid { get; set; }
}
