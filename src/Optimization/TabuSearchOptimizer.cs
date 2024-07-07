using ESSP.DataModel;

namespace Optimizing;

public class TabuSearchOptimizer : NeighbourOptimizer
{
  public EmergencyServicePlan StartPlan { get; set; }
  public int TabuTenure { get; set; }
  public int PlateuIteration { get; set; }
  public int MaxIterations { get; set; }

  private readonly MoveMaker _moveMaker = new();

  public TabuSearchOptimizer(World world, Constraints constraints, IUtilityFunction utilityFunction, IMoveGenerator moveGenerator, int tabuTenure, int maxIterations = int.MaxValue)
  : base(world, constraints, utilityFunction, moveGenerator)
  {
    MaxIterations = maxIterations;
    TabuTenure = tabuTenure;
    StartPlan = EmergencyServicePlan.GetNewEmpty(world);
  }

  public override List<EmergencyServicePlan> GetBest(ReadOnlySpan<Incident> incidents)
  {
    using StreamWriter writer = new("/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/log.txt");
    GaantView gaant = new(World, Constraints);

    HashSet<MoveSequence> tabu = new(TabuTenure, new MoveSequenceComparer());
    MoveSequence[] tabuQueue = new MoveSequence[TabuTenure];
    int tabuQueueIndex = -1;

    EmergencyServicePlan current = EmergencyServicePlan.GetNewEmpty(World);
    current.FillFrom(StartPlan);

    EmergencyServicePlan bestPlan = EmergencyServicePlan.GetNewEmpty(World);
    bestPlan.FillFrom(current);
    double bestPlanEval = UtilityFunction.Evaluate(bestPlan, incidents);

    MoveSequenceDuo bestMove = MoveSequenceDuo.GetNewEmpty(MoveGenerator.MovesBufferSize);
    for (PlateuIteration = 0; PlateuIteration < MaxIterations; ++PlateuIteration)
    {
      if (PlateuIteration % 10 == 0) Console.WriteLine(PlateuIteration);

      double bestNeighborEval = double.MinValue;
      foreach (var move in MoveGenerator.GetMoves(current))
      {
        _moveMaker.ModifyMakeMove(current, move.Normal);

        double neighbourEval = UtilityFunction.Evaluate(current, incidents);

        if (neighbourEval > bestNeighborEval && !tabu.Contains(move.Normal))
        {
          //gaant.Show(current, incidents, writer);
          //writer.WriteLine($"eval: {neighbourEval}");
          bestNeighborEval = neighbourEval;
          bestMove.FillFrom(move);
        }
        else if (neighbourEval > bestPlanEval)
        {
          //gaant.Show(current, incidents, writer);
          //writer.WriteLine($"eval: {neighbourEval}");
          bestNeighborEval = neighbourEval;
          bestMove.FillFrom(move);
        }
        
        //if (neighbourEval > bestNeighborEval && (!tabu.Contains(current) || neighbourEval > bestPlanEval))

        _moveMaker.ModifyMakeInverseMove(current, move.Inverse);
      }

      // plateu
      if (bestNeighborEval == double.MinValue)
      {
        return [bestPlan];
      }

      _moveMaker.ModifyMakeMove(current, bestMove.Normal);

      if (bestNeighborEval > bestPlanEval)
      {
        gaant.Show(current, incidents, writer);
        bestPlan.FillFrom(current);
        bestPlanEval = bestNeighborEval;
      }

      int position = (tabuQueueIndex + 1) % TabuTenure;
      //tabu.Remove(tabuQueue[position]);
      MoveSequence bestMoveCopy = MoveSequence.GetNewFrom(bestMove.Inverse);
      tabu.Add(bestMoveCopy);
      tabuQueueIndex = position;
      tabuQueue[tabuQueueIndex] = bestMoveCopy;
    }

    return [bestPlan];
  }
}

