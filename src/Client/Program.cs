//define Cache
//#define All

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
  #if Cache
  public static void Main()
  {
    PragueInput input = new PragueInput();
    var world = input.GetWorld();
    var incidents = input.GetMondayIncidents(300, new Random(420));
    
    var t1 = Task.Run(() => new CacheSerializer(world, incidents, new RealDistanceCalculator(world.Hospitals)).SerializeFromDepotsToIncidents("prague_monday_420_300_DepotsToIncidents"));
    var t2 = Task.Run(() => new CacheSerializer(world, incidents, new RealDistanceCalculator(world.Hospitals)).SerializeFromIncidentsToHospitals("prague_monday_420_300_IncidentsToHospitals"));
    //var t3 = Task.Run(() => new CacheSerializer(world, incidents, new RealDistanceCalculator(world.Hospitals)).SerializeFromHospitalsToDepots("prague_HospitalsToDepots"));

    //Task[] tasks = [t1, t2, t3];
    
    Task[] tasks = [t1, t2];
    Task.WaitAll(tasks);
  }
  #endif
  
  #if All
  public static void Main()
  {
    const string logDir = "/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/StatsLog/prague_monday_420_300";
    
    var inference = Task.Run(() =>
    {
      return;
      Random random = new Random(420);
      PragueInput input = new PragueInput(random);
      var world = input.GetWorld();
      Constraints constraints = input.GetConstraints();

      var incidents = input.GetMondayIncidents(300, new Random(12));
      IDistanceCalculator distanceCalculator = new RealDistanceCalculator(
        world,
        incidents,
        "prague_monday_12_300_IncidentsToHospitals",
        "prague_monday_12_300_DepotsToIncidents",
        "prague_HospitalsToDepots"
      );
      
    //var t3 = Task.Run(() => new CacheSerializer(world, incidents, new RealDistanceCalculator(world.Hospitals)).SerializeFromHospitalsToDepots("prague_monday_420_300_HospitalsToDepots"));

      //string log = "OptimalMovesSearch.log";
      string bestPlansLog = "OptimalMovesSearchBestPlans.log";
      var planJson =
        "{\"Assignments\":[{\"MedicTeams\":[{\"Shift\":{\"StartSec\":14400,\"EndSec\":43200,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":50400,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":64800,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":72000,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":72000,\"EndSec\":86400,\"DurationSec\":14400}}],\"Ambulances\":[{},{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":36000,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":43200,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":57600,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}}],\"Ambulances\":[{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":0,\"EndSec\":28800,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":28800,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":64800,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":14400,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":57600,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":72000,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":57600,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":72000,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":14400,\"EndSec\":57600,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":64800,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":72000,\"EndSec\":86400,\"DurationSec\":14400}}],\"Ambulances\":[{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":14400,\"EndSec\":28800,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":43200,\"EndSec\":57600,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":72000,\"EndSec\":86400,\"DurationSec\":14400}}],\"Ambulances\":[{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":14400,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}}],\"Ambulances\":[{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":28800,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":64800,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":57600,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":72000,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}}],\"Ambulances\":[{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":21600,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":43200,\"EndSec\":64800,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":72000,\"EndSec\":86400,\"DurationSec\":14400}}],\"Ambulances\":[{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":36000,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":72000,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":43200,\"EndSec\":64800,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":43200,\"EndSec\":64800,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":79200,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":72000,\"EndSec\":86400,\"DurationSec\":14400}}],\"Ambulances\":[{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":14400,\"EndSec\":43200,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":43200,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":50400,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":43200,\"EndSec\":79200,\"DurationSec\":36000}}],\"Ambulances\":[{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":57600,\"EndSec\":72000,\"DurationSec\":14400}}],\"Ambulances\":[{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":43200,\"DurationSec\":14400}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":50400,\"DurationSec\":21600}}],\"Ambulances\":[{},{},{},{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":14400,\"EndSec\":43200,\"DurationSec\":28800}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":36000,\"DurationSec\":21600}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":72000,\"DurationSec\":43200}}],\"Ambulances\":[{},{}]},{\"MedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":43200,\"DurationSec\":43200}},{\"Shift\":{\"StartSec\":14400,\"EndSec\":50400,\"DurationSec\":36000}},{\"Shift\":{\"StartSec\":28800,\"EndSec\":64800,\"DurationSec\":36000}}],\"Ambulances\":[{},{},{}]}],\"AmbulancesCount\":60,\"MedicTeamsCount\":94,\"TotalShiftDuration\":2505600,\"AvailableMedicTeams\":[{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}},{\"Shift\":{\"StartSec\":0,\"EndSec\":0,\"DurationSec\":0}}],\"AvailableAmbulances\":[{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{},{}],\"Cost\":2505660}";
      var plan = JsonSerializer.Deserialize<EmergencyServicePlan>(planJson);

      Simulation simulation = new(world, constraints, distanceCalculator);
      
      var handled = simulation.Run(plan, incidents.AsSpan());
      Console.WriteLine($"handled: {handled}");
    });
    
    var optimalMovesSearch = Task.Run(() =>
    {
      return;
      string log = "OptimalMovesSearch.log";
      string bestPlansLog = "Plans_OptimalMovesSearch.log";
      
      Random random = new Random(420);
      PragueInput input = new PragueInput(random);
      var world = input.GetWorld();
      var incidents = input.GetMondayIncidents(300, new Random(420));
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
    
    var localSearch = Task.Run(() =>
    {
      return;
      string log = "LocalSearch.log";
      string bestPlansLog = "LocalSearchBestPlans.log";
      
      Random random = new Random(420);
      PragueInput input = new PragueInput(random);
      var world = input.GetWorld();
      var incidents = input.GetMondayIncidents(300, new Random(420));
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
    
    var localSearchHybrid = Task.Run(() =>
    {
      return;
      string log = "HybridLocalSearch.log";
      string bestPlansLog = "HybridLocalSearchBestPlans.log";
      
      Random random = new Random(420);
      PragueInput input = new PragueInput(random);
      var world = input.GetWorld();
      var incidents = input.GetMondayIncidents(300, new Random(420));
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
      return;
      string log = "SimulatedAnnealing_5_0000001_1_exp99_fromOptimal.log";
      string bestPlansLog = "Plans_" + log; 
      
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
      var optimizer = new SimulatedAnnealingOptimizer(world, constraints, distanceCalculator, utilityFunction, moveGenerator, 5, 0.000001, 1, coolingSchedule, random);

      
      var optimalOptimizer = new OptimalMovesSearchOptimizer(world, shiftTimes, distanceCalculator, constraints, 1, new Random(1));
      optimalOptimizer.Writer = writer;
      optimalOptimizer.BestPlansWriter = bestPlansWriter;
      var optimalInCost = optimalOptimizer.GetBest(incidents).First();
      optimizer.StartPlan = optimalInCost;
      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.GetBest(incidents).First();
    });
    
    var t5 = Task.Run(() =>
    {
      return;
      string log = "SimulatedAnnealing_100_1_10_exp80_fromRandom.log";
      string bestPlansLog = "SimulatedAnnealingBestPlans_100_1_10_exp80_fromRandom.log";
      
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
      ICoolingSchedule coolingSchedule = new ExponentialCoolingSchedule(0.8);
      var optimizer = new SimulatedAnnealingOptimizer(world, constraints, distanceCalculator, utilityFunction, moveGenerator, 10000, 0.0000001, 10, coolingSchedule, random);

      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.StartPlan = new PlanSamplerUniform(world, shiftTimes, constraints, 0.5, new Random(42)).Sample();
      optimizer.GetBest(incidents).First();
    });
    
    var t6 = Task.Run(() =>
    {
      return;
      string log = "SimulatedAnnealing_10000_1_30_exp90_fromRandom.log";
      string bestPlansLog = "SimulatedAnnealingBestPlans_10000_1_30_exp90_fromRandom.log";
      
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
      ICoolingSchedule coolingSchedule = new ExponentialCoolingSchedule(0.90);
      var optimizer = new SimulatedAnnealingOptimizer(world, constraints, distanceCalculator, utilityFunction, moveGenerator, 10000, 1, 30, coolingSchedule, random);

      optimizer.Writer = writer;
      optimizer.BestPlansWriter = bestPlansWriter;
      optimizer.StartPlan = new PlanSamplerUniform(world, shiftTimes, constraints, 0.5, new Random(42)).Sample();
      optimizer.GetBest(incidents).First();
    });
  }
  #endif
}
