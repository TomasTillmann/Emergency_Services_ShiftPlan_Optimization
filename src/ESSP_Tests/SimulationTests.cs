using DataHandling;
using DataModel.Interfaces;
using ESSP.DataModel;
using Newtonsoft.Json;
using Simulating;

namespace ESSP_Tests;

public static partial class Helpers
{
    public static string ToJson(this object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static string ToJsonPretty(this object obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }
}

public class SimulationTests
{
    [Test]
    public void WillRatherPayReroutePenaltyThanPlanOnCompletelyFreeShiftTest()
    {
        World world = DataSerializer.Deserialize<World>("test1/world.json");
        IDistanceCalculator distanceCalculator = DataSerializer.Deserialize<DistanceCalculator>("test1/distanceCalculator2D.json");
        world.DistanceCalculator = distanceCalculator;
        Simulation simulation = new(world);

        ShiftPlan shiftPlan = DataSerializer.Deserialize<ShiftPlan>("test1/shiftPlan.json");
        SuccessRatedIncidents incidents = DataSerializer.Deserialize<SuccessRatedIncidents>("test1/incidents.json");

        Statistics stats = simulation.Run(incidents.Value, shiftPlan);
        
        var s = JsonConvert.SerializeObject(shiftPlan);
        var x = JsonConvert.DeserializeObject<ShiftPlan>(s);

        ShiftPlan expectedShiftPlan = JsonConvert.DeserializeObject<ShiftPlan>(File.ReadAllText(Path.Combine(DataSerializer.Path, "test1/shiftPlan_result.json")));
        for (int i = 0; i < shiftPlan.Shifts.Count; ++i)
        {
            CollectionAssert.AreEquivalent(shiftPlan.Shifts[i].PlannedIncidents.ToJson(), expectedShiftPlan.Shifts[i].PlannedIncidents.ToJson());
            Assert.That(shiftPlan.Shifts[i].Work.ToJson(), Is.EqualTo(expectedShiftPlan.Shifts[i].Work.ToJson()));
        }

        Statistics expectedStats = JsonConvert.DeserializeObject<Statistics>(File.ReadAllText(Path.Combine(DataSerializer.Path, "test1/stats_result.json")));

        Assert.That(stats.UnhandledIncidents.ToJson(), Is.EqualTo(expectedStats.UnhandledIncidents.ToJson()));
        Assert.That(stats.SuccessRate, Is.EqualTo(expectedStats.SuccessRate));
    }

    [Test]
    public void IfIncidentsAndShiftsCountsAreSameAndLastOccuringIncidentCanBeHandledThanSuccessRateIs100Test()
    {
        World world = DataSerializer.Deserialize<World>("test2/world.json");
        IDistanceCalculator distanceCalculator = DataSerializer.Deserialize<DistanceCalculator>("test2/distanceCalculator2D.json");
        world.DistanceCalculator = distanceCalculator;
        Simulation simulation = new(world);

        ShiftPlan shiftPlan = DataSerializer.Deserialize<ShiftPlan>("test2/shiftPlan.json");
        SuccessRatedIncidents incidents = DataSerializer.Deserialize<SuccessRatedIncidents>("test2/incidents.json");

        Statistics stats = simulation.Run(incidents.Value, shiftPlan);

        ShiftPlan expectedShiftPlan = JsonConvert.DeserializeObject<ShiftPlan>(File.ReadAllText(Path.Combine(DataSerializer.Path, "test2/shiftPlan_result.json")));
        for (int i = 0; i < shiftPlan.Shifts.Count; ++i)
        {
            CollectionAssert.AreEquivalent(shiftPlan.Shifts[i].PlannedIncidents.ToJson(), expectedShiftPlan.Shifts[i].PlannedIncidents.ToJson());
            Assert.That(shiftPlan.Shifts[i].Work.ToJson(), Is.EqualTo(expectedShiftPlan.Shifts[i].Work.ToJson()));
        }

        Statistics expectedStats = JsonConvert.DeserializeObject<Statistics>(File.ReadAllText(Path.Combine(DataSerializer.Path, "test2/stats_result.json")));

        Assert.That(stats.UnhandledIncidents.ToJson(), Is.EqualTo(expectedStats.UnhandledIncidents.ToJson()));
        Assert.That(stats.SuccessRate, Is.EqualTo(expectedStats.SuccessRate));
    }
}
