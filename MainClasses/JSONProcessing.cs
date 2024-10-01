using System.Text;
using System.Text.Json;

namespace ClassLibrary1;

public class JsonProcessing
{
    /// <summary>
    /// Запись файла через MemoryStream.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static List<WifiParks> Read(MemoryStream stream)
    {
        var data = Encoding.UTF8.GetString(stream.ToArray());
        return JsonSerializer.Deserialize<List<WifiParks>>(data) ?? new List<WifiParks>();
    }

    /// <summary>
    /// Чтение файла через MemoryStream.
    /// </summary>
    /// <param name="parksList"></param>
    /// <returns></returns>
    public static MemoryStream Write(List<WifiParks> parksList)
    {
        string json = "[" + string.Join(',', parksList.Select(x => x.Json())) + "]";
        var bytes = Encoding.UTF8.GetBytes(json);
        var stream = new MemoryStream();
        stream.Write(bytes);
        return stream;
    }
}