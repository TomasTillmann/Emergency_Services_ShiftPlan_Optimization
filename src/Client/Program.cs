//#define TabuSearchInput1
#define SAInput1

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

#if TabuSearchInput1
  static void Main()
  {
    using Visualizer visualizer = new(Console.Out);

    // specific input selection
    IInputParametrization inputParametrization = new Input1();
    //

    // input parsing
    Input input = inputParametrization.Get();
    World world = input.World;
    Constraints constraints = input.Constraints;
    ImmutableArray<SuccessRatedIncidents> successRatedIncidents = input.SuccessRatedIncidents;
    //

    // Optimizer init
    ILoss loss = new StandardLoss(world, constraints);

    TabuSearchOptimizer optimizer = new
    (
      world: world,
      constraints: constraints,
      loss: loss,
      iterations: 300,
      tabuSize: 50,
      random: new Random(42)
    );
    //

    // optimizer init
    optimizer.InitStepOptimizer
    (
     successRatedIncidents
    );
    //

    // Optimizing
    Console.WriteLine($"Amb count: {world.AllAmbulancesCount}");
    Stopwatch sw = Stopwatch.StartNew();
    int step = 0;
    while (!optimizer.IsFinished())
    {
      visualizer.PlotGraph(optimizer.CurrentWeights, world, successRatedIncidents.First().Value, _debug);
      Console.WriteLine($"Step: {++step}");
      optimizer.Step();
      _debug.WriteLine($"Current best move: {optimizer.CurrentBestMove}");
      _debug.WriteLine($"Cost: {ShiftPlan.GetFrom(world.Depots, optimizer.CurrentWeights).GetCost()}");
    }
    sw.Stop();
    _debug.WriteLine($"Optimizing took: {sw.Elapsed}");
    _debug.WriteLine($"one step took: {sw.Elapsed.TotalSeconds / (double)step}s");

    visualizer.PlotGraph(optimizer.OptimalWeights.First(), world, successRatedIncidents.First().Value, _debug);
    _debug.WriteLine($"Optimal shift plan cost: {ShiftPlan.GetFrom(world.Depots, optimizer.OptimalWeights.First()).GetCost()}");

    // disposing
    optimizer.Dispose();
    ((StandardLoss)loss)._debug.Dispose();
    _debug.Dispose();
  }
#endif

#if SAInput1
  static void Main()
  {
    using Visualizer visualizer = new(Console.Out);

    // specific input selection
    IInputParametrization inputParametrization = new Input1();
    //

    // input parsing
    Input input = inputParametrization.Get();
    World world = input.World;
    Constraints constraints = input.Constraints;
    ImmutableArray<SuccessRatedIncidents> successRatedIncidents = input.SuccessRatedIncidents;
    //

    // Optimizer init
    ILoss loss = new StandardLoss(world, constraints);

    SimulatedAnnealingOptimizer optimizer = new
    (
      world: world,
      constraints: constraints,
      loss: loss,
      random: new Random(42)
    );
    //

    // optimizer init
    optimizer.InitStepOptimizer
    (
     successRatedIncidents
    );
    //

    // Optimizing
    Console.WriteLine($"Amb count: {world.AllAmbulancesCount}");
    Stopwatch sw = Stopwatch.StartNew();
    int step = 0;
    while (!optimizer.IsFinished())
    {
      visualizer.PlotGraph(optimizer.CurrentWeights, world, successRatedIncidents.First().Value, _debug);
      Console.WriteLine($"Step: {++step}");
      optimizer.Step();
      _debug.WriteLine($"Current best move: {optimizer.CurrentMove}");
      _debug.WriteLine($"Cost: {ShiftPlan.GetFrom(world.Depots, optimizer.CurrentWeights).GetCost()}");
    }
    sw.Stop();
    _debug.WriteLine($"Optimizing took: {sw.Elapsed}");
    _debug.WriteLine($"one step took: {sw.Elapsed.TotalSeconds / (double)step}s");

    visualizer.PlotGraph(optimizer.OptimalWeights.First(), world, successRatedIncidents.First().Value, _debug);
    _debug.WriteLine($"Optimal shift plan cost: {ShiftPlan.GetFrom(world.Depots, optimizer.OptimalWeights.First()).GetCost()}");

    // disposing
    optimizer.Dispose();
    ((StandardLoss)loss)._debug.Dispose();
    _debug.Dispose();
  }
#endif
}
