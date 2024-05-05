namespace Optimizing;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ESSP.DataModel;

public class DynamicProgrammingOptimizer : MoveOptimizer
{
  public DynamicProgrammingOptimizer(World world, Constraints constraints, ShiftTimes shiftTimes, ILoss loss, Random? random = null)
  : base(world, constraints, shiftTimes, loss, random)
  {
  }

  public override IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents)
  {
    ReadOnlySpan<Incident> allIncidents = incidents.AsSpan();

    // need empty weights - empty shift plan
    StartWeights = new Weights(World.Depots.Length, Constraints.MaxMedicTeamsOnDepotCount, Constraints.MaxAmbulancesOnDepotCount);
    Weights optimal = StartWeights;

    double lastSuccessRate = 0;
    double currentSuccessRate;
    List<Move> bestMoves = new();

    for (int i = 1; i < incidents.Length; ++i)
    {
      ReadOnlySpan<Incident> currentIncidents = allIncidents.Slice(0, i);
      Incident currentIncident = incidents[i];
      bestMoves.Clear();

      double baseLoss = Loss.Get(optimal, currentIncidents);
      double bestLoss = baseLoss;
      Move? allocateAmbulance = null;

      Move? move;
      double newLoss;

      for (int depotIndex = 0; depotIndex < World.Depots.Length; ++depotIndex)
      {
        // Make longer
        for (int teamIndex = 0; teamIndex < optimal.MedicTeamsPerDepotCount[depotIndex]; ++teamIndex)
        {
          if (TryGenerateMove(optimal, depotIndex, teamIndex, MoveType.ShiftLonger, out move))
          {
            ModifyMakeMove(optimal, move.Value);

            newLoss = Loss.Get(optimal, currentIncidents);
            if (newLoss < bestLoss)
            {
              bestMoves.Clear();
              bestMoves.Add(move.Value);
              bestLoss = newLoss;
              lastSuccessRate = Loss.Simulation.SuccessRate;
            }

            ModifyUnmakeMove(optimal, move.Value);
          }
        }

        // Allocate new team 
        int newTeamIndex = optimal.MedicTeamsPerDepotCount[depotIndex];
        if (newTeamIndex != Constraints.MaxMedicTeamsOnDepotCount)
        {
          if (TryGenerateMove(optimal, depotIndex, newTeamIndex, MoveType.AllocateMedicTeam, out move))
          {
            ModifyMakeMove(optimal, move.Value);
            optimal.MedicTeamAllocations[depotIndex, newTeamIndex] = GetLatestShortestShiftTimeContaining(currentIncident.OccurenceSec);

            newLoss = Loss.Get(optimal, currentIncidents);
            if (newLoss < bestLoss)
            {
              bestMoves.Clear();
              bestMoves.Add(move.Value);
              bestLoss = newLoss;
              lastSuccessRate = Loss.Simulation.SuccessRate;
            }

            // Allocate new ambulance
            if (TryGenerateMove(optimal, depotIndex, -1, MoveType.AllocateAmbulance, out allocateAmbulance))
            {
              ModifyMakeMove(optimal, allocateAmbulance.Value);

              newLoss = Loss.Get(optimal, currentIncidents);
              if (newLoss < bestLoss)
              {
                bestMoves.Clear();
                bestMoves.Add(move.Value);
                bestMoves.Add(allocateAmbulance.Value);
                bestLoss = newLoss;
                lastSuccessRate = Loss.Simulation.SuccessRate;
              }

              ModifyUnmakeMove(optimal, allocateAmbulance.Value);
            }

            // Don't have to unmake the starting time sec to 0, unnecessary 
            ModifyUnmakeMove(optimal, move.Value);
          }
        }
      }

      // CurrentIncident is outlier incident, simply cannot be handled no matter what.
      if (bestLoss > baseLoss)
      {
        continue;
      }

      for (int m = 0; m < bestMoves.Count; ++m)
      {
        ModifyMakeMove(optimal, bestMoves[m]);
      }
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
