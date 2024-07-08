using System;
using System.Collections.Generic;
using System.Linq;
using DistanceAPI;
using MathNet.Numerics.Distributions;

namespace ESSP.DataModel;

public class DataModelGenerator
{
  public WorldModel GenerateWorldModel(
      CoordinateModel worldSize,
      int depotsCount,
      int hospitalsCount,
      int availableMedicTeamsCount,
      int availableAmbulancesCount,
      Random random = null
  )
  {
    random ??= new Random();
    WorldModel worldModel = new();

    // available medic teams
    worldModel.AvailableMedicTeams = Enumerable.Range(0, availableMedicTeamsCount).Select(_ => new MedicTeamModel()).ToList();
    //

    // available ambulances
    worldModel.AvailableAmbulances = Enumerable.Range(0, availableAmbulancesCount).Select(_ => new AmbulanceModel()).ToList();
    //

    // depots
    worldModel.Depots = new List<DepotModel>(depotsCount);
    for (int i = 0; i < depotsCount; ++i)
    {
      CoordinateModel randomLoc = new()
      {
        Longitude = random.NextDouble() * worldSize.Longitude,
        Latitude = random.NextDouble() * worldSize.Latitude
      };

      DepotModel depot = new DepotModel()
      {
        Location = randomLoc,
      };

      worldModel.Depots.Add(depot);
    }
    //

    // hospitals
    worldModel.Hospitals = new List<HospitalModel>(hospitalsCount);
    for (int i = 0; i < hospitalsCount; ++i)
    {
      CoordinateModel randomLoc = new()
      {
        Longitude = random.NextDouble() * worldSize.Longitude,
        Latitude = random.NextDouble() * worldSize.Latitude
      };

      worldModel.Hospitals.Add(new HospitalModel()
      {
        Location = randomLoc
      });
    }
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
      Random random = null
  )
  {
    random ??= new Random();
    Normal onSceneDurationDistribution = new(onSceneDurationNormalExpected.Value, onSceneDurationNormalStddev.Value, random);
    Normal inHospitalDeliveryDistribution = new(inHospitalDeliveryNormalExpected.Value, inHospitalDeliveryNormalStddev.Value, random);

    List<IncidentModel> incidents = new(incidentsCount);
    for (int i = 0; i < incidentsCount; ++i)
    {
      CoordinateModel randomLoc = new()
      {
        Longitude = random.NextDouble() * worldSize.Longitude,
        Latitude = random.NextDouble() * worldSize.Latitude
      };

      IncidentModel incident = new()
      {
        Location = randomLoc,
        OccurenceSec = random.Next(duration.Value),
        OnSceneDurationSec = Math.Min(Math.Max(10.ToMinutes().ToSeconds().Value, (int)onSceneDurationDistribution.Sample()), 30.ToMinutes().ToSeconds().Value),
        InHospitalDeliverySec = Math.Min(Math.Max(10.ToMinutes().ToSeconds().Value, (int)inHospitalDeliveryDistribution.Sample()), 30.ToMinutes().ToSeconds().Value),
        GoldTimeSec = 40.ToMinutes().ToSeconds().Value
      };

      incidents.Add(incident);
    }

    incidents.Sort((x, y) => x.OccurenceSec.CompareTo(y.OccurenceSec));
    return incidents.ToArray();
  }
}
