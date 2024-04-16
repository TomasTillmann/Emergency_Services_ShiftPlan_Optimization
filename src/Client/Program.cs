//#define AllOptimizers_Parametrized1
#define simulation_test

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
    SuccessRatedIncidents successRatedIncidents = inputInitial.SuccessRatedIncidents;
    //

    foreach (int randomSeed in Enumerable.Range(0, 100))
    {
      Random random = new Random(randomSeed);

      // input parametrization
      Input input = inputParametrization.Get(new Random());

      World world = input.World;
      ShiftTimes shiftTimes = input.ShiftTimes;
      //

      // Optimizer init
      ILoss loss = new StandardLoss(new Simulation(world), shiftTimes);

      // optimizers init
      List<IOptimizer> optimizers = new()
      {
        new HillClimbOptimizer
        (
          world: world,
          shiftTimes: shiftTimes,
          loss: loss,
          steps: 199,
          random: random
        ),
        new TabuSearchOptimizer
        (
          world: world,
          shiftTimes: shiftTimes,
          loss: loss,
          iterations: 50,
          tabuSize: 12,
          random: random
        ),
        new SimulatedAnnealingOptimizer
        (
          world: world,
          shiftTimes: shiftTimes,
          loss: loss,
          random: random
        )
      };
      //

      foreach (IOptimizer optimizer in optimizers)
      {
        optimizer.Debug = new StreamWriter(_logsPath + optimizer.GetType().Name + randomSeed + ".log");

        // plot starting weights
        visualizer.PlotGraph(optimizer, optimizer.StartWeights, successRatedIncidents.Value, optimizer.Debug);

        optimizer.Debug.WriteLine($"Amb count: {world.AvailableMedicTeams.Length}");

        // Optimizing
        Stopwatch sw = Stopwatch.StartNew();
        Weights optimalWeights = optimizer.FindOptimal(successRatedIncidents).First();
        sw.Stop();

        // writing result
        visualizer.PlotGraph(optimizer, optimalWeights, successRatedIncidents.Value, optimizer.Debug);
        optimizer.Debug.WriteLine($"Optimizing took: {sw.Elapsed}");

        optimizer.Debug.Dispose();
      }
    }
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
    ImmutableArray<Incident> incidentsValue = input.GetIncidents();
    SuccessRatedIncidents incidents = new() { Value = incidentsValue, SuccessRate = 1 };

    Simulation simulation = new(world);

    ILoss loss = new StandardLoss(simulation, shiftTimes);
    IOptimizer optimizer = new HillClimbOptimizer(world, constraints, shiftTimes, loss);
    optimizer.Debug = _debug;

    var optimal = optimizer.FindOptimal(incidents).First();

    visualizer.PlotGraph(optimizer, optimizer.StartWeights, incidents.Value);
    visualizer.PlotGraph(optimizer, optimal, incidents.Value);

    visualizer.Dispose();
  }
#endif
}
