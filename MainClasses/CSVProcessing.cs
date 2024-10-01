using System.Text;
using MainClasses;

namespace ClassLibrary1;

public class CsvProcessing
{
    /// <summary>
    /// Чтение файла через MemoryStream.
    /// </summary>
    /// <param name="fileStream"></param>
    /// <returns></returns>
    public static List<WifiParks> Read(MemoryStream fileStream)
    {
        string[] headers =
        {
            "\"ID\"", "\"global_id\"",
            "\"Name\"", "\"AdmArea\"", "\"District\"", "\"ParkName\"", "\"WiFiName\"",
            "\"CoverageArea\"", "\"FunctionFlag\"", "\"AccessFlag\"", "\"Password\"", "\"Longitude_WGS84\"",
            "\"Latitude_WGS84\"", "\"geodata_center\"", "\"geoarea\""
        };
        List<WifiParks> parksList = new List<WifiParks>();
        var data = Encoding.UTF8.GetString(fileStream.ToArray());

        var split = data.Split('\n');
        if (split[0] != string.Join(";", headers) + ";") {return new List<WifiParks>();}

        foreach (var el in split)
        {
            var park = el.Split(";");
            if (park.Length == 16 & park[0] != "\"ID\"" & park[0] != "\"Код\"")
            {
                parksList.Add(new WifiParks
                {
                    Id = long.Parse(park[0][1..^1]),
                    GlobalId = long.Parse(park[1][1..^1]),
                    Name = park[2][1..^1],
                    AdmArea = park[3][1..^1],
                    District = park[4][1..^1],
                    ParkName = park[5][1..^1],
                    WifiName = park[6][1..^1],
                    CoverageArea = long.Parse(park[7][1..^1]),
                    FunctionFlag = park[8][1..^1],
                    AccessFlag = park[9][1..^1],
                    Password = park[10][1..^1],
                    LongitudeWgs84 = double.Parse(park[11][1..^1].Replace(".", ",")),
                    LatitudeWgs84 = double.Parse(park[12][1..^1].Replace(".", ",")),
                    GeodataCenter = park[13][1..^1],
                    GeoArea = park[14][1..^1]
                });
            }
        }

        return parksList;
    }

    /// <summary>
    /// Запись файла через MemoryStream.
    /// </summary>
    /// <param name="parksList"></param>
    /// <returns></returns>
    public static MemoryStream Write(List<WifiParks> parksList)
    {
        string[] headers =
        {
            "\"ID\"", "\"global_id\"",
            "\"Name\"", "\"AdmArea\"", "\"District\"", "\"ParkName\"", "\"WiFiName\"",
            "\"CoverageArea\"", "\"FunctionFlag\"", "\"AccessFlag\"", "\"Password\"", "\"Longitude_WGS84\"",
            "\"Latitude_WGS84\"", "\"geodata_center\"", "\"geoarea\""
        };
        string csv = string.Join('\n', parksList.Select(x => x.CSv()));
        var bytes = Encoding.UTF8.GetBytes(string.Join(";", headers) + ";\n" + csv);
        var stream = new MemoryStream();
        stream.Write(bytes);
        return stream;
    }
}