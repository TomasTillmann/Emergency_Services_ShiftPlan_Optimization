#define RunTabuSearch
//#define RunSimulatedAnnealing
//#define HowDoNeighboursLook
//#define HowDoesRandomSampleLook
//#define RunACO
//#define PlotConvergence
//#define RunReport
#define OptimizedSimul 


using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using DataHandling;
using DataModel;
using DataModel.Interfaces;
using ESSP.DataModel;
using Model.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Optimization;
using Optimizing;
using Simulating;
using SimulatingOptimized;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Client;

class Program
{
#if RunReport
    static void Main()
    {
        using Report report = new(Console.Out);
        
        World world = Worlds.GetNormal_1();
        Domain domain = Domains.GetStandardDomain();
        
        DataProvider dataProvider = new(20);
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(80, 23.ToHours(), successRateThreshold: 1)
        };
        
        List<IOptimizer> optimizers = new()
        {
            new TabuSearchOptimizer(world, domain),
            new SimulatedAnnealingOptimizer(world, domain),
        };
        
        report.Run(optimizers, incidents);
    }
#endif

#if RunTabuSearch
    static void Main()
    {
        using Visualizer visualizer = new(Console.Out);
        DataProvider dataProvider = new(20);
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(70, 23.ToHours(), successRateThreshold: 0.7)
        };

        visualizer.WriteSuccessRatedIncidentsSet(incidents);

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
            iterations: 30,
            tabuSize: 10,
            neighboursLimit: 30
        );

        IEnumerable<ShiftPlan> optimals = optimizer.FindOptimal(incidents);

        Simulation simulation = new(dataProvider.GetWorld());
        foreach(ShiftPlan optimal in optimals)
        {
            Statistics stats = simulation.Run(incidents.First().Value, optimal);
            visualizer.WriteStats(stats);
            visualizer.WriteGraph(optimal, 24.ToHours().ToSeconds());
        }
    }
#endif

#if RunSimulatedAnnealing
    static void Main()
    {
        using TextWriter writer = Console.Out;
        using Visualizer visualizer = new(writer);
        
        DataProvider dataProvider = new(50);
        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(200, 22.ToHours().ToSeconds() + 30.ToMinutes().ToSeconds(), successRateThreshold: 0.9)
        };
        
        visualizer.WriteSuccessRatedIncidentsSet(incidents);

        //List<SuccessRatedIncidents> incidents = new()
        //{
        //    new SuccessRatedIncidents(new List<Incident>
        //    {
        //        new Incident(Coordinate.FromMeters(10_000, 10_000), 60000.ToSeconds(), 3600.ToSeconds(), 200.ToSeconds(), IncidentType.Default),
        //        new Incident(Coordinate.FromMeters(30_000, 10_000), 1000.ToSeconds(), 3600.ToSeconds(), 200.ToSeconds(), IncidentType.Default)
        //    }, 1)
        //};

        SimulatedAnnealingOptimizer optimizer = new
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

        //IEnumerable<ShiftPlan> optimals = optimizer.FindOptimal(incidents);
        optimizer.InitStepThroughOptimizer(incidents);

        while (!optimizer.IsFinished())
        {
            writer.WriteLine($"Curr step: {optimizer.CurrStep}");
            writer.WriteLine($"Curr temp: {optimizer.CurrentTemperature}");
            writer.WriteLine($"Global best: {optimizer.GlobalBest}");
            writer.WriteLine($"Global best fitness: {optimizer.GlobalBestFitness}");
            writer.WriteLine($"Curr best: {optimizer.CurrentBest}");
            writer.WriteLine($"Curr best fitness: {optimizer.CurrentBestFitness}");
            writer.WriteLine("------------------");
            writer.Flush();
            
            optimizer.Step();
        }
        
        Simulation simulation = new(dataProvider.GetWorld());
        foreach(var optimal in optimizer.OptimalShiftPlans)
        {
            Statistics stats = simulation.Run(incidents.First().Value, optimal);
            visualizer.WriteStats(stats);
            visualizer.WriteGraph(optimal, 24.ToHours().ToSeconds());
        }
    }
#endif

