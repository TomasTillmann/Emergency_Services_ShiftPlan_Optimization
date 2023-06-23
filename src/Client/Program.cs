//#define RunTabuSearch
#define Statistics

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
using Optimization;
using Optimizing;
using Simulating;
using System.Diagnostics;

namespace Client;

class Program
{
#if RunTabuSearch
    static void Main()
    {
        DataProvider dataProvider = new(20);
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(80, 23.ToHours(), successRateThreshold: 1)
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
            tabuSize: 20,
            neighboursLimit: 30
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

#if Statistics
    static void Main()
    {
        DataProvider dataProvider = new(20);
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(80, 23.ToHours(), successRateThreshold: 1)
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
            constraints: dataProvider.GetDomain(),
            iterations: 50,
            tabuSize: 20,
            neighboursLimit: 30
        );

        //Console.WriteLine(incidents.Visualize(separator: "\n"));
        Stopwatch sw = Stopwatch.StartNew();

        ShiftPlan optimal = optimizer.FindOptimal(incidents).First();

        Logger.Instance.WriteLineForce($"Optimizing took: {sw.ElapsedMilliseconds}ms.");

        Simulation simulation = new(dataProvider.GetWorld());
        Statistics stats = simulation.Run(incidents.First().Value, optimal);
        optimal.ShowGraph(24.ToHours().ToSeconds());
        Logger.Instance.WriteLineForce(stats);
        Logger.Instance.WriteLineForce();

        ShiftsTravel traveler = new(dataProvider.GetDomain());

        Logger.Instance.WriteLineForce($"Optimal: {optimizer.Fitness(optimal, incidents)}");
        IEnumerable<Move> moves = traveler.GetNeighborhoodMoves(optimal);
        foreach(Move move in moves)
        {
            ShiftPlan neighbor = traveler.ModifyMakeMove(optimal, move);

            int fitness = optimizer.Fitness(neighbor, incidents);
            Logger.Instance.WriteLineForce($"Neighbor: {fitness}");

            traveler.ModifyUnmakeMove(optimal, move);
        }
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
            dataProvider.GetDomain().AllowedShiftStartingTimes.Min(),
            dataProvider.GetDomain().AllowedShiftDurations.Max());

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
