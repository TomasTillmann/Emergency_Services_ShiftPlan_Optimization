//#define LocalSearch 
#define DynamicProgramming 

using ESSP.DataModel;
using Optimizing;
using Simulating;
using System.Collections.Immutable;
using System.Diagnostics;
using Optimization;

namespace Client;

class Program
{
#if LocalSearch
  public static void Main()
  {
    Random random = new Random(66);
    IInputParametrization input = new Input1(random);
    World world = input.GetWorld();
    Constraints constraints = input.GetConstraints();
    ShiftTimes shiftTimes = input.GetShiftTimes();

    ImmutableArray<Incident> incidents = input.GetIncidents();

    LocalSearchOptimizer optimizer;
    Simulation simulation;
    IUtilityFunction utilityFunction;

    simulation = new(world, constraints);
    utilityFunction = new WeightedSum(simulation, EmergencyServicePlan.GetMaxCost(world, shiftTimes));
    IMoveGenerator moveGenerator = new AllBasicMovesGenerator(shiftTimes, constraints);
    optimizer = new LocalSearchOptimizer(30, world, constraints, utilityFunction, moveGenerator);

    Stopwatch sw = Stopwatch.StartNew();
    var optimal = optimizer.GetBest(incidents.AsSpan()).ToList().First();
    double eval = utilityFunction.Evaluate(optimal, incidents.AsSpan());
    
    Console.WriteLine($"Iteration: {optimizer.PlateuIteration} " +
                      $"eval: {eval}," +
                      $"handled: {utilityFunction.HandledIncidentsCount} / {incidents.Length} " +
                      $"cost: {optimal.Cost}");

    using StreamWriter writer = new("/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/log.txt");
    GaantView gaant = new GaantView(world, constraints);
    gaant.Show(optimal, incidents.AsSpan(), writer);
  }
#endif

#if DynamicProgramming
  public static void Main()
  {
    Random random = new Random(66);
    IInputParametrization input = new Input1(random);
    World world = input.GetWorld();
    Constraints constraints = input.GetConstraints();
    ShiftTimes shiftTimes = input.GetShiftTimes();
    ImmutableArray<Incident> incidents = input.GetIncidents();
    OptimalMovesSearchOptimizer optimizer;
    Simulation simulation;
    IUtilityFunction utilityFunction;

    simulation = new(world, constraints);
    optimizer = new OptimalMovesSearchOptimizer(world, shiftTimes, constraints);

    Stopwatch sw = Stopwatch.StartNew();
    var optimal = optimizer.GetBest(incidents).First();

    simulation.Run(optimal, incidents.AsSpan());
    Console.WriteLine($"handled: {simulation.HandledIncidentsCount} / {incidents.Length} " +
                      $"cost: {optimal.Cost}");

    // using StreamWriter writer = new("/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/log.txt");
    // GaantView gaant = new GaantView(world, constraints);
    // gaant.Show(optimal, incidents.AsSpan(), writer);
  }
#endif
}
