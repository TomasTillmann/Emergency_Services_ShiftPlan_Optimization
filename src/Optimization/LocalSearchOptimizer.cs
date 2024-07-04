
using ESSP.DataModel;

namespace Optimizing;

public class LocalSearchOptimizer : NeighbourOptimizer
{
  public EmergencyServicePlan StartPlan { get; set; }

  public int PlateuIteration { get; private set; }

  public int MaxIterations { get; set; }

  public LocalSearchOptimizer(int maxIterations, World world, Constraints constraints, IUtilityFunction utilityFunction, IMoveGenerator moveGenerator)
  : base(world, constraints, utilityFunction, moveGenerator)
  {
    StartPlan = EmergencyServicePlan.GetNewEmpty(world);
    MaxIterations = maxIterations;
  }

  public override List<EmergencyServicePlan> GetBest(ReadOnlySpan<Incident> incidents)
  {
    EmergencyServicePlan currentPlan = EmergencyServicePlan.GetNewEmpty(World);
    currentPlan.FillFrom(StartPlan);

    MoveSequence bestMove = MoveSequence.GetNewEmpty(MoveGenerator.MovesBufferSize);
    for (PlateuIteration = 0; PlateuIteration < MaxIterations; ++PlateuIteration)
    {
      if (PlateuIteration % 10 == 0) Console.WriteLine(PlateuIteration);
      bestMove.Count = 0;
      double bestEval = UtilityFunction.Evaluate(currentPlan, incidents);

      foreach (MoveSequenceDuo moves in MoveGenerator.GetMoves(currentPlan))
      {
        ModifyMakeMove(currentPlan, moves.Normal);

        double neighbourEval = UtilityFunction.Evaluate(currentPlan, incidents);
        if (neighbourEval >= bestEval)
        {
          bestMove.FillFrom(moves.Normal);
          bestEval = neighbourEval;
        }

        ModifyMakeInverseMove(currentPlan, moves.Inverse);
      }

      // plateu
      if (bestMove.Count == 0)
      {
        break;
      }

      ModifyMakeMove(currentPlan, bestMove);
    }
    return new List<EmergencyServicePlan>()
    {
      currentPlan
    };
  }
}
