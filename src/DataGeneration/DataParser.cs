using ESSP.DataModel;
using Newtonsoft.Json;
using System.Linq;

namespace DataHandling;

public class DataParser
{
  private readonly WorldOptMapper _worldModelMapper = new();
  private readonly IncidentOptMapper _incidentModelMapper = new();

  public WorldOpt ParseWorldFromJson(string json)
  {
    WorldModel model = JsonConvert.DeserializeObject<WorldModel>(json);
    return _worldModelMapper.MapBack(model);
  }

  public string ParseWorldToJson(WorldOpt world)
  {
    WorldModel model = _worldModelMapper.Map(world);
    string json = JsonConvert.SerializeObject(model);
    return json;
  }

  public IncidentOpt[] ParseIncidentsFromJson(string json)
  {
    IncidentModel[] incidents = JsonConvert.DeserializeObject<IncidentModel[]>(json);
    return incidents.Select(inc => _incidentModelMapper.MapBack(inc)).ToArray();
  }

  public string ParseIncidentsToJson(IncidentOpt[] incidents)
  {
    IncidentModel[] incidentModels = incidents.Select(inc => _incidentModelMapper.Map(inc)).ToArray();
    string json = JsonConvert.SerializeObject(incidentModels);
    return json;
  }

}

