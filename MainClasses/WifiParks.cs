using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace ClassLibrary1;

public class WifiParks
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    
    [JsonPropertyName("globalId")]
    public long GlobalId { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("admArea")]
    public string AdmArea { get; set; }
    
    [JsonPropertyName("district")]
    public string District { get; set; }
    
    [JsonPropertyName("parkName")]
    public string ParkName { get; set; }
    
    [JsonPropertyName("wifiName")]
    public string WifiName { get; set; }
    
    [JsonPropertyName("coverageArea")]
    public long CoverageArea { get; set; }
    
    [JsonPropertyName("functionFlag")]
    public string FunctionFlag { get; set; }
    
    [JsonPropertyName("accessFlag")]
    public string AccessFlag { get; set; }
    
    [JsonPropertyName("password")]
    public string Password { get; set; }
    
    [JsonPropertyName("longitudeWgs84")]
    public double LongitudeWgs84 { get; set; }
    
    [JsonPropertyName("latitudeWgs84")]
    public double LatitudeWgs84 { get; set; }
    
    [JsonPropertyName("geodataCenter")]
    public string GeodataCenter { get; set; }
    
    [JsonPropertyName("geoArea")]
    public string GeoArea { get; set; }
    
    public WifiParks()
    {
        Id = 0;
        GlobalId = 0;
        Name = "";
        AdmArea = "";
        District = "";
        ParkName = "";
        WifiName = "";
        CoverageArea = 0;
        FunctionFlag = "";
        AccessFlag = "";
        Password = null;
        LongitudeWgs84 = 0.0;
        LatitudeWgs84 = 0.0;
        GeodataCenter = null;
        GeoArea = null;
    }
    
    [JsonConstructor]
    public WifiParks(long id, long globalId, string name, string admArea, string district, string parkName,
        string wifiName,
        long coverageArea, string functionFlag, string accessFlag, string password, double longitudeWgs84,
        double latitudeWgs84,
        string geodataCenter, string geoArea)
    {
        Id = id;
        GlobalId = globalId;
        Name = name;
        AdmArea = admArea;
        District = district;
        ParkName = parkName;
        WifiName = wifiName;
        CoverageArea = coverageArea;
        FunctionFlag = functionFlag;
        AccessFlag = accessFlag;
        Password = password;
        LongitudeWgs84 = longitudeWgs84;
        LatitudeWgs84 = latitudeWgs84;
        GeodataCenter = geodataCenter;
        GeoArea = geoArea;
    }
    public string CSv()
    {
        return '"' + Id + '"' + ";\"" + GlobalId + "\";\"" + string.Join(@""";""", new string?[]
        {
            Name, AdmArea, District, ParkName,
            WifiName, CoverageArea.ToString(), FunctionFlag, AccessFlag, Password, LongitudeWgs84.ToString().Replace(",", "."),
            LatitudeWgs84.ToString().Replace(",", "."), GeodataCenter, GeoArea
        }) + "\";";
    }

    public string Json()
    {
        var options1 = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true
        };
        var jsonString = JsonSerializer.Serialize(this, options1);
        return jsonString.Replace(@"\u022", "");
    }
}