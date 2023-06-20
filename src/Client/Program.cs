using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
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

#if false
    static void Main()
    {
        DataProvider dataProvider = new();
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(5, 24.ToHours(), successRateThreshold: 1)
        };

        ExhaustiveOptimizer optimizer = new ExhaustiveOptimizer(dataProvider.GetWorld(), dataProvider.GetConstraints());

        Console.WriteLine(incidents.Visualize(separator: "\n"));
        Stopwatch sw = Stopwatch.StartNew();

        IEnumerable<ShiftPlan> optimalShiftPlans = optimizer.FindOptimal(incidents);

        Console.WriteLine("Celkem zabralo: " + (sw.ElapsedMilliseconds / 1000d) + "s");
        Console.WriteLine("Celkem prohledano: " + optimizer.SearchedShiftPlans);
        Console.WriteLine("Celkem splnujicich: " + optimizer.SatisfyingShiftPlans);
        Console.WriteLine("Nejvic optimalni: \n" + optimalShiftPlans.Visualize(separator: "\n"));
    }
#endif

#if false
    static void Main()
    {
        DataProvider dataProvider = new();
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(5, 24.ToHours(), successRateThreshold: 1)
        };

        Simulation simulation = new(dataProvider.GetWorld());

        ShiftPlan maximalShiftPlan = ShiftPlan.ConstructFrom(dataProvider.GetDepots(),
            dataProvider.GetConstraints().AllowedShiftStartingTimes.Min(),
            dataProvider.GetConstraints().AllowedShiftDurations.Max());
        SuccessRatedIncidents successRatedIncidents = dataProvider.GetIncidents(7, 24.ToHours(), 1);

        ExhaustiveOptimizer optimizer = new ExhaustiveOptimizer(dataProvider.GetWorld(), dataProvider.GetConstraints());
        var optimals = optimizer.FindOptimal(new List<SuccessRatedIncidents> { successRatedIncidents });

        simulation.Run(successRatedIncidents.Value, maximalShiftPlan);

        foreach(var incident in successRatedIncidents.Value)
        {
            Logger.Instance.WriteLineForce($"occurence: {incident.Occurence} | {incident.Occurence.Value / 60 / 60}");
        }

        maximalShiftPlan.ShowGraph(24.ToHours().ToSeconds());

        Logger.Instance.WriteLineForce();
        Logger.Instance.WriteLineForce();

        foreach(var optimal in optimals)
        {
            simulation.Run(successRatedIncidents.Value, optimal);
            optimal.ShowGraph(24.ToHours().ToSeconds());
            Logger.Instance.WriteLineForce();
            Logger.Instance.WriteLineForce();
        }
    }
#endif

#if false
    static void Main()
    {
        DataProvider dataProvider = new(ambulancesCount: 40);
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(1000, 23.ToHours(), successRateThreshold: 1)
        };

        Simulation simulation = new(dataProvider.GetWorld());

        ShiftPlan maximalShiftPlan = ShiftPlan.ConstructFrom(dataProvider.GetDepots(),
            dataProvider.GetConstraints().AllowedShiftStartingTimes.Min(),
            dataProvider.GetConstraints().AllowedShiftDurations.Max());

        Statistics stats = simulation.Run(incidents.First().Value, maximalShiftPlan);

        maximalShiftPlan.ShowGraph(24.ToHours().ToSeconds());

        Logger.Instance.WriteLineForce();
        Logger.Instance.WriteLineForce(stats);

        foreach(var incident in incidents.First().Value)
        {
            Logger.Instance.WriteLineForce($"occurence: {incident.Occurence} | {incident.Occurence.Value / 60 / 60}");
        }
    }
#endif

    // BENCHMARK OF SIMULATION
#if true
    static void Main()
    {
        BenchmarkRunner.Run<SimulationBenchmark>();
    }
#endif
}

//[SimpleJob(RuntimeMoniker.Net70)]
//[SimpleJob(RuntimeMoniker.NativeAot70)]
//[RPlotExporter]
public class SimulationBenchmark
{
    DataProvider dataProvider;
    Simulation simulation;

    [Params(50)]
    public int ambulancesCount = 10;

    [Params(1000)]
    public int incidentsCount = 10;

    List<Incident> incidents;
    ShiftPlan shiftPlan;

    [GlobalSetup]
    public void Setup()
    {
        dataProvider = new(ambulancesCount: ambulancesCount);

        simulation = new(dataProvider.GetWorld());

        shiftPlan = ShiftPlan.ConstructFrom(dataProvider.GetDepots(),
            dataProvider.GetConstraints().AllowedShiftStartingTimes.Min(),
            dataProvider.GetConstraints().AllowedShiftDurations.Max());

        incidents = dataProvider.GetIncidents(incidentsCount, 23.ToHours(), 1).Value;
    }

    [Benchmark]
    public void RunSimulation()
    {
        simulation.Run(incidents, shiftPlan);
    }
}

// RESULTS
// 50, 1000: 6,892,934.815 us
