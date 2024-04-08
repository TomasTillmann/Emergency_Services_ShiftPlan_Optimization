using System;
using System.Collections.Generic;
using MathNet.Numerics.Distributions;

namespace ESSP.DataModel;

public class DataModelGenerator
{
  public WorldModel GenerateWorldModel(
      CoordinateModel worldSize,
      int depotsCount,
      int hospitalsCount,
      int ambulancesOnDepotNormalExpected,
      int ambulanceOnDepotNormalStddev,
      AmbulanceTypeModel[] ambTypes,
      double[] ambTypeCategorical,
      Dictionary<string, HashSet<string>> incToAmbTypesTable,
      Random random = null
  )
  {
    random ??= new Random();
    Normal ambulancesOnDepotDistribution = new(ambulancesOnDepotNormalExpected, ambulanceOnDepotNormalStddev, random);
    Categorical ambTypesDistribution = new(ambTypeCategorical, random);

    WorldModel worldModel = new();

    // depots
    worldModel.Depots = new List<DepotModel>(depotsCount);
    for (int i = 0; i < depotsCount; ++i)
    {
      CoordinateModel randomLoc = new()
      {
        XMet = random.Next(worldSize.XMet),
        YMet = random.Next(worldSize.YMet)
      };

      int ambCount = (int)ambulancesOnDepotDistribution.Sample();
      DepotModel depot = new DepotModel()
      {
        Location = randomLoc,
        Ambulances = new List<AmbulanceModel>(ambCount)
      };

      for (int j = 0; j < ambCount; ++j)
      {
        depot.Ambulances.Add(new AmbulanceModel()
        {
          Type = ambTypes[ambTypesDistribution.Sample()]
        });
      }

      worldModel.Depots.Add(depot);
    }
    //

    // hospitals
    worldModel.Hospitals = new List<HospitalModel>(hospitalsCount);
    for (int i = 0; i < hospitalsCount; ++i)
    {
      CoordinateModel randomLoc = new()
      {
        XMet = random.Next(worldSize.XMet),
        YMet = random.Next(worldSize.YMet)
      };

      worldModel.Hospitals.Add(new HospitalModel()
      {
        Location = randomLoc
      });
    }
    //

    // inc to amb types table
    worldModel.AmbTypes = ambTypes;
    worldModel.IncToAmbTypesTable = incToAmbTypesTable;
    //

    return worldModel;
  }

  public IncidentModel[] GenerateIncidentModels(
      CoordinateModel worldSize,
      int incidentsCount,
      Seconds duration,
      Seconds onSceneDurationNormalExpected,
      Seconds onSceneDurationNormalStddev,
      Seconds inHospitalDeliveryNormalExpected,
      Seconds inHospitalDeliveryNormalStddev,
      IncidentTypeModel[] incTypes,
      double[] incTypesCategorical,
      Random random = null
  )
  {
    random ??= new Random();
    Normal onSceneDurationDistribution = new(onSceneDurationNormalExpected.Value, onSceneDurationNormalStddev.Value, random);
    Normal inHospitalDeliveryDistribution = new(inHospitalDeliveryNormalExpected.Value, inHospitalDeliveryNormalStddev.Value, random);
    Categorical incTypesDistribution = new(incTypesCategorical, random);

    List<IncidentModel> incidents = new(incidentsCount);
    for (int i = 0; i < incidentsCount; ++i)
    {
      CoordinateModel randomLoc = new()
      {
        XMet = random.Next(worldSize.XMet),
        YMet = random.Next(worldSize.YMet)
      };

      IncidentModel incident = new()
      {
        Location = randomLoc,
        OccurenceSec = random.Next(duration.Value),
        OnSceneDurationSec = (int)onSceneDurationDistribution.Sample(),
        InHospitalDeliverySec = (int)inHospitalDeliveryDistribution.Sample(),
        Type = incTypes[incTypesDistribution.Sample()]
      };

      incidents.Add(incident);
    }

    incidents.Sort((x, y) => x.OccurenceSec.CompareTo(y.OccurenceSec));
    return incidents.ToArray();
  }

  public WorldModel GenerateExampleWorld()
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

      IncToAmbTypesTable = new()
      {
        {"A1", new() { "I1", "I2" } },
        {"A2", new() { "I3" } },
      }
    };

    return world;
  }

  public IncidentModel[] GenerateExampleIncidents()
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

    return incidents.ToArray();
  }
}
