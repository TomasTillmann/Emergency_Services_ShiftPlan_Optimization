using System.Diagnostics.CodeAnalysis;
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

    HashSet<MoveSequence?> tabu = new(TabuTenure, new MoveSequenceComparer());
    MoveSequence[] tabuQueue = new MoveSequence[TabuTenure];
    int tabuQueueIndex = -1;

    EmergencyServicePlan current = EmergencyServicePlan.GetNewEmpty(World);
    current.FillFrom(StartPlan);

    EmergencyServicePlan bestPlan = EmergencyServicePlan.GetNewEmpty(World);
    bestPlan.FillFrom(current);
    double bestPlanEval = UtilityFunction.Evaluate(bestPlan, incidents);

    MoveSequenceDuo bestMove = MoveSequenceDuo.GetNewEmpty(MoveGenerator.MovesBufferSize);
    double bestNeighborEval;

    for (PlateuIteration = 0; PlateuIteration < MaxIterations; ++PlateuIteration)
    {
      Console.WriteLine(PlateuIteration);

      bestNeighborEval = double.MinValue;
      foreach (var move in MoveGenerator.GetMoves(current))
      {
        _moveMaker.ModifyMakeMove(current, move.Normal);

        double neighbourEval = UtilityFunction.Evaluate(current, incidents);
        if (tabu.Contains(move.Normal))
        {

        }
        if (neighbourEval > bestNeighborEval && (!tabu.Contains(move.Normal) || neighbourEval > bestPlanEval))
        {
          gaant.Show(current, incidents, writer);
          writer.WriteLine($"eval: {neighbourEval}");
          bestNeighborEval = neighbourEval;
          bestMove.FillFrom(move);
        }

        _moveMaker.ModifyMakeInverseMove(current, move.Inverse);
      }

      // plateu
      if (bestNeighborEval == double.MinValue)
      {
        return new List<EmergencyServicePlan> { bestPlan };
      }

      _moveMaker.ModifyMakeMove(current, bestMove.Normal);

      if (bestNeighborEval > bestPlanEval)
      {
        bestPlan.FillFrom(current);
      }

      int position = (tabuQueueIndex + 1) % TabuTenure;
      tabu.Remove(tabuQueue[position]);
      tabu.Add(MoveSequence.GetNewFrom(bestMove.Inverse));
      tabuQueueIndex = position;
      tabuQueue[tabuQueueIndex] = bestMove.Inverse;
    }

    return new List<EmergencyServicePlan> { bestPlan };
  }

  public class MoveSequenceComparer : IEqualityComparer<MoveSequence>
  {
    public bool Equals(MoveSequence x, MoveSequence y)
    {
      if (x.Count != y.Count)
      {
        return false;
      }

      for (int i = 0; i < x.Count; ++i)
      {
        if (x.MovesBuffer[i] != y.MovesBuffer[i])
        {
          return false;
        }
      }

      return true;
    }

    public int GetHashCode([DisallowNull] MoveSequence obj)
    {
      return HashCode.Combine(obj.Count, );
    }
  }
}

