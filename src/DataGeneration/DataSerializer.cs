using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Reflection;

namespace DataHandling;

public static class DataSerializer
{
    private class PrivateResolver : DefaultContractResolver {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            if (!prop.Writable) {
                var property = member as PropertyInfo;
                var hasPrivateSetter = property?.GetSetMethod(true) != null;
                prop.Writable = hasPrivateSetter;
            }

            return prop;
        }

}
    public static string Path { get; } = "D:/Playground/EmergencyServicesShiftPlanOptimization/src/Data/";

    public static void Serialize<T>(T data, string file)
    {
        file = System.IO.Path.Combine(Path, file);
        using StreamWriter writer = new(file);

        writer.Write(JsonConvert.SerializeObject(data));
    }

    public static T Deserialize<T>(string file)
    {
        file = System.IO.Path.Combine(Path, file);

        string data = File.ReadAllText(file);
        return JsonConvert.DeserializeObject<T>(data, new JsonSerializerSettings
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
    }

    //public static object Deserialize(string file)
    //{
    //    file = System.IO.Path.Combine(Path, file);

    //    string data = File.ReadAllText(file);
    //    return JsonConvert.DeserializeObject(data);
    //}
}
