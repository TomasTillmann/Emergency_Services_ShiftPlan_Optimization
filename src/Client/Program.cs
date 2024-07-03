#define AllOptimizers_Parametrized1
// #define simulation_test
//#define dynamic_programming
//#define serializing

using ESSP.DataModel;
using Optimizing;
using Simulating;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Client;

class Program
{
  private static readonly string _logsPath = @"/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/logs/";
  private static readonly StreamWriter _debug = new(_logsPath + "debug.log");

#if serializing
  static void Main()
  {
    const int seed = 42;
    Random random = new(seed);
    IInputParametrization input = new Input1(random);
    World world = input.GetWorld();
    ImmutableArray<Incident> incidents = input.GetIncidents();
    Constraints constraints = input.GetConstraints();
    Simulation simulation = new(world);
    ShiftTimes shiftTimes = input.GetShiftTimes();

    IOptimizer opt = new HillClimbOptimizer(world, constraints, shiftTimes, new StandardLoss(simulation, shiftTimes));
    opt.Debug = _debug;
    opt.FindOptimal(incidents);

    string worldJson = ModelPersistor.Serialize(WorldMapper.Map(world), false);
    File.WriteAllText(_logsPath + "json/" + $"World_Input1_{seed}.json", worldJson);

    string incidentsJson = ModelPersistor.Serialize(incidents.Select(incident => IncidentMapper.Map(incident)), false);
    File.WriteAllText(_logsPath + "json/" + $"Incidents_Input1_{seed}.json", incidentsJson);

    _debug.Dispose();
  }
#endif

#if AllOptimizers_Parametrized1
  public static void Main()
  {
    Random random = new Random(66);
    IInputParametrization input = new Input1(random);
    World world = input.GetWorld();
    Constraints constraints = input.GetConstraints();
    ShiftTimes shiftTimes = input.GetShiftTimes();

    ImmutableArray<Incident> incidents = input.GetIncidents();

    IOptimizer optimizer;
    Simulation simulation;
    IUtilityFunction utilityFunction;

    simulation = new(world, constraints);
    utilityFunction = new WeightedSum(simulation, EmergencyServicePlan.GetMaxCost(world, shiftTimes));
    IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
    optimizer = new LocalSearchOptimizer(world, constraints, utilityFunction, moveGenerator);

    Stopwatch sw = Stopwatch.StartNew();
    var optimals = optimizer.GetBest(incidents.AsSpan()).ToList();
    _debug.WriteLine("Elapsed: " + sw.Elapsed);
  }
#endif

#if simulation_test
  public static void Main()
  {
    Visualizer visualizer = new(_debug);

    IInputParametrization input = new Input1();
    World world = input.GetWorld();
    Constraints constraints = input.GetConstraints();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    ImmutableArray<Incident> incidents = input.GetIncidents();

    Simulation simulation = new(world);

    ILoss loss = new StandardLoss(simulation, shiftTimes, 0.999, 0.8);
    IOptimizer optimizer = new HillClimbOptimizer(world, constraints, shiftTimes, loss, steps: 40);
    optimizer.Debug = _debug;

    visualizer.PlotGraph(optimizer, optimizer.StartWeights, incidents);

    var optimal = optimizer.FindOptimal(incidents).First();

    visualizer.PlotGraph(optimizer, optimal, incidents);

    visualizer.Dispose();
  }
#endif

#if dynamic_programming
  public static void Main()
  {
    Visualizer visualizer = new(_debug);

    IInputParametrization input = new Input1(new Random(42));
    World world = input.GetWorld();
    Constraints constraints = input.GetConstraints();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    ImmutableArray<Incident> incidents = input.GetIncidents();

    Simulation simulation = new(world);

    ILoss loss = new StandardLoss(simulation, shiftTimes);
    IOptimizer optimizer = new DynamicProgrammingOptimizer(world, constraints, shiftTimes, loss);
    optimizer.Debug = _debug;

    var optimal = optimizer.FindOptimal(incidents).First();

    visualizer.PlotGraph(optimizer, optimal, incidents);

    visualizer.Dispose();
  }
#endif
}
