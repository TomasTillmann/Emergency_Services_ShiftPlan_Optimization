using System.Collections.Immutable;
using ESSP.DataModel;

public class MoveGeneratorTestInput(Random random = null) : IInputParametrization
{
  private readonly Random _random = random ?? new Random();
  private readonly DataModelGenerator _dataGenerator = new();
  private const int availalbeMedicTeamsCount = 10;
  private const int availableAmbulancesCount = 5;
  private const int depotsCount = 3;

  public Constraints GetConstraints()
  {
    return new Constraints
    {
      MaxTeamsPerDepotCount = Enumerable.Repeat(30, depotsCount).ToImmutableArray(),
      MaxAmbulancesPerDepotCount = Enumerable.Repeat(20, depotsCount).ToImmutableArray(),
    };
  }

  public World GetWorld()
  {
    // World init
    World world = WorldMapper.MapBack(_dataGenerator.GenerateWorldModel(
      worldSize: new CoordinateModel { XMet = 10_000, YMet = 10_000 },
      depotsCount: depotsCount,
      hospitalsCount: 20,
      availableMedicTeamsCount: availalbeMedicTeamsCount,
      availableAmbulancesCount: availableAmbulancesCount,
      random: _random
    ));

    return world;
  }

  public ImmutableArray<Incident> GetIncidents(int count)
  {
    throw new NotImplementedException();
  }

  public ShiftTimes GetShiftTimes()
  {
    // shift times init
    ShiftTimes shiftTimes = new()
    {
      AllowedShiftStartingTimesSec = [1, 2, 3, 4],

      AllowedShiftDurationsSec = [10, 20, 30, 40]
    };
    //

    return shiftTimes;
  }
}