//#define TabuSearchInput1
//#define SAInput1
//#define HillClimb_Parametrized1
//#define TabuSearch_Parametrized1
#define AllOptimizers_Parametrized1

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
    using Visualizer visualizer = new(Console.Out);

    // specific input selection
    IInputParametrization inputParametrization = new Input1();
    //

    // dont change the incidents set
    Input inputInitial = inputParametrization.Get(new Random(666));
    ImmutableArray<SuccessRatedIncidents> successRatedIncidents = inputInitial.SuccessRatedIncidents;
    //

    foreach (int randomSeed in Enumerable.Range(0, 100))
    {
      Random random = new Random(randomSeed);

      // input parametrization
      Input input = inputParametrization.Get(new Random());

      World world = input.World;
      Constraints constraints = input.Constraints;
      //

      // Optimizer init
      ILoss loss = new StandardLoss(world, constraints);

      // optimizers init
      List<IOptimizer> optimizers = new()
      {
        new HillClimbOptimizer
        (
          world: world,
          constraints: constraints,
          loss: loss,
          steps: 199,
          random: random
        ),
        new TabuSearchOptimizer
        (
          world: world,
          constraints: constraints,
          loss: loss,
          iterations: 50,
          tabuSize: 12,
          random: random
        ),
        new SimulatedAnnealingOptimizer
        (
          world: world,
          constraints: constraints,
          loss: loss,
          random: random
        )
      };
      //

      foreach (IOptimizer optimizer in optimizers)
      {
        optimizer.Debug = new StreamWriter(_logsPath + optimizer.GetType().Name + randomSeed + ".log");

        // plot starting weights
        visualizer.PlotGraph(optimizer.StartWeights, world, successRatedIncidents.First().Value, optimizer.Debug);

        optimizer.Debug.WriteLine($"Amb count: {world.AllAmbulancesCount}");

        // Optimizing
        Stopwatch sw = Stopwatch.StartNew();
        Weights optimalWeights = optimizer.FindOptimal(successRatedIncidents).First();
        sw.Stop();

        // writing result
        visualizer.PlotGraph(optimalWeights, world, successRatedIncidents.First().Value, optimizer.Debug);
        optimizer.Debug.WriteLine($"Optimizing took: {sw.Elapsed}");

        optimizer.Debug.Dispose();
      }
    }
  }
#endif

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

#if HillClimb_Parametrized1
  static void Main()
  {
    using Visualizer visualizer = new(Console.Out);

    // specific input selection
    IInputParametrization inputParametrization = new Input1();
    //

    // dont change the incidents set
    Input inputInitial = inputParametrization.Get(new Random(666));
    ImmutableArray<SuccessRatedIncidents> successRatedIncidents = inputInitial.SuccessRatedIncidents;
    //

    foreach (int i in Enumerable.Range(0, 100))
    {
      Random random = new Random(i);

      // input parametrization
      Input input = inputParametrization.Get(new Random());

      World world = input.World;
      Constraints constraints = input.Constraints;
      //

      // Optimizer init
      ILoss loss = new StandardLoss(world, constraints);

      HillClimbOptimizer optimizer = new
      (
        world: world,
        constraints: constraints,
        loss: loss,
        steps: 200,
        random: random
      );

      optimizer.Debug = _debug;
      //

      // plot starting weights
      visualizer.PlotGraph(optimizer.StartWeights, world, successRatedIncidents.First().Value, _debug);
      //

      // Optimizing
      Console.WriteLine($"Amb count: {world.AllAmbulancesCount}");

      Stopwatch sw = Stopwatch.StartNew();
      Weights optimalWeights = optimizer.FindOptimal(successRatedIncidents).First();
      sw.Stop();
      //

      // writing result
      visualizer.PlotGraph(optimalWeights, world, successRatedIncidents.First().Value, _debug);
      _debug.WriteLine($"Optimizing took: {sw.Elapsed}");
      //

      optimizer.Dispose();
    }

    _debug.Dispose();
  }
#endif

#if TabuSearch_Parametrized1
  static void Main()
  {
    using Visualizer visualizer = new(Console.Out);

    // specific input selection
    IInputParametrization inputParametrization = new Input1();
    //

    // dont change the incidents set
    Input inputInitial = inputParametrization.Get(new Random(666));
    ImmutableArray<SuccessRatedIncidents> successRatedIncidents = inputInitial.SuccessRatedIncidents;
    //

    foreach (int i in Enumerable.Range(0, 100))
    {
      Random random = new Random(i);

      // input parametrization
      Input input = inputParametrization.Get(new Random());

      World world = input.World;
      Constraints constraints = input.Constraints;
      //

      // Optimizer init
      ILoss loss = new StandardLoss(world, constraints);

      TabuSearchOptimizer optimizer = new
      (
        world: world,
        constraints: constraints,
        loss: loss,
        iterations: 50,
        tabuSize: 12,
        random: random
      );

      optimizer.Debug = _debug;
      //

      // plot starting weights
      visualizer.PlotGraph(optimizer.StartWeights, world, successRatedIncidents.First().Value, _debug);
      //

      // Optimizing
      Console.WriteLine($"Amb count: {world.AllAmbulancesCount}");

      Stopwatch sw = Stopwatch.StartNew();
      Weights optimalWeights = optimizer.FindOptimal(successRatedIncidents).First();
      sw.Stop();
      //

      // writing result
      visualizer.PlotGraph(optimalWeights, world, successRatedIncidents.First().Value, _debug);
      _debug.WriteLine($"Optimizing took: {sw.Elapsed}");
      //

      optimizer.Dispose();
    }

    _debug.Dispose();
  }
#endif
}
