//#define LocalSearch 
//#define DynamicProgramming 
//#define TabuSearch
//#define SimulatedAnnealing
//#define Cache
#define All

using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using DataModel.Interfaces;
using DistanceAPI;
using ESSP.DataModel;
using Newtonsoft.Json;
using Optimizing;
using Simulating;
using Coordinate = GeoAPI.Geometries.Coordinate;
using Random = System.Random;

namespace Client;

class Program
{
#if LocalSearch
  public static void Main()
  {
    Random random = new Random(420);
    PragueInput input = new PragueInput(random);
    var world = input.GetWorld();
    Constraints constraints = input.GetConstraints();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    PlanSampler planSampler = new PlanSamperUniform(world, shiftTimes, constraints, 0.5, random);
    var incidents = input.GetMondayIncidents(300);
      
    IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "FromIncidentToHospitals_420_200_Prague",
        "FromDepotToIncidents_420_200_Prague",
        "FromDepotsToHospitals_420_200_Prague"
     );

    Simulation simulation = new(world, constraints, distanceCalculator);
    IUtilityFunction utilityFunction = new WeightedSum(simulation, EmergencyServicePlan.GetMaxCost(world, shiftTimes));
    IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
    var optimizer = new LocalSearchOptimizer(int.MaxValue, world, constraints, distanceCalculator, utilityFunction, moveGenerator);
    //optimizer.StartPlan = planSampler.Sample();

    Console.WriteLine($"Start time: {DateTime.Now}");
    var optimal = optimizer.GetBest(incidents.AsSpan()).ToList().First();
    double eval = utilityFunction.Evaluate(optimal, incidents.AsSpan());

    Console.WriteLine($"Iteration: {optimizer.PlateuIteration} " +
                      $"eval: {eval}," +
                      $"handled: {utilityFunction.HandledIncidentsCount} / {incidents.Length} " +
                      $"cost: {optimal.Cost}");
  }
#endif

#if TabuSearch
  public static void Main()
  {
    Random random = new Random(420);
    IInputParametrization input = new Input1(random);
    World world = input.GetWorld();
    Constraints constraints = input.GetConstraints();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    PlanSampler planSampler = new PlanSamperUniform(world, shiftTimes, constraints, 0.5, random);
    ImmutableArray<Incident> incidents = input.GetIncidents(100);

    Simulation simulation = new(world, constraints);
    IUtilityFunction utilityFunction = new WeightedSum(simulation, EmergencyServicePlan.GetMaxCost(world, shiftTimes));
    IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
    var optimizer = new TabuSearchOptimizer(world, constraints, utilityFunction, moveGenerator, tabuTenure: 1000);
    //optimizer.StartPlan = planSampler.Sample();

    Stopwatch sw = Stopwatch.StartNew();
    var optimal = optimizer.GetBest(incidents.AsSpan()).ToList().First();
    double eval = utilityFunction.Evaluate(optimal, incidents.AsSpan());

    Console.WriteLine($"Iteration: {optimizer.PlateuIteration} " +
                      $"eval: {eval}," +
                      $"handled: {utilityFunction.HandledIncidentsCount} / {incidents.Length} " +
                      $"cost: {optimal.Cost}");

    using StreamWriter writer = new("/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/log.txt");
    GaantView gaant = new GaantView(world, constraints);
    gaant.Show(optimal, incidents.AsSpan(), writer);
  }
#endif

#if SimulatedAnnealing
  public static void Main()
  {
    Random random = new Random(420);
    IInputParametrization input = new Input1(random);
    World world = input.GetWorld();
    Constraints constraints = input.GetConstraints();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    PlanSampler planSampler = new PlanSamperUniform(world, shiftTimes, constraints, 0.5, random);
    ImmutableArray<Incident> incidents = input.GetIncidents(100);

    Simulation simulation = new(world, constraints);
    IUtilityFunction utilityFunction = new WeightedSum(simulation, EmergencyServicePlan.GetMaxCost(world, shiftTimes));
    IMoveGenerator moveGenerator = new RandomBasicMoveSampler(shiftTimes, constraints);
    var optimizer = new SimulatedAnnealingOptimizer(
      world,
      constraints,
      utilityFunction,
      new RandomBasicMoveSampler(shiftTimes, constraints, random),
      100,
      10,
      200,
      new ExponentialCoolingSchedule(0.99),
      random
    );
    //optimizer.StartPlan = planSampler.Sample();

    Stopwatch sw = Stopwatch.StartNew();
    var optimal = optimizer.GetBest(incidents.AsSpan()).ToList().First();
    double eval = utilityFunction.Evaluate(optimal, incidents.AsSpan());

    Console.WriteLine($"Iteration: {optimizer.Iteration} " +
                      $"eval: {eval}," +
                      $"handled: {utilityFunction.HandledIncidentsCount} / {incidents.Length} " +
                      $"cost: {optimal.Cost}");
  }
#endif

