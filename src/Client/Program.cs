//#define OptimizedSimul 
#define TestSimulatedAnnealing

using ESSP.DataModel;
using Optimization;
using Optimizing;
using Simulating;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Client;

class Program
{
  private static readonly StreamWriter _debug = new(@"/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/logs/" + "program.log");

#if TestSimulatedAnnealing
  static void Main()
  {
    Visualizer visualizer = new(Console.Out);
    WorldMapper worldMapper = new();
    DataModelGenerator dataGenerator = new();

    // World and constraints init
    World world = worldMapper.MapBack(dataGenerator.GenerateWorldModel(
      worldSize: new CoordinateModel { XMet = 50_000, YMet = 50_000 },
      depotsCount: 10,
      hospitalsCount: 20,
      ambulancesOnDepotNormalExpected: 6,
      ambulanceOnDepotNormalStddev: 1,
      ambTypes: new AmbulanceTypeModel[] {
        new AmbulanceTypeModel
        {
          Name = "A1",
          Cost = 40
        },
       new AmbulanceTypeModel
       {
         Name = "A2",
         Cost = 100
       },
       new AmbulanceTypeModel
       {
         Name = "A3",
         Cost = 120
       },
       new AmbulanceTypeModel
       {
         Name = "A4",
         Cost = 500
       },
      },
      ambTypeCategorical: new double[] { 0.5, 0.3, 0.15, 0.05 },
      incToAmbTypesTable: new Dictionary<string, HashSet<string>>
      {
        { "I1", new HashSet<string> { "A1", "A2", "A3", "A4" } },
        //{ "I2", new HashSet<string> { "A2", "A3", "A4" } }
      },
      random: new Random(42)
    ));

    IncidentMapper incidentMapper = new();
    ImmutableArray<Incident> incidents = dataGenerator.GenerateIncidentModels(
      worldSize: new CoordinateModel { XMet = 50_000, YMet = 50_000 },
      incidentsCount: 200,
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
       // new IncidentTypeModel
       // {
       //   Name = "I2",
       //   MaximumResponseTimeSec = 1.ToHours().ToMinutes().ToSeconds().Value
       // },
       // new IncidentTypeModel
       // {
       //   Name = "I3",
       //   MaximumResponseTimeSec = 30.ToMinutes().ToSeconds().Value
       // },
      },
      //incTypesCategorical: new double[] { 0.7, 0.2, 0.1 },
      incTypesCategorical: new double[] { 1 },
      random: new Random(42)
    ).Select(inc => incidentMapper.MapBack(inc)).ToImmutableArray();

    //TODO: Add clipping so even if start time + duration sec > simulationTime it doesn't break anything
    Constraints constrains = new()
    {
      AllowedShiftStartingTimesSec = new HashSet<int>()
      {
        0.ToHours().ToMinutes().ToSeconds().Value,
        4.ToHours().ToMinutes().ToSeconds().Value,
        8.ToHours().ToMinutes().ToSeconds().Value,
        12.ToHours().ToMinutes().ToSeconds().Value,
      },

      AllowedShiftDurationsSec = new HashSet<int>()
      {
        8.ToHours().ToMinutes().ToSeconds().Value,
        12.ToHours().ToMinutes().ToSeconds().Value,
      }
    };
    //

    // Optimizer init
    ILoss loss = new StandardLoss(world, incidents.Length);

    IStepOptimizer optimizer = new TabuSearchOptimizer
    (
      world: world,
      constraints: constrains,
      loss: loss,
      iterations: 200,
      tabuSize: 1000,
      random: new Random(42)
    );
    //

    // Success rated incidents init
    var successRatedIncidents = new List<SuccessRatedIncidents>()
    {
      new SuccessRatedIncidents { Value = incidents, SuccessRate = 1 }
    }.ToImmutableArray();
    //

    // optimizer init
    optimizer.InitStepOptimizer
    (
     successRatedIncidents
    );
    //

    // Optimizing
    Stopwatch sw = Stopwatch.StartNew();
    int step = 0;
    while (!optimizer.IsFinished())
    {
      Console.WriteLine($"Step {++step}");
      optimizer.Step();
    }
    sw.Stop();
    _debug.WriteLine($"Optimizing took: {sw.Elapsed}");
    _debug.WriteLine($"one step took: {sw.Elapsed.Seconds / step}s");
    //

    // Visualization of found optimal shift plan on trained incidents set
    Weights optimalWeights = optimizer.OptimalWeights.First();
    ShiftPlan optimalShiftPlan = ShiftPlan.GetFrom(world.Depots, optimalWeights);

    Simulation simulation = new(world);
    simulation.Run(incidents, optimalShiftPlan);

    visualizer.WriteGraph(optimalShiftPlan, 24.ToHours().ToSeconds(), _debug);
    _debug.WriteLine($"Success rate: {simulation.SuccessRate}");
    //

    optimizer.Dispose();
    _debug.Dispose();
  }
#endif

#if OptimizedSimul
  static void Main()
  {
    Visualizer visualizer = new(Console.Out);
    WorldMapper worldMapper = new();
    DataModelGenerator dataGenerator = new();

    World world = worldMapper.MapBack(dataGenerator.GenerateWorldModel(
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

    IncidentMapper incidentMapper = new();
    ImmutableArray<Incident> incidents = dataGenerator.GenerateIncidentModels(
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

    ShiftPlan simulatedOn = ShiftPlan.GetFrom(world.Depots, incidents.Length);

    Stopwatch sw = Stopwatch.StartNew();
    Simulation simulation = new(world);
    sw.Stop();
  }
#endif
}
