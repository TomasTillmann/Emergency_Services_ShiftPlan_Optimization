using System.Collections.Generic;
using System.Linq;

namespace ESSP.DataModel;

public class DataModelGenerator
{
  private readonly WorldModelMapper _worldModelMapper = new();
  private readonly IncidentModelMapper _incidentModelMapper = new();

  public WorldOpt GenerateExampleWorld()
  {
    WorldModel world = new()
    {
      Depots = new()
      {
        new DepotModel
        {
          Location = new CoordinateModel
          {
            XMet = 100,
            YMet = 100
          },
          Ambulances = new()
          {
            new AmbulanceModel
            {
              Type = new AmbulanceTypeModel
              {
                Name = "A1",
                Cost = 400
              }
            },
            new AmbulanceModel
            {
              Type = new AmbulanceTypeModel
              {
                Name = "A1",
                Cost = 400
              }
            },
            new AmbulanceModel
            {
              Type = new AmbulanceTypeModel
              {
                Name = "A1",
                Cost = 400
              }
            }
          }
        }
      },

      Hospitals = new()
      {
        new HospitalModel
        {
          Location = new CoordinateModel
          {
            XMet = 5_000,
            YMet = 5_000
          }
        }
      },

      Table = new()
      {
        {"A1", new() { "I1", "I2" } },
        {"A2", new() { "I3" } },
      }
    };

    return _worldModelMapper.MapBack(world);
  }

  public IncidentOpt[] GenerateExampleIncidents()
  {
    List<IncidentModel> incidents = new()
    {
      new IncidentModel()
      {
        Location = new CoordinateModel
        {
          XMet = 2_000,
          YMet = 2_000
        },
        OccurenceSec = 100,
        OnSceneDurationSec = 700,
        InHospitalDeliverySec = 1000,
        Type = new IncidentTypeModel()
        {
          Name = "I1",
          MaximumResponseTimeSec = 5_000
        }
      },
      new IncidentModel()
      {
        Location = new CoordinateModel
        {
          XMet = 3_000,
          YMet = 3_000
        },
        OccurenceSec = 1000,
        OnSceneDurationSec = 700,
        InHospitalDeliverySec = 1000,
        Type = new IncidentTypeModel()
        {
          Name = "I2",
          MaximumResponseTimeSec = 5_000
        }
      }
    };

    return incidents.Select(inc => _incidentModelMapper.MapBack(inc)).ToArray();
  }
}
