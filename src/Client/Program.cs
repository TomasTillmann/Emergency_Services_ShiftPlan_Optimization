//#define RunTabuSearch
//#define RunSimulatedAnnealing
//#define HowDoNeighboursLook
//#define HowDoesRandomSampleLook
#define RunACO
//#define PlotConvergence

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

#if RunSimulatedAnnealing
    static void Main()
    {
        DataProvider dataProvider = new(50);
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(200, 22.ToHours().ToSeconds() + 30.ToMinutes().ToSeconds(), successRateThreshold: 1)
        };

        //List<SuccessRatedIncidents> incidents = new()
        //{
        //    new SuccessRatedIncidents(new List<Incident>
        //    {
        //        new Incident(Coordinate.FromMeters(10_000, 10_000), 60000.ToSeconds(), 3600.ToSeconds(), 200.ToSeconds(), IncidentType.Default),
        //        new Incident(Coordinate.FromMeters(30_000, 10_000), 1000.ToSeconds(), 3600.ToSeconds(), 200.ToSeconds(), IncidentType.Default)
        //    }, 1)
        //};

        IOptimizer optimizer = new SimulatedAnnealingOptimizer
        (
            world: dataProvider.GetWorld(),
            constraints: dataProvider.GetDomain(),
            lowestTemperature: 5,
            highestTemperature: 100,
            temperatureReductionFactor: 0.9,
            neighbourLimit: 30,
            new Random(42)
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

#if RunACO
    static void Main()
    {
        const int ambulancesCount = 30;
        DataProvider dataProvider = new(ambulancesCount);
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(80, 22.ToHours().ToSeconds() + 30.ToMinutes().ToSeconds(), successRateThreshold: 1)
        };

        //List<SuccessRatedIncidents> incidents = new()
        //{
        //    new SuccessRatedIncidents(new List<Incident>
        //    {
        //        new Incident(Coordinate.FromMeters(10_000, 10_000), occurence: 60000.ToSeconds(), 3600.ToSeconds(), 200.ToSeconds(), IncidentType.Default),
        //        new Incident(Coordinate.FromMeters(30_000, 10_000), occurence: 1000.ToSeconds(), 3600.ToSeconds(), 200.ToSeconds(), IncidentType.Default)
        //    }, 1)
        //};

        IOptimizer optimizer = new AntColonizationOptimizer
        (
            world: dataProvider.GetWorld(),
            constraints: dataProvider.GetDomain(),
            iterations: 200, 
            permutations: 10,
            initialPheromone: 0.3f,
            pheromoneEvaporationRate: 0.5f,
            alpha: 1,
            beta: 1f,
            simulationDuration: 24.ToHours(),
            estimatedMinimalShiftPlanDuration: incidents.First().Value.Sum(inc => inc.OnSceneDuration.Value + inc.InHospitalDelivery.Value).ToSeconds(),
            estimatedMaximalShiftPlanDuration: (12.ToHours().ToSeconds().Value * ambulancesCount).ToSeconds()
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
            Logger.Instance.WriteLineForce($"Cost: {optimal.GetCost()}");
        }
    }
#endif

#if PlotConvergence
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
            tabuSize: 50,
            neighboursLimit: 30
        );

        //Console.WriteLine(incidents.Visualize(separator: "\n"));
        Stopwatch sw = Stopwatch.StartNew();

        IEnumerable<ShiftPlan> optimals = optimizer.FindOptimal(incidents);

        Logger.Instance.WriteLineForce($"Optimizing took: {sw.ElapsedMilliseconds}ms.");

        Simulation simulation = new(dataProvider.GetWorld());
        foreach (var optimal in optimals)
        {
            Statistics stats = simulation.Run(incidents.First().Value, optimal);
            optimal.ShowGraph(24.ToHours().ToSeconds());
            Logger.Instance.WriteLineForce(stats);
            Logger.Instance.WriteLineForce();
        }
    }
#endif

#if HowDoNeighboursLook
    static void Main()
    {
        DataProvider dataProvider = new(30);
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(200, 22.ToHours().ToSeconds() + 30.ToMinutes().ToSeconds(), successRateThreshold: 1)
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
            iterations: 80,
            tabuSize: 20,
            neighboursLimit: 20,
            seed: 42
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

        optimal.ClearAllPlannedIncidents();
        Logger.Instance.WriteLineForce($"Optimal: {optimizer.Fitness(optimal, incidents)}");
        IEnumerable<Move> moves = traveler.GetNeighborhoodMoves(optimal);
        List<(int Fitness, Move Move)> fitnesses = new();

        foreach(Move move in moves)
        {
            ShiftPlan neighbor = traveler.ModifyMakeMove(optimal, move);

            int fitness = optimizer.Fitness(neighbor, incidents);
            fitnesses.Add((fitness, move));

            traveler.ModifyUnmakeMove(optimal, move);
        }

        fitnesses.Sort((a,b) => a.Fitness.CompareTo(b.Fitness));
        foreach(var fitness in fitnesses)
        {
            Logger.Instance.WriteLineForce($"Neighbor: {fitness.Fitness} | {fitness.Move}");
        }
    }
#endif

#if HowDoesRandomSampleLook
    static void Main()
    {
        DataProvider dataProvider = new(30);
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(3, 22.ToHours().ToSeconds() + 30.ToMinutes().ToSeconds(), successRateThreshold: 1)
        };

        IOptimizer optimizer = new TabuSearchOptimizer
        (
            world: dataProvider.GetWorld(),
            constraints: dataProvider.GetDomain(),
            iterations: 80,
            tabuSize: 20,
            neighboursLimit: 20,
            seed: 42
        );

        Random random = new(10);
        List<(int Fitness, ShiftPlan Random)> fitnesses = new();

        for(int i = 0; i < 50000; i++)
        {
            ShiftPlan randomSample = ShiftPlan.ConstructEmpty(dataProvider.GetDepots());
            foreach(Shift shift in randomSample.Shifts)
            {
                shift.Work
                    = Interval.GetByStartAndDuration(dataProvider.GetDomain().AllowedShiftStartingTimes.ToList().GetRandomElement(random),
                    dataProvider.GetDomain().AllowedShiftDurations.ToList().GetRandomElement(random));
            }

            int fitness = optimizer.Fitness(randomSample, incidents);
            fitnesses.Add((fitness, randomSample));
        }

        fitnesses.Sort((a,b) => a.Fitness.CompareTo(b.Fitness));
        foreach(var fitness in fitnesses)
        {
            Logger.Instance.WriteLineForce($"Random sample: {fitness.Fitness} | {fitness.Random}");
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
