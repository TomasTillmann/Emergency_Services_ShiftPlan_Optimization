﻿//#define LocalSearch 
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
using JsonSerializer = System.Text.Json.JsonSerializer;
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
    const int seed = 15;
    PragueInput input = new PragueInput();
    var world = input.GetWorld();
    int count = 300;
    var incidents = input.GetStandardIncidents(count, new Random(seed));
    
    var t1 = Task.Run(() => new CacheSerializer(world, incidents, new RealDistanceCalculator(world.Hospitals)).SerializeFromDepotsToIncidents($"prague_monday_{seed}_{count}_DepotsToIncidents"));
    var t2 = Task.Run(() => new CacheSerializer(world, incidents, new RealDistanceCalculator(world.Hospitals)).SerializeFromIncidentsToHospitals($"prague_monday_{seed}_{count}_IncidentsToHospitals"));
    //var t3 = Task.Run(() => new CacheSerializer(world, incidents, new RealDistanceCalculator(world.Hospitals)).SerializeFromHospitalsToDepots("prague_HospitalsToDepots"));

    //Task[] tasks = [t1, t2, t3];
    
    Task[] tasks = [t1, t2];
    Task.WaitAll(tasks);
  }
  #endif
  
  #if All
  public static void Main()
  {
    const string logDir = "/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/StatsLog/prague_420_300/";
  

    List<Task> tasks = new();
    var incidentsInference = Task.Run(() =>
    {
      PragueInput input = new PragueInput();
      var world = input.GetWorld();
      Constraints constraints = input.GetConstraints();

      
      const string plansDir = "/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/StatsLog/prague_420_300_results/";
      //string[] bestPlans = ["BestPlan_Optimal", "BestPlan_HybridLocal", "BestPlan_HybridTabu"];
      string[] bestPlans = ["BestPlan_HybridLocal", "BestPlan_HybridTabu"];

      foreach (var planString in bestPlans)
      {
          var plan = JsonSerializer.Deserialize<EmergencyServicePlan>(File.ReadAllText(Path.Join(plansDir, planString)));
          
          var incidents = input.GetIncidents(300, new Random(12));
          var distanceCalculator = new RealDistanceCalculator(
            world,
            incidents,
            "prague_monday_12_300_IncidentsToHospitals",
            "prague_monday_12_300_DepotsToIncidents",
            "prague_HospitalsToDepots"
          );
          Simulation simulation = new(world, constraints, distanceCalculator);
          var handled = simulation.Run(plan, incidents.AsSpan());
          Console.WriteLine($"plan: {planString}, incidents: 12-300, handled: {handled}");
          
          incidents = input.GetIncidents(300, new Random(13));
          distanceCalculator = new RealDistanceCalculator(
            world,
            incidents,
            "prague_monday_13_300_IncidentsToHospitals",
            "prague_monday_13_300_DepotsToIncidents",
            "prague_HospitalsToDepots"
          );
          simulation = new(world, constraints, distanceCalculator);
          handled = simulation.Run(plan, incidents.AsSpan());
          Console.WriteLine($"plan: {planString}, incidents: 13-300, handled: {handled}");
          
          incidents = input.GetIncidents(300, new Random(14));
          distanceCalculator = new RealDistanceCalculator(
            world,
            incidents,
            "prague_monday_14_300_IncidentsToHospitals",
            "prague_monday_14_300_DepotsToIncidents",
            "prague_HospitalsToDepots"
          );
          simulation = new(world, constraints, distanceCalculator);
          handled = simulation.Run(plan, incidents.AsSpan());
          Console.WriteLine($"plan: {planString}, incidents: 14-300, handled: {handled}");
          
          incidents = input.GetIncidents(300, new Random(15));
          distanceCalculator = new RealDistanceCalculator(
            world,
            incidents,
            "prague_monday_15_300_IncidentsToHospitals",
            "prague_monday_15_300_DepotsToIncidents",
            "prague_HospitalsToDepots"
          );
          simulation = new(world, constraints, distanceCalculator);
          handled = simulation.Run(plan, incidents.AsSpan());
          Console.WriteLine($"plan: {planString}, incidents: 15-300, handled: {handled}");
          
          incidents = input.GetIncidents(330, new Random(15));
          distanceCalculator = new RealDistanceCalculator(
            world,
            incidents,
            "prague_monday_15_330_IncidentsToHospitals",
            "prague_monday_15_330_DepotsToIncidents",
            "prague_HospitalsToDepots"
          );
          simulation = new(world, constraints, distanceCalculator);
          handled = simulation.Run(plan, incidents.AsSpan());
          Console.WriteLine($"plan: {planString}, incidents: 15-330, handled: {handled}");
          
          incidents = input.GetIncidents(350, new Random(15));
          distanceCalculator = new RealDistanceCalculator(
            world,
            incidents,
            "prague_monday_15_350_IncidentsToHospitals",
            "prague_monday_15_350_DepotsToIncidents",
            "prague_HospitalsToDepots"
          );
          simulation = new(world, constraints, distanceCalculator);
          handled = simulation.Run(plan, incidents.AsSpan());
          Console.WriteLine($"plan: {planString}, incidents: 15-350, handled: {handled}");
          
          incidents = input.GetIncidents(370, new Random(15));
          distanceCalculator = new RealDistanceCalculator(
            world,
            incidents,
            "prague_monday_15_370_IncidentsToHospitals",
            "prague_monday_15_370_DepotsToIncidents",
            "prague_HospitalsToDepots"
          );
          simulation = new(world, constraints, distanceCalculator);
          handled = simulation.Run(plan, incidents.AsSpan());
          Console.WriteLine($"plan: {planString}, incidents: 15-370, handled: {handled}");
          
          incidents = input.GetIncidents(380, new Random(15));
          distanceCalculator = new RealDistanceCalculator(
            world,
            incidents,
            "prague_monday_15_380_IncidentsToHospitals",
            "prague_monday_15_380_DepotsToIncidents",
            "prague_HospitalsToDepots"
          );
          simulation = new(world, constraints, distanceCalculator);
          handled = simulation.Run(plan, incidents.AsSpan());
          Console.WriteLine($"plan: {planString}, incidents: 15-380, handled: {handled}");
          
          incidents = input.GetIncidents(390, new Random(15));
          distanceCalculator = new RealDistanceCalculator(
            world,
            incidents,
            "prague_monday_15_390_IncidentsToHospitals",
            "prague_monday_15_390_DepotsToIncidents",
            "prague_HospitalsToDepots"
          );
          simulation = new(world, constraints, distanceCalculator);
          handled = simulation.Run(plan, incidents.AsSpan());
          Console.WriteLine($"plan: {planString}, incidents: 15-390, handled: {handled}");
          
          incidents = input.GetIncidents(400, new Random(15));
          distanceCalculator = new RealDistanceCalculator(
            world,
            incidents,
            "prague_monday_15_400_IncidentsToHospitals",
            "prague_monday_15_400_DepotsToIncidents",
            "prague_HospitalsToDepots"
          );
          simulation = new(world, constraints, distanceCalculator);
          handled = simulation.Run(plan, incidents.AsSpan());
          Console.WriteLine($"plan: {planString}, incidents: 15-400, handled: {handled}");
      }
    });
    tasks.Add(incidentsInference);
    
    var extraIncidentsInference = Task.Run(() =>
    {
      return;
      PragueInput input = new PragueInput();
      var world = input.GetWorld();
      Constraints constraints = input.GetConstraints();

      var incidents = input.GetIncidents(300, new Random(12));
      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_12_300_IncidentsToHospitals",
        "prague_monday_12_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );

      string bestPlansLog = "OptimalMovesSearchBestPlans.log";
      var planJson =
        "{\"Assignments\":[{\"MedicTeams\":[{\"Shift\":{\"StartSec\":14400,\"EndSec\":43200,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":50400,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":64800,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":72000,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":72000,\"EndSec\":86400,\"DurationSec\":14400}}],\"Ambulances\":[{},{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":36000,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":43200,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":57600,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}}],\"Ambulances\":[{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":0,\"EndSec\":28800,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":28800,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":64800,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":14400,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":57600,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":72000,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":57600,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":72000,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":14400,\"EndSec\":57600,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":64800,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":72000,\"EndSec\":86400,\"DurationSec\":14400}}],\"Ambulances\":[{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":14400,\"EndSec\":28800,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":43200,\"EndSec\":57600,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":72000,\"EndSec\":86400,\"DurationSec\":14400}}],\"Ambulances\":[{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":14400,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}}],\"Ambulances\":[{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":28800,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":64800,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":57600,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":72000,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}}],\"Ambulances\":[{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":21600,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":43200,\"EndSec\":64800,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":72000,\"EndSec\":86400,\"DurationSec\":14400}}],\"Ambulances\":[{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":36000,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":72000,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":43200,\"EndSec\":64800,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":43200,\"EndSec\":64800,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":72000,\"EndSec\":86400,\"DurationSec\":14400}}],\"Ambulances\":[{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":14400,\"EndSec\":43200,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":43200,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":50400,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":43200,\"EndSec\":79200,\"DurationSec\":36000}}],\"Ambulances\":[{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":72000,\"DurationSec\":14400}}],\"Ambulances\":[{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":14400,\"EndSec\":43200,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":36000,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":72000,\"DurationSec\":43200}}],\"Ambulances\":[{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":50400,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":64800,\"DurationSec\":36000}}],\"Ambulances\":[{},{},{}]}],\"AmbulancesCount\":60,\"MedicTeamsCount\":94,\"TotalShiftDuration\":2505600,\"AvailableMedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}}],\"AvailableAmbulances\":[{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{}],\"Cost\":2505660}";
      var plan = JsonSerializer.Deserialize<EmergencyServicePlan>(planJson);

      Simulation simulation = new(world, constraints, distanceCalculator);
      
      var handled = simulation.Run(plan, incidents.AsSpan());
      Console.WriteLine($"handled: {handled}");
    });
    tasks.Add(extraIncidentsInference);
    
    var optimalMovesSearch = Task.Run(() =>
    {
      return;
      string log = "OptimalMovesSearch.log";
      string bestPlansLog = "Plans_OptimalMovesSearch.log";
      
      PragueInput input = new PragueInput();
      var world = input.GetWorld();
      var incidents = input.GetIncidents(300, new Random(420));
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_420_300_IncidentsToHospitals",
        "prague_monday_420_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );
      
      using var writer = new StreamWriter(Path.Join(logDir, log));
      using var bestPlansWriter = new StreamWriter(Path.Join(logDir, bestPlansLog));
      var optimalOptimizer = new OptimalMovesSearchOptimizer(world, shiftTimes, distanceCalculator, constraints, 10, new Random(1));
      optimalOptimizer.Writer = writer;
      optimalOptimizer.BestPlansWriter = bestPlansWriter;
      optimalOptimizer.GetBest(incidents).First();
    });
    tasks.Add(optimalMovesSearch);
    
    var localSearchEmpty = Task.Run(() =>
    {
      return;
      string log = "LocalSearch.log";
      string bestPlansLog = "Plans_LocalSearch.log";
      
      PragueInput input = new PragueInput();
      var world = input.GetWorld();
      var incidents = input.GetIncidents(300, new Random(420));
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_420_300_IncidentsToHospitals",
        "prague_monday_420_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );
      
      using var writer = new StreamWriter(Path.Join(logDir, log));
      using var bestPlansWriter = new StreamWriter(Path.Join(logDir, bestPlansLog));
      
      IUtilityFunction utilityFunction = new WeightedSum(new Simulation(world, constraints, distanceCalculator), EmergencyServicePlan.GetMaxCost(world, shiftTimes));
      IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
      var optimizer = new LocalSearchOptimizer(int.MaxValue, world, constraints, distanceCalculator, utilityFunction, moveGenerator);
      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.GetBest(incidents).First();
    });
    tasks.Add(localSearchEmpty);
    
    var localSearchHybrid = Task.Run(() =>
    {
      return;
      string log = "HybridLocalSearch.log";
      string bestPlansLog = "Plans_HybridLocalSearch.log";
      
      PragueInput input = new PragueInput();
      var world = input.GetWorld();
      var incidents = input.GetIncidents(300, new Random(420));
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_420_300_IncidentsToHospitals",
        "prague_monday_420_300_DepotsToIncidents",
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
      var optimizer = new LocalSearchOptimizer(int.MaxValue, world, constraints, distanceCalculator, utilityFunction, moveGenerator);
      optimizer.StartPlan = optimalInCost;
      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.GetBest(incidents).First();
    });
    tasks.Add(localSearchHybrid);
    
    var tabuSearchEmpty = Task.Run(() =>
    {
      return;
      string log = "TabuSearch_empty.log";
      string bestPlansLog = "Plans_TabuSearch_empty.log";
      
      PragueInput input = new PragueInput();
      var world = input.GetWorld();
      var incidents = input.GetIncidents(300, new Random(420));
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_420_300_IncidentsToHospitals",
        "prague_monday_420_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );
      
      using var writer = new StreamWriter(Path.Join(logDir, log));
      using var bestPlansWriter = new StreamWriter(Path.Join(logDir, bestPlansLog));
      
      IUtilityFunction utilityFunction = new WeightedSum(new Simulation(world, constraints, distanceCalculator), EmergencyServicePlan.GetMaxCost(world, shiftTimes));
      IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
      var optimizer = new TabuSearchOptimizer(world, constraints, distanceCalculator, utilityFunction, moveGenerator, 10000);
      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.GetBest(incidents).First();
    });
    tasks.Add(tabuSearchEmpty);
    
    var tabuSearchFromOptimal = Task.Run(() =>
    {
      return;
      string log = "HybridTabuSearch.log";
      string bestPlansLog = "Plans_HybridTabuSearch.log";
      
      PragueInput input = new PragueInput();
      var world = input.GetWorld();
      var incidents = input.GetIncidents(300, new Random(420));
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_420_300_IncidentsToHospitals",
        "prague_monday_420_300_DepotsToIncidents",
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
      var optimizer = new TabuSearchOptimizer(world, constraints, distanceCalculator, utilityFunction, moveGenerator, 10000);
      optimizer.StartPlan = optimalInCost;
      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.GetBest(incidents).First();
    });
    tasks.Add(tabuSearchFromOptimal);
    
    var sa_empty = Task.Run(() =>
    {
      return;
      string log = "SimulatedAnnealing_5_0000001_1_exp99_fromEmpty.log";
      string bestPlansLog = "Plans_" + log;
      
      PragueInput input = new PragueInput();
      var world = input.GetWorld();
      var incidents = input.GetIncidents(300, new Random(420));
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_420_300_IncidentsToHospitals",
        "prague_monday_420_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );
      
      using var writer = new StreamWriter(Path.Join(logDir, log));
      using var bestPlansWriter = new StreamWriter(Path.Join(logDir, bestPlansLog));
      
      IUtilityFunction utilityFunction = new WeightedSum(new Simulation(world, constraints, distanceCalculator), EmergencyServicePlan.GetMaxCost(world, shiftTimes));
      IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
      ICoolingSchedule coolingSchedule = new ExponentialCoolingSchedule(0.99);
      var optimizer = new SimulatedAnnealingOptimizer(world, constraints, distanceCalculator, utilityFunction, moveGenerator, 5, 0.000001, 1, coolingSchedule, new Random(1));
      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.GetBest(incidents).First();
    });
    tasks.Add(sa_empty);
    
    var sa_fromOptimal = Task.Run(() =>
    {
      return;
      string log = "SimulatedAnnealing_5_0000001_1_exp99_fromOptimal.log";
      string bestPlansLog = "Plans_" + log; 
      
      PragueInput input = new PragueInput();
      var world = input.GetWorld();
      var incidents = input.GetIncidents(300, new Random(420));
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_420_300_IncidentsToHospitals",
        "prague_monday_420_300_DepotsToIncidents",
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
      ICoolingSchedule coolingSchedule = new ExponentialCoolingSchedule(0.99);
      var optimizer = new SimulatedAnnealingOptimizer(world, constraints, distanceCalculator, utilityFunction, moveGenerator, 5, 0.000001, 1, coolingSchedule, new Random(1));
      optimizer.StartPlan = optimalInCost;
      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.GetBest(incidents).First();
    });
    tasks.Add(sa_fromOptimal);
    
    var sa_fromRandom_85 = Task.Run(() =>
    {
      return;
      string log = "SimulatedAnnealing_5_0000001_10_exp85_fromRandom.log";
      string bestPlansLog = "Plans_" + log;
      
      PragueInput input = new PragueInput();
      var world = input.GetWorld();
      var incidents = input.GetIncidents(300, new Random(420));
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_420_300_IncidentsToHospitals",
        "prague_monday_420_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );
      
      using var writer = new StreamWriter(Path.Join(logDir, log));
      using var bestPlansWriter = new StreamWriter(Path.Join(logDir, bestPlansLog));
      
      IUtilityFunction utilityFunction = new WeightedSum(new Simulation(world, constraints, distanceCalculator), EmergencyServicePlan.GetMaxCost(world, shiftTimes));
      IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
      ICoolingSchedule coolingSchedule = new ExponentialCoolingSchedule(0.85);
      var optimizer = new SimulatedAnnealingOptimizer(world, constraints, distanceCalculator, utilityFunction, moveGenerator, 5, 0.0000001, 10, coolingSchedule, new Random(1));

      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.StartPlan = new PlanSamplerUniform(world, shiftTimes, constraints, 0.5, new Random(1)).Sample();
      optimizer.GetBest(incidents).First();
    });
    tasks.Add(sa_fromRandom_85);
    
    var sa_fromRandom_90 = Task.Run(() =>
    {
      return;
      string log = "SimulatedAnnealing_3_000000001_30_exp90_fromRandom.log";
      string bestPlansLog = "Plan_" + log; 
      
      PragueInput input = new PragueInput();
      var world = input.GetWorld();
      var incidents = input.GetIncidents(300, new Random(420));
      Constraints constraints = input.GetConstraints();
      ShiftTimes shiftTimes = input.GetShiftTimes();

      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_420_300_IncidentsToHospitals",
        "prague_monday_420_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );
      
      using var writer = new StreamWriter(Path.Join(logDir, log));
      using var bestPlansWriter = new StreamWriter(Path.Join(logDir, bestPlansLog));
      
      IUtilityFunction utilityFunction = new WeightedSum(new Simulation(world, constraints, distanceCalculator), EmergencyServicePlan.GetMaxCost(world, shiftTimes));
      IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
      ICoolingSchedule coolingSchedule = new ExponentialCoolingSchedule(0.90);
      var optimizer = new SimulatedAnnealingOptimizer(world, constraints, distanceCalculator, utilityFunction, moveGenerator, 3, 0.000000001, 30, coolingSchedule, new Random(1));

      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.StartPlan = new PlanSamplerUniform(world, shiftTimes, constraints, 0.5, new Random(42)).Sample();
      optimizer.GetBest(incidents).First();
    });
    tasks.Add(sa_fromRandom_90);

    Task.WaitAll(tasks.ToArray());
  }
  #endif
}
