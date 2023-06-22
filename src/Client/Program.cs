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
    static void Main()
    {
        DataProvider dataProvider = new(50);
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(400, 23.ToHours(), successRateThreshold: 1)
        };

        //List<SuccessRatedIncidents> incidents = new()
        //{
        //    new SuccessRatedIncidents(new List<Incident>
        //    {
        //        new Incident(Coordinate.FromMeters(10_000, 10_000), 60000.ToSeconds(), 3600.ToSeconds(), 200.ToSeconds(), IncidentType.Default),
        //        new Incident(Coordinate.FromMeters(30_000, 10_000), 1000.ToSeconds(), 3600.ToSeconds(), 200.ToSeconds(), IncidentType.Default)
        //    }, 1)
        //};

        IOptimizer optimizer = new TabuSearchOptimizer
        (
            world: dataProvider.GetWorld(),
            constraints: dataProvider.GetConstraints(),
            iterations: 50,
            maxTabuSize: 20
        );

        //Console.WriteLine(incidents.Visualize(separator: "\n"));
        Stopwatch sw = Stopwatch.StartNew();

        IEnumerable<ShiftPlan> optimals = optimizer.FindOptimal(incidents);

        Logger.Instance.WriteLineForce($"Optimizing took: {sw.ElapsedMilliseconds}ms.");

        Simulation simulation = new(dataProvider.GetWorld());
        foreach(var optimal in optimals)
        {
            Statistics stats = simulation.Run(incidents.First().Value, optimal);
            optimal.ShowGraph(24.ToHours().ToSeconds());
            Logger.Instance.WriteLineForce(stats);
            Logger.Instance.WriteLineForce();
        }
    }
#endif

#if false
    static void Main()
    {
        DataProvider dataProvider = new(3);
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

#if true
    static void Main()
    {
        DataProvider dataProvider = new(ambulancesCount: 1000);
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(3000, 23.ToHours(), successRateThreshold: 1)
        };

        Simulation simulation = new(dataProvider.GetWorld());

        ShiftPlan maximalShiftPlan = ShiftPlan.ConstructFrom(dataProvider.GetDepots(),
            dataProvider.GetConstraints().AllowedShiftStartingTimes.Min(),
            dataProvider.GetConstraints().AllowedShiftDurations.Max());

        Stopwatch sw = new(); 
        for(int i = 0; i < 50; i++)
        {
            sw.Start();
            simulation.Run(incidents.First().Value, maximalShiftPlan);
            Logger.Instance.WriteLineForce($"{sw.ElapsedMilliseconds}ms");
            sw.Restart();
        }


        //Logger.Instance.WriteLineForce($"Simulation took: {sw.ElapsedMilliseconds}ms");

        //maximalShiftPlan.ShowGraph(24.ToHours().ToSeconds());

        //Logger.Instance.WriteLineForce();
        //Logger.Instance.WriteLineForce(stats);

        //foreach(var incident in incidents.First().Value)
        //{
        //    Logger.Instance.WriteLineForce($"occurence: {incident.Occurence} | {incident.Occurence.Value / 60 / 60}");
        //}
    }
#endif

    // BENCHMARK OF SIMULATION
#if false
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

    [Params(100)]
    public int ambulancesCount = 10;

    [Params(2000)]
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
