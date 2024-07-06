
using ESSP.DataModel;

namespace Optimizing;

public class LocalSearchOptimizer : NeighbourOptimizer
{
  public EmergencyServicePlan StartPlan { get; set; }
  public int PlateuIteration { get; private set; }
  public int MaxIterations { get; set; }
  private readonly GaantView _gaantView;

  public LocalSearchOptimizer(int maxIterations, World world, Constraints constraints, IUtilityFunction utilityFunction, IMoveGenerator moveGenerator)
  : base(world, constraints, utilityFunction, moveGenerator)
  {
    StartPlan = EmergencyServicePlan.GetNewEmpty(world);
    MaxIterations = maxIterations;
    _gaantView = new GaantView(world, constraints);
  }

  public override List<EmergencyServicePlan> GetBest(ReadOnlySpan<Incident> incidents)
  {
    using StreamWriter writer = new("/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/log.txt");
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
          _gaantView.Show(currentPlan, incidents, writer);
        }

        ModifyMakeInverseMove(currentPlan, moves.Inverse);
      }

      // plateu
      if (bestMove.Count == 0)
      {
        break;
      }

      Console.WriteLine($"best move: {bestMove}");
      ModifyMakeMove(currentPlan, bestMove);
    }
    return new List<EmergencyServicePlan>()
    {
      currentPlan
    };
  }
}
