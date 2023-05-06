using DataHandling;
using DataModel.Interfaces;
using ESSP.DataModel;
using Newtonsoft.Json;
using Simulating;
using System;

namespace ESSP_Tests
{
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
        public void Test1()
        {
            return;

            // TODO: Deserializing doesn't work as expected
            World world = DataSerializer.Deserialize<World>("test1/world.json");
            DistanceCalculator distanceCalculator = DataSerializer.Deserialize<DistanceCalculator>("test1/distanceCalculator2D.json");
            Simulation simulation = new(world, distanceCalculator);

            ShiftPlan shiftPlan = DataSerializer.Deserialize<ShiftPlan>("test1/shiftPlan.json");
            Incidents incidents = DataSerializer.Deserialize<Incidents>("test1/incidents.json");


            Statistics stats = simulation.Run(incidents.Value, shiftPlan);


            Assert.That(shiftPlan.ToJson(), Is.EqualTo(File.ReadAllText(Path.Combine(DataSerializer.Path, "test1/shiftPlan_result.json"))));
            Assert.That(stats.ToJson(), Is.EqualTo(File.ReadAllText(Path.Combine(DataSerializer.Path, "test1/stats_result.json"))));
        }
    }
}
