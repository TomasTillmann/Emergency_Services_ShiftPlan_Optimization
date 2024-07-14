using System.Collections.Immutable;
using System.Diagnostics;
using DataModel.Interfaces;
using ESSP.DataModel;
using MyExtensions;
using Simulating;

namespace Optimizing;

/// <summary>
/// Implementation of simulated annealing optimizer.
/// </summary>
public class SimulatedAnnealingOptimizer : NeighbourOptimizer
{
  /// <summary>
  /// Plan from which to start the local search.
  /// By default is set to empty plan.
  /// </summary>
  public EmergencyServicePlan StartPlan { get; set; }
  
  /// <summary>
  /// Start temperature.
  /// </summary>
  public double StartTemp { get; set; }
  
  /// <summary>
  /// Final temperature.
  /// </summary>
  public double FinalTemp { get; set; }
  
  /// <summary>
  /// How many iterations to make in a temperature.
  /// </summary>
  public int M_k { get; set; }
  
  /// <summary>
  /// Concrete cooling schedule, which decreases current temperature.
  /// </summary>
  public ICoolingSchedule CoolingSchedule { get; set; }
  
  public Random Random { get; set; }
  
  /// <summary>
  /// Current iteration.
  /// </summary>
  public int Iteration { get; private set; }

  private readonly MoveMaker _moveMaker = new();
  private readonly IMoveGenerator _moveGenerator;

  public SimulatedAnnealingOptimizer(World world, Constraints constraints, IUtilityFunction utilityFunction, IMoveGenerator moveGenerator,
      double startTemp, double finalTemp, int M_k, ICoolingSchedule coolingSchedule, Random? random = null)
  : base(world, constraints, utilityFunction, moveGenerator)
  {
    StartPlan = EmergencyServicePlan.GetNewEmpty(world);
    StartTemp = startTemp;
    FinalTemp = finalTemp;
    this.M_k = M_k;
    CoolingSchedule = coolingSchedule;
    _moveGenerator = moveGenerator;
    Random = random ?? new Random();
  }

  public override List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents)
  {
    EmergencyServicePlan current = EmergencyServicePlan.GetNewFrom(World, StartPlan);
    double currentEval = UtilityFunction.Evaluate(current, incidents.AsSpan());

    EmergencyServicePlan best = EmergencyServicePlan.GetNewFrom(World, current);
    double bestEval = currentEval;

    double temp = StartTemp;
    while (temp > FinalTemp)
    {
      ++Iteration;
      int neighbor = 0;
      for (int m = 0; m < M_k; ++m)
      {
        var moves = _moveGenerator.GetMoves(current).Enumerate(2).ToList();
        int index = Random.Next(moves.Count);
        var move = moves[index];
        
        _moveMaker.ModifyMakeMove(current, move.Normal);

        double neighbourEval = UtilityFunction.Evaluate(current, incidents.AsSpan());
        double delta = currentEval - neighbourEval;
        
        if (delta > 0)
        {
          double probabilityToNotAccept = 1 - Math.Exp(-delta / temp);
          if (Random.NextDouble() < probabilityToNotAccept)
          {
            _moveMaker.ModifyMakeInverseMove(current, move.Inverse);
            continue;
          }
        }

        currentEval = neighbourEval;
        if (currentEval > bestEval)
        {
          best.FillFrom(current);
          bestEval = currentEval;
        }
      }

      temp = CoolingSchedule.Calculate(temp);
    }

    return [best];
  }
}
