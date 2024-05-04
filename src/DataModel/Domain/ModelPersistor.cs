using System.Collections.Generic;
using System.Text.Json;

namespace ESSP.DataModel;

public static class ModelPersistor
{
  public static string Serialize(WorldModel world, bool indent = true)
  {
    return JsonSerializer.Serialize(world, new JsonSerializerOptions { WriteIndented = indent });
  }

  public static string Serialize(IEnumerable<IncidentModel> incidents, bool indent = true)
  {
    return JsonSerializer.Serialize(incidents, new JsonSerializerOptions { WriteIndented = indent });
  }

  public static T Deserialize<T>(string json)
  {
    return JsonSerializer.Deserialize<T>(json);
  }
}
