using System.Collections.Immutable;
using ESSP.DataModel;

public class Input1 : IInputParametrization
{
  private readonly Random _random;
  private readonly DataModelGenerator _dataGenerator = new();

  public Input1(Random random = null)
  {
    _random = random ?? new Random();
  }

  public World GetWorld()
  {
    WorldMapper worldMapper = new();

    // World init
    World world = worldMapper.MapBack(_dataGenerator.GenerateWorldModel(
      worldSize: new CoordinateModel { XMet = 50_000, YMet = 50_000 },
      depotsCount: 10,
      hospitalsCount: 20,
      availableMedicTeamsCount: 60,
      availableAmbulancesCount: 200,
      goldenTimeSec: 15.ToMinutes().ToSeconds().Value,
      random: _random
    ));

    return world;
  }

  public ImmutableArray<Incident> GetIncidents()
  {
    // Incidents init
    IncidentMapper incidentMapper = new();
    ImmutableArray<Incident> incidents = _dataGenerator.GenerateIncidentModels(
      worldSize: new CoordinateModel { XMet = 50_000, YMet = 50_000 },
      incidentsCount: 250,
      duration: 21.ToHours().ToSeconds(),
      onSceneDurationNormalExpected: 20.ToMinutes().ToSeconds(),
      onSceneDurationNormalStddev: 10.ToMinutes().ToSeconds(),
      inHospitalDeliveryNormalExpected: 15.ToMinutes().ToSeconds(),
      inHospitalDeliveryNormalStddev: 10.ToMinutes().ToSeconds(),
      random: _random
    ).Select(inc => incidentMapper.MapBack(inc)).ToImmutableArray();


    // TODO: Move success rate as knob to optimizing
    var successRatedIncidents = new SuccessRatedIncidents { Value = incidents, SuccessRate = 1 };
    //

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
        8.ToHours().ToMinutes().ToSeconds().Value,
        12.ToHours().ToMinutes().ToSeconds().Value,
      },

      AllowedShiftDurationsSec = new HashSet<int>()
      {
        4.ToHours().ToMinutes().ToSeconds().Value,
        8.ToHours().ToMinutes().ToSeconds().Value,
        12.ToHours().ToMinutes().ToSeconds().Value,
      }
    };
    //

    return shiftTimes;
  }
}
