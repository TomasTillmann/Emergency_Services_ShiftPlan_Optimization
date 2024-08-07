using System.Collections.Immutable;
using ESSP.DataModel;

public class EmergencyServicePlanTestInput(Random random = null) : IInputParametrization
{
    private readonly Random _random = random ?? new Random();
    private readonly DataModelGenerator _dataGenerator = new();
    private const int availalbeMedicTeamsCount = 200;
    private const int availableAmbulancesCount = 150;
    private const int depotsCount = 50;

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
            worldSize: new CoordinateModel { Longitude = 10_000, Latitude = 10_000 },
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