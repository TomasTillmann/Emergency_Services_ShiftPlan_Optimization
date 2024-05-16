namespace Optimizing;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ESSP.DataModel;

public class DynamicProgrammingOptimizer : LocalSearchOptimizer
{
  public DynamicProgrammingOptimizer(World world, Constraints constraints, ShiftTimes shiftTimes, IObjectiveFunction loss, bool shouldPermutate = true, int neighboursLimit = int.MaxValue, Random? random = null)
  : base(world, constraints, shiftTimes, loss, shouldPermutate, neighboursLimit, random) { }

  public override IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents)
  {
    ReadOnlySpan<Incident> allIncidents = incidents.AsSpan();
    Debug.WriteLine(string.Join(", ", World.Depots));

    // need empty weights - empty shift plan
    StartWeights = new Weights(World.Depots.Length, Constraints.MaxMedicTeamsOnDepotCount);
    //Debug.WriteLine(StartWeights);
    Weights optimal = StartWeights;
    List<Move> bestMoves = new();

    for (int i = 1; i < incidents.Length; ++i)
    {
      ReadOnlySpan<Incident> currentIncidents = allIncidents.Slice(0, i);
      Debug.WriteLine($"currIncident: {currentIncidents[i - 1]}");
      double bestLoss = ObjectiveFunction.Get(optimal, currentIncidents);
      Debug.WriteLine($"baseLoss: {bestLoss}");
      double currentLoss;

      Move? bestMove = null;
      Move? allocateAmbulance = null;

      GetMovesToNeighbours(optimal);
      for (int j = 0; j < movesBuffer.Count; ++j)
      {
        Move currentMove = movesBuffer[j];
        //Debug.WriteLine($"currentMove: {currentMove}");

        if (currentMove.MoveType == MoveType.AllocateAmbulance)
        {
          continue;
        }

        ModifyMakeMove(optimal, currentMove);
        currentLoss = ObjectiveFunction.Get(optimal, currentIncidents);
        //Debug.WriteLine($"currentLoss: {currentLoss}");

        if (currentLoss < bestLoss)
        {
          bestLoss = currentLoss;
          bestMove = currentMove;
          allocateAmbulance = null;
        }

        if (currentMove.MoveType == MoveType.AllocateMedicTeam)
        {
          if (TryGenerateMove(optimal, currentMove.DepotIndex, -1, MoveType.AllocateAmbulance, out Move? ambulanceMove))
          {
            ModifyMakeMove(optimal, ambulanceMove.Value);
            currentLoss = ObjectiveFunction.Get(optimal, currentIncidents);

            if (currentLoss < bestLoss)
            {
              bestLoss = currentLoss;
              bestMove = currentMove;
              allocateAmbulance = ambulanceMove;
            }

            ModifyUnmakeMove(optimal, ambulanceMove.Value);
          }
        }

        ModifyUnmakeMove(optimal, currentMove);
      }

      if (bestMove != null)
      {
        Debug.WriteLine($"bestMove: {bestMove}");
        ModifyMakeMove(optimal, bestMove.Value);
        if (allocateAmbulance != null)
        {
          Debug.WriteLine($"allocate amb: {allocateAmbulance.Value}");
          ModifyMakeMove(optimal, allocateAmbulance.Value);
        }
      }

      Debug.WriteLine($"plan so far: \n {optimal}");

      Debug.WriteLine("===");
    }

    return new List<Weights>() { optimal };
  }

  /// <summary>
  /// Returns latest shift time with shortest duration, which contains <paramref name="seconds"/>.
  /// NOTE, that such shift time might not exist, but thats a fundemental flaw in shift time desing, and since performance is essential, there is not if check to take care of this
  /// scenarion, so beware it does not happen!
  /// </summary>
  private Interval GetLatestShortestShiftTimeContaining(int seconds)
  {
    Interval latestShortest = Interval.GetByStartAndDuration(0, int.MaxValue);

    for (int i = 0; i < ShiftTimes.AllowedStartingTimesSecSorted.Length; ++i)
    {
      for (int j = 0; j < ShiftTimes.AllowedDurationsSecSorted.Length; ++j)
      {
        Interval interval = Interval.GetByStartAndDuration(ShiftTimes.AllowedStartingTimesSecSorted[i], ShiftTimes.AllowedDurationsSecSorted[j]);
        if (interval.IsInInterval(seconds))
        {
          // lexicographical comparison
          if (interval.StartSec > latestShortest.StartSec || (interval.StartSec == latestShortest.StartSec && interval.DurationSec < latestShortest.DurationSec))
          {
            latestShortest = interval;
          }
        }
      }
    }

    return latestShortest;
  }
}