#if RunACO
    static void Main()
    {
        const int ambulancesCount = 5;
        DataProvider dataProvider = new(ambulancesCount);
        Random random = new(42);

        List<SuccessRatedIncidents> incidents = new()
        {
            dataProvider.GetIncidents(15, 22.ToHours().ToSeconds() + 30.ToMinutes().ToSeconds(), successRateThreshold: 1)
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
            iterations: 60, 
            permutations: 10,
            initialPheromone: 0.1f,
            pheromoneEvaporationRate: 0.1f,
            alpha: 1,
            beta: 0.3f,
            simulationDuration: 24.ToHours(),
            estimatedMinimalShiftPlanDuration: incidents.First().Value.Sum(inc => inc.OnSceneDuration.Value + inc.InHospitalDelivery.Value).ToSeconds(),
            estimatedMaximalShiftPlanDuration: (12.ToHours().ToSeconds().Value * ambulancesCount).ToSeconds(),
            random: random,
            localSearchOptimizer: new TabuSearchOptimizer
            (
                world: dataProvider.GetWorld(),
                constraints: dataProvider.GetDomain(),
                iterations: 50,
                tabuSize: 50,
                neighboursLimit: 20,
                random: random
            )
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

        Simulating simulation = new(dataProvider.GetWorld());
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

#if OptimizedSimul
  static void Main()
  {
    Visualizer visualizer = new(Console.Out);
    WorldOptMapper worldMapper = new();
    DataModelGenerator dataGenerator = new();

    WorldOpt world = worldMapper.MapBack(dataGenerator.GenerateWorldModel(
      worldSize: new CoordinateModel { XMet = 50_000, YMet = 50_000 },
      depotsCount: 30,
      hospitalsCount: 20,
      ambulancesOnDepotNormalExpected: 20,
      ambulanceOnDepotNormalStddev: 10,
      ambTypes: new AmbulanceTypeModel[] {
        new AmbulanceTypeModel
        {
          Name = "A1",
          Cost = 400
        },
        new AmbulanceTypeModel
        {
          Name = "A2",
          Cost = 1000
        },
        new AmbulanceTypeModel
        {
          Name = "A3",
          Cost = 1200
        },
        new AmbulanceTypeModel
        {
          Name = "A4",
          Cost = 5000
        },
      },
      ambTypeCategorical: new double[] { 0.5, 0.3, 0.15, 0.05 },
      incToAmbTypesTable: new Dictionary<string, HashSet<string>>
      {
        { "I2", new HashSet<string> { "A2", "A3", "A4" } }
      },
      random: new Random(42)
    ));

    IncidentOptMapper incidentMapper = new();
    ImmutableArray<IncidentOpt> incidents = dataGenerator.GenerateIncidentModels(
      worldSize: new CoordinateModel { XMet = 50_000, YMet = 50_000 },
      incidentsCount: 5_000,
      duration: 22.ToHours().ToSeconds() + 30.ToMinutes().ToSeconds(),
      onSceneDurationNormalExpected: 20.ToMinutes().ToSeconds(),
      onSceneDurationNormalStddev: 10.ToMinutes().ToSeconds(),
      inHospitalDeliveryNormalExpected: 15.ToMinutes().ToSeconds(),
      inHospitalDeliveryNormalStddev: 10.ToMinutes().ToSeconds(),
      incTypes: new IncidentTypeModel[] {
        new IncidentTypeModel
        {
          Name = "I1",
          MaximumResponseTimeSec = 2.ToHours().ToMinutes().ToSeconds().Value
        },
        new IncidentTypeModel
        {
          Name = "I2",
          MaximumResponseTimeSec = 1.ToHours().ToMinutes().ToSeconds().Value
        },
        new IncidentTypeModel
        {
          Name = "I3",
          MaximumResponseTimeSec = 30.ToMinutes().ToSeconds().Value
        },
      },
      incTypesCategorical: new double[] { 0.7, 0.2, 0.1 },
      random: new Random(42)
    ).Select(inc => incidentMapper.MapBack(inc)).ToImmutableArray();

    // DataParser dataParser = new();

    // using StreamWriter worldWriter = new StreamWriter("world.json");
    // using StreamWriter incidentsWriter = new StreamWriter("incidents.json");

    // string json = dataParser.ParseWorldToJson(world);
    // worldWriter.WriteLine(json);

    // json = dataParser.ParseIncidentsToJson(incidents);
    // incidentsWriter.WriteLine(json);

    SimulationOptimized simulation = new(world);

    ShiftPlanOpt simulatedOn = ShiftPlanOpt.GetFrom(world.Depots, incidents.Length);

    Stopwatch sw = Stopwatch.StartNew();
    simulation.Run(incidents, simulatedOn);
    sw.Stop();

    // visualizer.WriteGraph(simulatedOn, 24.ToHours().ToSeconds());
    Console.WriteLine($"Simulating {incidents.Length} incidents on {simulatedOn.Shifts.Length} shifts took {sw.Elapsed}.");

    Console.WriteLine("-------");

    DataProvider dataProvider = new(640);
    List<Incident> incidentsOld = dataProvider.GetIncidents(5_000, 22.ToHours().ToSeconds() + 30.ToMinutes().ToSeconds());
    World worldOld = dataProvider.GetWorld();

    Simulation simulationOld = new(worldOld);
    ShiftPlan simulatedOnOld = ShiftPlan.ConstructFrom(worldOld.Depots, 0.ToSeconds(), 24.ToHours().ToSeconds());

    sw.Restart();
    Statistics stats = simulationOld.Run(incidentsOld.ToImmutableArray(), simulatedOnOld);
    sw.Stop();

    // visualizer.WriteGraph(simulatedOnOld, 24.ToHours().ToSeconds());
    Console.WriteLine($"Simulating {incidentsOld.Count} incidents on {simulatedOnOld.Shifts.Count} shifts took {sw.Elapsed}.");
    Console.WriteLine(stats);
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

    incidents = dataProvider.GetSuccessRatedIncidents(incidentsCount, 23.ToHours(), 1).Value;
  }

  [Benchmark]
  public void RunSimulation()
  {
    simulation.Run(incidents, shiftPlan);
  }
}

// RESULTS
// 50, 1000: 6,892,934.815 us
