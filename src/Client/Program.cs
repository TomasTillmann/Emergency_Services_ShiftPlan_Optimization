using System.Collections.Immutable;
using DataModel.Interfaces;
using DistanceAPI;
using ESSP.DataModel;
using Optimizing;
using Simulating;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Random = System.Random;

namespace Client;

class Program
{
  public static void Main()
  {
    
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
      
      IUtilityFunction utilityFunction = new WeightedSum(new Simulation(world, constraints, distanceCalculator), EmergencyServicePlan.GetMaxCost(world, shiftTimes));
      var optimizer = new NaiveSolutionOptimizer(world, constraints, utilityFunction, shiftTimes, 1000, new Random(1));
      optimizer.GetBest(incidents);
  }
  
  public static void Main2()
  {
    const string logDir = "StatsLog"; // Choose logdir

    List<Task> tasks = new();
    var incidentsInference = Task.Run(() =>
    {
      PragueInput input = new PragueInput();
      var world = input.GetWorld();
      Constraints constraints = input.GetConstraints();

      
      const string plansDir = "/StatsLog/prague_420_300_results/"; // Choose plans dir
      string[] bestPlans = ["BestPlan_Optimal", "BestPlan_HybridLocal", "BestPlan_HybridTabu", "BestPlan_SA_fromEmpty", "BestPlan_SA_fromOptimal", "BestPlan_SA_fromRandom85", "bestPlan_SA_fromRandom90" ];

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
      PragueInput input = new PragueInput();
      var world = input.GetWorld();
      Constraints constraints = input.GetConstraints();

      const string plansDir = "StatsLog/prague_420_300_results/";
      string[] bestPlans = ["BestPlan_Optimal", "BestPlan_HybridLocal", "BestPlan_HybridTabu", "BestPlan_SA_fromEmpty", "BestPlan_SA_fromOptimal", "BestPlan_SA_fromRandom85", "bestPlan_SA_fromRandom90" ];

      foreach (var planString in bestPlans)
      {
        var plan = JsonSerializer.Deserialize<EmergencyServicePlan>(File.ReadAllText(Path.Join(plansDir, planString)));

        var incidentsTemp = input.GetIncidents(300, new Random(12)).ToList();
        var extraIncidents = input.GetExtraKlanoviceIncidents(10, new Random(12)).ToList();
        incidentsTemp.AddRange(extraIncidents);
        incidentsTemp.Sort((x, y) => x.OccurenceSec.CompareTo(y.OccurenceSec));
        var incidents = incidentsTemp.ToImmutableArray();
          
        var distanceCalculator = new RealDistanceCalculator(
          world.Hospitals
        );
        Simulation simulation = new(world, constraints, distanceCalculator);
        var handled = simulation.Run(plan, incidents.AsSpan());
        Console.WriteLine($"plan: {planString}, incidents: 12-300_extra30, handled: {handled}");
        
        
        incidentsTemp = input.GetIncidents(300, new Random(12)).ToList();
        extraIncidents = input.GetExtraKlanoviceIncidents(10, new Random(12)).ToList();
        incidentsTemp.AddRange(extraIncidents);
        incidentsTemp.Sort((x, y) => x.OccurenceSec.CompareTo(y.OccurenceSec));
        incidents = incidentsTemp.ToImmutableArray();
          
        distanceCalculator = new RealDistanceCalculator(
          world.Hospitals
        );
        simulation = new(world, constraints, distanceCalculator);
        handled = simulation.Run(plan, incidents.AsSpan());
        Console.WriteLine($"plan: {planString}, incidents: 12-300_extra20, handled: {handled}");
        
        
        incidentsTemp = input.GetIncidents(300, new Random(12)).ToList();
        extraIncidents = input.GetExtraKlanoviceIncidents(10, new Random(12)).ToList();
        incidentsTemp.AddRange(extraIncidents);
        incidentsTemp.Sort((x, y) => x.OccurenceSec.CompareTo(y.OccurenceSec));
        incidents = incidentsTemp.ToImmutableArray();
        
        distanceCalculator = new RealDistanceCalculator(
          world.Hospitals
        );
        simulation = new(world, constraints, distanceCalculator);
        handled = simulation.Run(plan, incidents.AsSpan());
        Console.WriteLine($"plan: {planString}, incidents: 12-300_extra30, handled: {handled}");
        
        
        incidentsTemp = input.GetIncidents(300, new Random(12)).ToList();
        extraIncidents = input.GetExtraKlanoviceIncidents(10, new Random(12)).ToList();
        incidentsTemp.AddRange(extraIncidents);
        incidentsTemp.Sort((x, y) => x.OccurenceSec.CompareTo(y.OccurenceSec));
        incidents = incidentsTemp.ToImmutableArray();

        distanceCalculator = new RealDistanceCalculator(
          world.Hospitals
        );
        simulation = new(world, constraints, distanceCalculator);
        handled = simulation.Run(plan, incidents.AsSpan());
        Console.WriteLine($"plan: {planString}, incidents: 12-300_extra40, handled: {handled}");
      }
    });
    tasks.Add(extraIncidentsInference);
    
    var optimalMovesSearch = Task.Run(() =>
    {
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
}
