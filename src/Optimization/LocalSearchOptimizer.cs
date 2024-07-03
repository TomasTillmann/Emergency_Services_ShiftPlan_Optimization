
using ESSP.DataModel;
using Optimizng;

namespace Optimizing;

public class LocalSearchOptimizer : NeighbourOptimizer
{
  public EmergencyServicePlan StartPlan { get; set; }

  public LocalSearchOptimizer(World world, Constraints constraints, IUtilityFunction utilityFunction, IMoveGenerator moveGenerator)
  : base(world, constraints, utilityFunction, moveGenerator)
  {
    StartPlan = EmergencyServicePlan.GetNewEmpty(world);
  }

  public override List<EmergencyServicePlan> GetBest(ReadOnlySpan<Incident> incidents)
  {
    EmergencyServicePlan currentPlan = EmergencyServicePlan.GetNewEmpty(World);
    currentPlan.FillFrom(StartPlan);

    MoveSequence bestMove = MoveSequence.GetNewEmpty(1);
    while (true)
    {
      bestMove.Count = 0;
      double bestEval = UtilityFunction.Get(currentPlan, incidents);

      foreach (MoveSequenceDuo moves in MoveGenerator.GetMoves(currentPlan))
      {
        ModifyMakeMove(currentPlan, moves.Normal);

        double neighbourEval = UtilityFunction.Get(currentPlan, incidents);
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
        return new List<EmergencyServicePlan>()
        {
          currentPlan
        };
      }

      ModifyMakeMove(currentPlan, bestMove);
    }
  }
}