#if DynamicProgramming
  public static void Main()
  {
    Random random = new(420);
    Input1 input = new Input1(random);
    var world = input.GetWorld();
    Constraints constraints = input.GetConstraints();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    var incidents = input.GetIncidents(200);

    IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
      world,
      incidents,
      "FromIncidentToHospitals_420_200_Prague",
      "FromDepotToIncidents_420_200_Prague",
      "FromDepotsToHospitals_420_200_Prague"
    );
    
    Simulation simulation = new(world, constraints, distanceCalculator);
    var optimizer = new OptimalMovesSearchOptimizer(world, shiftTimes, distanceCalculator, constraints, random);
    var optimal = optimizer.GetBest(incidents).First();

    simulation.Run(optimal, incidents.AsSpan());
    Console.WriteLine($"handled: {simulation.HandledIncidentsCount} / {incidents.Length} " +
                      $"cost: {optimal.Cost}");
  }
#endif
  
  #if Cache
  public static void Main()
  {
    Random random = new Random(111);
    PragueInput input = new PragueInput(random);
    var world = input.GetWorld();
    var incidents = input.GetIncidents(300);
    
    var t1 = Task.Run(() => new CacheSerializer(world, incidents, new RealDistanceCalculator(world.Hospitals)).SerializeFromDepotsToIncidents("prague_monday_111_300_DepotsToIncidents"));
    var t2 = Task.Run(() => new CacheSerializer(world, incidents, new RealDistanceCalculator(world.Hospitals)).SerializeFromIncidentsToHospitals("prague_monday_111_300_IncidentsToHospitals"));
    //var t3 = Task.Run(() => new CacheSerializer(world, incidents, new RealDistanceCalculator(world.Hospitals)).SerializeFromHospitalsToDepots("prague_monday_420_300_HospitalsToDepots"));

    //Task[] tasks = [t1, t2, t3];
    
    Task[] tasks = [t1, t2];
    Task.WaitAll(tasks);
  }
  #endif
  
  #if All
  public static void Main()
  {
    const string logDir = "/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/StatsLog/prague_monday_111_300";
    
    var t1 = Task.Run(() =>
    {
      return;
      Random random = new Random(420);
      PragueInput input = new PragueInput(random);
      var world = input.GetWorld();
      var incidents = input.GetMondayIncidents(300);
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_111_300_IncidentsToHospitals",
        "prague_monday_111_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );

      string log = "OptimalMovesSearch.log";
      string bestPlansLog = "OptimalMovesSearchBestPlans.log";
      
      using var writer = new StreamWriter(Path.Join(logDir, log));
      using var bestPlansWriter = new StreamWriter(Path.Join(logDir, bestPlansLog));
      var optimizer = new OptimalMovesSearchOptimizer(world, shiftTimes, distanceCalculator, constraints, 500, new Random(69));
      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.GetBest(incidents).First();
    });
    
    var t2 = Task.Run(() =>
    {
      return;
      string log = "HybridLocalSearch.log";
      string bestPlansLog = "HybridLocalSearchBestPlans.log";
      
      Random random = new Random(420);
      PragueInput input = new PragueInput(random);
      var world = input.GetWorld();
      var incidents = input.GetMondayIncidents(300);
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_111_300_IncidentsToHospitals",
        "prague_monday_111_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );
      
      using var writer = new StreamWriter(Path.Join(logDir, log));
      using var bestPlansWriter = new StreamWriter(Path.Join(logDir, bestPlansLog));
      
      using var writerLocal = new StreamWriter(Path.Join(logDir, "Local" + log));
      using var bestPlansWriterLocal = new StreamWriter(Path.Join(logDir, "Local" + bestPlansLog));
      
      var optimalOptimizer = new OptimalMovesSearchOptimizer(world, shiftTimes, distanceCalculator, constraints, 1, new Random(1));
      optimalOptimizer.Writer = writer;
      optimalOptimizer.BestPlansWriter = bestPlansWriter;
      var optimalInCost = optimalOptimizer.GetBest(incidents).First();
      
      IUtilityFunction utilityFunction = new WeightedSum(new Simulation(world, constraints, distanceCalculator), EmergencyServicePlan.GetMaxCost(world, shiftTimes));
      IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
      var optimizer = new LocalSearchOptimizer(int.MaxValue, world, constraints, distanceCalculator, utilityFunction, moveGenerator);
      optimizer.StartPlan = optimalInCost;
      optimizer.Writer = writerLocal;
      optimizer.BestPlansWriter = bestPlansWriterLocal;
      optimizer.GetBest(incidents).First();
    });
    
    var t3 = Task.Run(() =>
    {
      return;
      string log = "HybridTabuSearch.log";
      string bestPlansLog = "HybridTabuSearchBestPlans.log";
      
      Random random = new Random(420);
      PragueInput input = new PragueInput(random);
      var world = input.GetWorld();
      var incidents = input.GetMondayIncidents(300);
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_111_300_IncidentsToHospitals",
        "prague_monday_111_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );
      
      using var writer = new StreamWriter(Path.Join(logDir, log));
      using var bestPlansWriter = new StreamWriter(Path.Join(logDir, bestPlansLog));
      
      var optimalOptimizer = new OptimalMovesSearchOptimizer(world, shiftTimes, distanceCalculator, constraints, 1, new Random(1));
      optimalOptimizer.Writer = writer;
      optimalOptimizer.BestPlansWriter = bestPlansWriter;
      var optimalInCost = optimalOptimizer.GetBest(incidents).First();
      
      IUtilityFunction utilityFunction = new WeightedSum(new Simulation(world, constraints, distanceCalculator), EmergencyServicePlan.GetMaxCost(world, shiftTimes));
      IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
      var optimizer = new TabuSearchOptimizer(world, constraints, distanceCalculator, utilityFunction, moveGenerator, 100000);
      optimizer.StartPlan = optimalInCost;
      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.GetBest(incidents).First();
    });
    
    var t4 = Task.Run(() =>
    {
      string log = "SimulatedAnnealing_100_1_10_exp99_fromEmpty.log";
      string bestPlansLog = "SimulatedAnnealingBestPlans_100_1_10_exp99_fromEmpty.log";
      
      Random random = new Random(420);
      PragueInput input = new PragueInput(random);
      var world = input.GetWorld();
      var incidents = input.GetMondayIncidents(300);
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_111_300_IncidentsToHospitals",
        "prague_monday_111_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );
      
      using var writer = new StreamWriter(Path.Join(logDir, log));
      using var bestPlansWriter = new StreamWriter(Path.Join(logDir, bestPlansLog));
      
      IUtilityFunction utilityFunction = new WeightedSum(new Simulation(world, constraints, distanceCalculator), EmergencyServicePlan.GetMaxCost(world, shiftTimes));
      IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
      ICoolingSchedule coolingSchedule = new ExponentialCoolingSchedule(0.99);
      var optimizer = new SimulatedAnnealingOptimizer(world, constraints, distanceCalculator, utilityFunction, moveGenerator, 100, 1, 10, coolingSchedule, random);

      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.GetBest(incidents).First();
    });
    
    var t5 = Task.Run(() =>
    {
      string log = "SimulatedAnnealing_100_1_10_exp99_fromRandom.log";
      string bestPlansLog = "SimulatedAnnealingBestPlans_100_1_10_exp99_fromRandom.log";
      
      Random random = new Random(420);
      PragueInput input = new PragueInput(random);
      var world = input.GetWorld();
      var incidents = input.GetMondayIncidents(300);
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_111_300_IncidentsToHospitals",
        "prague_monday_111_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );
      
      using var writer = new StreamWriter(Path.Join(logDir, log));
      using var bestPlansWriter = new StreamWriter(Path.Join(logDir, bestPlansLog));
      
      IUtilityFunction utilityFunction = new WeightedSum(new Simulation(world, constraints, distanceCalculator), EmergencyServicePlan.GetMaxCost(world, shiftTimes));
      IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
      ICoolingSchedule coolingSchedule = new ExponentialCoolingSchedule(0.99);
      var optimizer = new SimulatedAnnealingOptimizer(world, constraints, distanceCalculator, utilityFunction, moveGenerator, 100, 1, 10, coolingSchedule, random);

      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.StartPlan = new PlanSamplerUniform(world, shiftTimes, constraints, 0.5, new Random(42)).Sample();
      optimizer.GetBest(incidents).First();
    });
    
    var t6 = Task.Run(() =>
    {
      string log = "SimulatedAnnealing_100_1_30_exp97_fromRandom.log";
      string bestPlansLog = "SimulatedAnnealingBestPlans_100_1_30_exp97_fromRandom.log";
      
      Random random = new Random(420);
      PragueInput input = new PragueInput(random);
      var world = input.GetWorld();
      var incidents = input.GetMondayIncidents(300);
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_111_300_IncidentsToHospitals",
        "prague_monday_111_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );
      
      using var writer = new StreamWriter(Path.Join(logDir, log));
      using var bestPlansWriter = new StreamWriter(Path.Join(logDir, bestPlansLog));
      
      IUtilityFunction utilityFunction = new WeightedSum(new Simulation(world, constraints, distanceCalculator), EmergencyServicePlan.GetMaxCost(world, shiftTimes));
      IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
      ICoolingSchedule coolingSchedule = new ExponentialCoolingSchedule(0.97);
      var optimizer = new SimulatedAnnealingOptimizer(world, constraints, distanceCalculator, utilityFunction, moveGenerator, 100, 1, 30, coolingSchedule, random);

      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.StartPlan = new PlanSamplerUniform(world, shiftTimes, constraints, 0.5, new Random(42)).Sample();
      optimizer.GetBest(incidents).First();
    });

    Task.WaitAll([t1, t2, t3, t4, t5, t6]);
  }
  #endif
}
