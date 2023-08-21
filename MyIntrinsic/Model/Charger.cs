using System.Text.Json.Serialization;

namespace MyIntrinsic.Model;
public class Charger
{
    public Charger()
    {
        SerialNumber = String.Empty;
        ChargerName = String.Empty;
        Description = String.Empty; 
    }

    [JsonPropertyName("serial_no")]
    public string SerialNumber { get; set; }

    [JsonPropertyName("charger_name")]
    public string ChargerName { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}