using System;
using System.Collections.Generic;
using System.Linq;
using DistanceAPI;
using NetTopologySuite.Geometries;
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
  
  public List<CoordinateModel> GetRandomIncidentsLocationsInPolygon(
    Polygon polygon,
    int incidentsCount,
    Random random = null
  )
  {
    random ??= new();
    RandomCoordinateGenerator generator = new(random);
    return Enumerable.Range(0, incidentsCount).Select(_ => generator.GenerateRandomCoordinateIn(polygon)).ToList();
  }

  public IncidentModel[] GenerateIncidentModelsFromCoordinates(
      IEnumerable<CoordinateModel> coordinates,
      Seconds totalDuration,
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

    List<IncidentModel> incidents = new();
    foreach (var coordinate in coordinates)
    {
      incidents.Add(GetIncidentModelOnLocation(
        coordinate,
        totalDuration,
        onSceneDurationDistribution,
        inHospitalDeliveryDistribution,
        random
        )
      );
    }

    incidents.Sort((x, y) => x.OccurenceSec.CompareTo(y.OccurenceSec));
    return incidents.ToArray();
  }

  public IncidentModel[] GenerateIncidentModels(
      CoordinateModel worldSize,
      int incidentsCount,
      Seconds totalDuration,
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
        Latitude = random.NextDouble() * worldSize.Latitude,
        Longitude = random.NextDouble() * worldSize.Longitude,
      };

      incidents.Add(GetIncidentModelOnLocation(
        randomLoc,
        totalDuration,
        onSceneDurationDistribution,
        inHospitalDeliveryDistribution,
        random
        )
      );
    }

    incidents.Sort((x, y) => x.OccurenceSec.CompareTo(y.OccurenceSec));
    return incidents.ToArray();
  }
  
  public IncidentModel GetIncidentModelOnLocation(
    CoordinateModel location,
    Seconds totalDuration,
    Normal onSceneDurationDistribution,
    Normal inHospitalDeliveryDistribution,
    Random random = null
  )
  {
      return new IncidentModel
      {
        Location = location,
        OccurenceSec = random.Next(totalDuration.Value),
        OnSceneDurationSec = Math.Min(Math.Max(10.ToMinutes().ToSeconds().Value, (int)onSceneDurationDistribution.Sample()), 30.ToMinutes().ToSeconds().Value),
        InHospitalDeliverySec = Math.Min(Math.Max(10.ToMinutes().ToSeconds().Value, (int)inHospitalDeliveryDistribution.Sample()), 30.ToMinutes().ToSeconds().Value),
        GoldTimeSec = 40.ToMinutes().ToSeconds().Value
      };
  }
}
