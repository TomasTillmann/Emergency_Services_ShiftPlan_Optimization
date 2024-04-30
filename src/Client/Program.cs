#define AllOptimizers_Parametrized1
// #define simulation_test
//#define dynamic_programming

using ESSP.DataModel;
using Optimization;
using Optimizing;
using Simulating;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Client;

class Program
{
  private static readonly string _logsPath = @"/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/logs/";
  private static readonly StreamWriter _debug = new(_logsPath + "debug.log");

#if AllOptimizers_Parametrized1
  static void Main()
  {
    Visualizer visualizer = new(_debug);

    Random random = new Random(42);
    IInputParametrization input = new Input1(random);
    World world = input.GetWorld();
    Constraints constraints = input.GetConstraints();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    ImmutableArray<Incident> incidents = input.GetIncidents();
    Weights startWeights = Weights.GetUniformlyRandom(world, constraints, shiftTimes, random);

    IOptimizer optimizer;
    Simulation simulation;
    ILoss loss;
    List<IOptimizer> optimizers = new();

    simulation = new(world);
    loss = new StandardLoss(simulation, shiftTimes);
    visualizer.PlotGraph(loss, startWeights, incidents, _debug);

    simulation = new(world);
    loss = new StandardLoss(simulation, shiftTimes);
    optimizer = new HillClimbOptimizer(world, constraints, shiftTimes, loss, random: random);
    optimizer.StartWeights = startWeights;
    optimizer.Debug = _debug;
    optimizers.Add(optimizer);

    simulation = new(world);
    loss = new StandardLoss(simulation, shiftTimes);
    optimizer = new TabuSearchOptimizer(world, constraints, shiftTimes, loss, random: random);
    optimizer.StartWeights = startWeights;
    optimizer.Debug = _debug;
    optimizers.Add(optimizer);

    simulation = new(world);
    loss = new StandardLoss(simulation, shiftTimes);
    optimizer = new DynamicProgrammingOptimizer(world, constraints, shiftTimes, loss, random: random);
    optimizer.StartWeights = startWeights;
    optimizer.Debug = _debug;
    optimizers.Add(optimizer);

    simulation = new(world);
    loss = new StandardLoss(simulation, shiftTimes);
    optimizer = new SimulatedAnnealingOptimizer(world, constraints, shiftTimes, loss, random: random);
    optimizer.StartWeights = startWeights;
    optimizer.Debug = _debug;
    optimizers.Add(optimizer);

    foreach (IOptimizer opt in optimizers)
    {
      _debug.WriteLine($"{opt.GetType().Name}: ");
      Stopwatch sw = Stopwatch.StartNew();
      var optimal = opt.FindOptimal(incidents).First();
      _debug.WriteLine("Elapsed: " + sw.Elapsed);
      visualizer.PlotGraph(loss, optimal, incidents);
    }

    visualizer.Dispose();
    _debug.Dispose();
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
