using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DistanceAPI;
using ESSP.DataModel;

public class Input1 : IInputParametrization
{
  private readonly Random _random;
  private readonly DataModelGenerator _dataGenerator = new();
  private const int avaialbeMedicTeamsCount = 50;
  private const int availableAmbulancesCount = 20;
  private const int depotsCount = 30;

  public Input1(Random random = null)
  {
    _random = random ?? new Random();
  }

  public Constraints GetConstraints()
  {
    return new Constraints
    {
      MaxTeamsPerDepotCount = Enumerable.Repeat(10, depotsCount).ToImmutableArray(),
      MaxAmbulancesPerDepotCount = Enumerable.Repeat(10, depotsCount).ToImmutableArray(),
    };
  }

  public World GetWorld()
  {
    // World init
    var worldModel = new WorldModel
    {
      Depots = new List<DepotModel>
      {
        
      }
    };

    return world;
  }

  public ImmutableArray<Incident> GetIncidents(int count)
  {
    // Incidents init
    ImmutableArray<Incident> incidents = _dataGenerator.GenerateIncidentModels(
      worldSize: new CoordinateModel { Longitude = 50_000, Latitude = 50_000 },
      incidentsCount: count,
      duration: 21.ToHours().ToSeconds(),
      onSceneDurationNormalExpected: 20.ToMinutes().ToSeconds(),
      onSceneDurationNormalStddev: 10.ToMinutes().ToSeconds(),
      inHospitalDeliveryNormalExpected: 15.ToMinutes().ToSeconds(),
      inHospitalDeliveryNormalStddev: 5.ToMinutes().ToSeconds(),
      random: _random
    ).Select(inc => IncidentMapper.MapBack(inc)).ToImmutableArray();

    return incidents;
  }

  public ShiftTimes GetShiftTimes()
  {
    // shift times init
    ShiftTimes shiftTimes = new()
    {
      AllowedShiftStartingTimesSec = new HashSet<int>()
      {
        0.ToHours().ToMinutes().ToSeconds().Value,
        //2.ToHours().ToMinutes().ToSeconds().Value,
        4.ToHours().ToMinutes().ToSeconds().Value,
        // 6.ToHours().ToMinutes().ToSeconds().Value,
        8.ToHours().ToMinutes().ToSeconds().Value,
        //10.ToHours().ToMinutes().ToSeconds().Value,
        12.ToHours().ToMinutes().ToSeconds().Value,
        //14.ToHours().ToMinutes().ToSeconds().Value,
        16.ToHours().ToMinutes().ToSeconds().Value,
        //18.ToHours().ToMinutes().ToSeconds().Value,
        //20.ToHours().ToMinutes().ToSeconds().Value,
      },

      AllowedShiftDurationsSec = new HashSet<int>()
      {
        4.ToHours().ToMinutes().ToSeconds().Value,
        6.ToHours().ToMinutes().ToSeconds().Value,
        8.ToHours().ToMinutes().ToSeconds().Value,
        10.ToHours().ToMinutes().ToSeconds().Value,
        12.ToHours().ToMinutes().ToSeconds().Value,
      }
    };
    //

    return shiftTimes;
  }
}
