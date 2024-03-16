//#define OptimizedSimul 
#define TestSimulatedAnnealing

using ESSP.DataModel;
using Model.Extensions;
using Optimization;
using Optimizing;
using Simulating;
using Simulating;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Client;

class Program
{
#if TestSimulatedAnnealing
  static void Main()
  {
    Visualizer visualizer = new(Console.Out);
    WorldMapper worldMapper = new();
    DataModelGenerator dataGenerator = new();

    World world = worldMapper.MapBack(dataGenerator.GenerateWorldModel(
      worldSize: new CoordinateModel { XMet = 50_000, YMet = 50_000 },
      depotsCount: 3,
      hospitalsCount: 20,
      ambulancesOnDepotNormalExpected: 10,
      ambulanceOnDepotNormalStddev: 1,
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

    //TODO: Add clipping so even if start time + duration sec > simulationTime it doesn't break anything
    Constraints constrains = new()
    {
      AllowedShiftStartingTimesSec = new HashSet<int>()
      {
        0, 4, 8, 12
      },
      AllowedShiftDurationsSec = new HashSet<int>()
      {
        8, 10, 12
      }
    };

    ILoss loss = new StandardLoss(world, incidents.Length);

    IStepOptimizer optimizer = new SimulatedAnnealingOptimizer
    (
      world: world,
      constraints: constrains,
      loss: loss
    );

    optimizer.InitStepOptimizer
    (
      new ImmutableArray<SuccessRatedIncidents>()
      {
        new SuccessRatedIncidents { Value = incidents, SuccessRate = 0.85 }
      }
    );

    Weights startWeights = ShiftPlan.GetFrom(world.Depots, incidents.Length).ToWeights();
    optimizer.StartWeights = startWeights;

    while (!optimizer.IsFinished())
    {
      optimizer.Step();
    }

    Weights optimalWeights = optimizer.OptimalWeights.First();
    ShiftPlan optimalShiftPlan = ShiftPlan.GetFrom(world.Depots, incidents.Length, optimalWeights);

    Simulation simulation = new(world);
    simulation.Run(incidents, optimalShiftPlan);

    visualizer.WriteGraph(optimalShiftPlan, 24.ToHours().ToSeconds());
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
