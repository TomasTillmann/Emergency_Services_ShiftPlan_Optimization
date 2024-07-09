using System.Collections.Immutable;
using ESSP.DataModel;

public class TabuSearchTestInput(Random random = null) : IInputParametrization
{
    private readonly Random _random = random ?? new Random();
    private readonly DataModelGenerator _dataGenerator = new();
    private const int availalbeMedicTeamsCount = 50;
    private const int availableAmbulancesCount = 50;
    private const int depotsCount = 15;

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
            worldSize: new CoordinateModel { Longitude = 50_000, Latitude = 50_000 },
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
        ImmutableArray<Incident> incidents = _dataGenerator.GenerateIncidentModels(
            worldSize: new CoordinateModel { Longitude = 50_000, Latitude = 50_000 },
            incidentsCount: count,
            totalDuration: 21.ToHours().ToSeconds(),
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