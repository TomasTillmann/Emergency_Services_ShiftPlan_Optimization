using DataHandling;
using DataModel.Interfaces;
using ESSP.DataModel;
using Logging;
using Model.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Optimizing;
using Simulating;
using System.Diagnostics;

namespace Client;

class Program
{
#if false
    static void Main(string[] args)
    {
        DataProvider dataProvider = new();
        List<IncidentsSet> incidents = new()
        {
            dataProvider.GetIncidents(5, 24.ToHours(), successRateThreshold: 1)
        };

        IOptimizer optimizer = new ExhaustiveOptimizer(dataProvider.GetWorld(), dataProvider.GetConstraints());
        ShiftPlan optimalShiftPlan = optimizer.FindOptimal(dataProvider.GetShiftPlan(), incidents).FirstOrDefault();
    }
#endif

#if false
    static void Main(string[] args)
    {
        DataProvider dataProvider = new();
        World world = dataProvider.GetWorld();
        Incidents incidents = dataProvider.GetIncidents(100, 11.ToHours());
        ShiftPlan shiftPlan = dataProvider.GetShiftPlan();
        shiftPlan.ModifyToLargest(new Constraints(null, new List<Seconds> { 12.ToHours().ToSeconds() }));


        DataSerializer.Serialize(world, "test2/world.json");
        DataSerializer.Serialize(incidents, "test2/incidents.json");
        DataSerializer.Serialize(shiftPlan, "test2/shiftPlan.json");
        DataSerializer.Serialize(dataProvider.GetDistanceCalculator(), "test2/distanceCalculator2D.json");

        Simulation simulation = new(world, dataProvider.GetDistanceCalculator());
        Statistics stats = simulation.Run(incidents.Value, shiftPlan);

        DataSerializer.Serialize(stats, "test2/stats_result.json");
        DataSerializer.Serialize(shiftPlan, "test2/shiftPlan_result.json");
    }
#endif

#if true
    static void Main()
    {
        DataProvider dataProvider = new();
        List<IncidentsSet> incidents = new()
        {
            dataProvider.GetIncidents(5, 24.ToHours(), successRateThreshold: 1)
        };

        ExhaustiveOptimizer optimizer = new ExhaustiveOptimizer(dataProvider.GetWorld(), dataProvider.GetConstraints());

        Logger.Instance.WriteLineForce(incidents.Visualize(separator: "\n"));
        Stopwatch sw = Stopwatch.StartNew();

        IEnumerable<ShiftPlan> optimalShiftPlans = optimizer.FindOptimal(dataProvider.GetShiftPlan(), incidents);

        Logger.Instance.WriteLineForce("Celkem zabralo: " + (sw.ElapsedMilliseconds / 1000d) + "s");
        Logger.Instance.WriteLineForce("Celkem prohledano: " + optimizer.SearchedShiftPlans);
        Logger.Instance.WriteLineForce("Celkem splnujicich: " + optimizer.SatisfyingShiftPlans);
        Logger.Instance.WriteLineForce("Nejvic optimalni: \n" + optimalShiftPlans.Visualize(separator: "\n"));
    }
#endif
}
