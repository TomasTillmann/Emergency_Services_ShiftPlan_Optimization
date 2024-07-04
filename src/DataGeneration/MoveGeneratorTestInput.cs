using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ESSP.DataModel;

public class MoveGeneratorTestInput : IInputParametrization
{
  private readonly Random _random;
  private readonly DataModelGenerator _dataGenerator = new();
  private const int avaialbeMedicTeamsCount = 10;
  private const int availableAmbulancesCount = 5;
  private const int depotsCount = 3;

  public MoveGeneratorTestInput(Random random = null)
  {
    _random = random ?? new Random();
  }

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
      availableMedicTeamsCount: avaialbeMedicTeamsCount,
      availableAmbulancesCount: availableAmbulancesCount,
      random: _random
    ));

    return world;
  }

  public ImmutableArray<Incident> GetIncidents()
  {
    throw new NotImplementedException();
  }

  public ShiftTimes GetShiftTimes()
  {
    // shift times init
    ShiftTimes shiftTimes = new()
    {
      AllowedShiftStartingTimesSec = new HashSet<int>()
      {
        1, 2, 3, 4
      },

      AllowedShiftDurationsSec = new HashSet<int>()
      {
        10, 20, 30, 40
      }
    };
    //

    return shiftTimes;
  }
}