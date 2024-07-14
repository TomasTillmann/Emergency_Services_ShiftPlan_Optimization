using System.IO;

namespace DistanceAPI;

public static class ApiKeyParser
{
    static ApiKeyParser()
    {
        ApiKey = File.ReadAllText("/home/tom/GoogleAPI/DistanceMatrixAPIkey_2.txt");
    }
    
    public static string ApiKey { get; }
}