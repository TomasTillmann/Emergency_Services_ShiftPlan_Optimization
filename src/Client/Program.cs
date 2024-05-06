#define AllOptimizers_Parametrized1
// #define simulation_test
//#define dynamic_programming
//#define serializing

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
  static void Main()
  {
    Visualizer visualizer = new(_debug);

    Random random = new Random(66);
    IInputParametrization input = new Input1(random);
    World world = input.GetWorld();
    Constraints constraints = input.GetConstraints();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    ImmutableArray<Incident> incidents = input.GetIncidents();
    Weights startWeights = Weights.GetUniformlyRandom(world, constraints, shiftTimes, random);

    IOptimizer optimizer;
    Simulation simulation;
    IObjectiveFunction loss;
    List<IOptimizer> optimizers = new();

    IncidentsNormalizer normalizer = new(world, shiftTimes);
    incidents = normalizer.Normalize(incidents);
    Console.WriteLine("incidents count: " + incidents.Length);

    simulation = new(world);
    loss = new StandardLoss(simulation, shiftTimes);
    visualizer.PlotGraph(loss, startWeights, incidents, _debug);

    optimizer = new GeneticAlgorithmOptimizer(world, constraints, shiftTimes, lossCoeff: 0.01f, populationSize: 700, populations: 80, mutationP: 0.01, random: random);
    optimizer.StartWeights = startWeights;
    optimizer.Debug = _debug;
    optimizers.Add(optimizer);

    simulation = new(world);
    loss = new SuccessRateLoss(simulation, shiftTimes);
    optimizer = new HillClimbOptimizer(world, constraints, shiftTimes, loss, iterations: 200, random: random);
    optimizer.StartWeights = startWeights;
    optimizer.Debug = _debug;
    optimizers.Add(optimizer);

    // FindMaxSuccessRateShiftPlan findMax = new(world, shiftTimes, constraints);
    // var o = findMax.FindOptimal(incidents).First();
    // visualizer.PlotGraph(loss, o, incidents);

    simulation = new(world);
    loss = new SuccessRateLoss(simulation, shiftTimes);
    optimizer = new SimulatedAnnealingOptimizer(world, constraints, shiftTimes, loss, random: random);
    optimizer.StartWeights = startWeights;
    optimizer.Debug = _debug;
    optimizers.Add(optimizer);


    simulation = new(world);
    loss = new StandardLoss(simulation, shiftTimes);
    optimizer = new RandomSampleHillClimbOptimizer(world, constraints, shiftTimes, loss, neighboursLimit: 30, samples: 50, iterations: 90, random: random);
    optimizer.Debug = _debug;
    optimizers.Add(optimizer);

    simulation = new(world);
    loss = new StandardLoss(simulation, shiftTimes);
    optimizer = new RandomWalkOptimizer(world, constraints, shiftTimes, loss, iterations: 10_000, random: random);
    optimizer.Debug = _debug;
    optimizers.Add(optimizer);

    simulation = new(world);
    loss = new StandardLoss(simulation, shiftTimes);
    optimizer = new TabuSearchOptimizer(world, constraints, shiftTimes, loss, tabuSize: 300, iterations: 200, random: random);
    optimizer.StartWeights = startWeights;
    optimizer.Debug = _debug;
    optimizers.Add(optimizer);

    simulation = new(world);
    loss = new StandardLoss(simulation, shiftTimes);
    optimizer = new DynamicProgrammingOptimizer(world, constraints, shiftTimes, loss, random: random);
    optimizer.StartWeights = startWeights;
    optimizer.Debug = _debug;
    optimizers.Add(optimizer);

    foreach (IOptimizer opt in optimizers)
    {
      _debug.WriteLine($"{opt.GetType().Name}: ");
      Stopwatch sw = Stopwatch.StartNew();
      var optimals = opt.FindOptimal(incidents).ToList();
      _debug.WriteLine("Elapsed: " + sw.Elapsed);
      optimals.ForEach(optimal => visualizer.PlotGraph(loss, optimal, incidents));
      break;
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
