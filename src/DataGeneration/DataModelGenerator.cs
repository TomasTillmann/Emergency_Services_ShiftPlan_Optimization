using System;
using System.Collections.Generic;
using System.Linq;
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
      int goldenTimeSec,
      Random random = null
  )
  {
    random ??= new Random();
    WorldModel worldModel = new();

    // available medic teams
    worldModel.AvailableMedicTeams = Enumerable.Range(0, availableMedicTeamsCount).Select(_ => new MedicTeam()).ToList();
    //

    // available ambulances
    worldModel.AvailableAmbulances = Enumerable.Range(0, availableAmbulancesCount).Select(_ => new Ambulance()).ToList();
    //

    // depots
    worldModel.Depots = new List<DepotModel>(depotsCount);
    for (int i = 0; i < depotsCount; ++i)
    {
      CoordinateModel randomLoc = new()
      {
        XMet = random.Next(worldSize.XMet),
        YMet = random.Next(worldSize.YMet)
      };

      DepotModel depot = new DepotModel()
      {
        Location = randomLoc,
        Ambulances = new List<AmbulanceModel>()
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
        XMet = random.Next(worldSize.XMet),
        YMet = random.Next(worldSize.YMet)
      };

      worldModel.Hospitals.Add(new HospitalModel()
      {
        Location = randomLoc
      });
    }
    //

    // golden time
    worldModel.GoldenTimeSec = goldenTimeSec;
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
        XMet = random.Next(worldSize.XMet),
        YMet = random.Next(worldSize.YMet)
      };

      IncidentModel incident = new()
      {
        Location = randomLoc,
        OccurenceSec = random.Next(duration.Value),
        OnSceneDurationSec = (int)onSceneDurationDistribution.Sample(),
        InHospitalDeliverySec = (int)inHospitalDeliveryDistribution.Sample(),
      };

      incidents.Add(incident);
    }

    incidents.Sort((x, y) => x.OccurenceSec.CompareTo(y.OccurenceSec));
    return incidents.ToArray();
  }
}
