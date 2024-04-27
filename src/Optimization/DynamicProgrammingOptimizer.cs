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
    ReadOnlySpan<Incident> currentIncidents = incidents.AsSpan();

    // need empty weights
    StartWeights = new Weights(World.Depots.Length, Constraints.MaxMedicTeamsOnDepotCount);
    Weights optimal = StartWeights;

    double lastSuccessRate = 0;
    double currentSuccessRate;

    for (int i = 1; i < incidents.Length; ++i)
    {
      Incident currentIncident = incidents[i];

      double baseLoss = Loss.Get(optimal, currentIncidents.Slice(0, i));

      double bestLoss = double.MaxValue;
      List<Move> bestMoves = new();
      Move? allocateAmbulance = null;

      if (Loss.Simulation.SuccessRate > lastSuccessRate)
      {
        // Current incident was succesfuly handled, no need to change the shift plan.
        continue;
      }

      // Current incident not handled.
      // Meaning, we have to either allocate new medic team (and possible new ambulance for it),
      // or prolong shift of one of the already allocated teams.
      // Do earlier and longer is useless, since the incidents are in ascending order by occurence.
      // Moving teams around will only worsen the loss.

      Move? move;
      double newLoss;
      for (int depotIndex = 0; depotIndex < optimal.MedicTeamAllocations.GetLength(0); ++depotIndex)
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
            }

            ModifyUnmakeMove(optimal, move.Value);
          }
        }

        // Allocate new team 
        if (TryGenerateMove(optimal, depotIndex, -1, MoveType.AllocateMedicTeam, out move))
        {
          ModifyMakeMove(optimal, move.Value);

          newLoss = Loss.Get(optimal, currentIncidents);
          if (newLoss < bestLoss)
          {
            bestMoves.Clear();
            bestMoves.Add(move.Value);
            bestLoss = newLoss;
          }

          // Allocate new ambulance
          if (TryGenerateMove(optimal, depotIndex, -1, MoveType.AllocateAmbulance, out allocateAmbulance))
          {
            ModifyMakeMove(optimal, allocateAmbulance.Value);

            newLoss = Loss.Get(optimal, currentIncidents);
            if (newLoss < bestLoss)
            {
              bestMoves.Add(allocateAmbulance.Value);
              bestLoss = newLoss;
            }

            ModifyUnmakeMove(optimal, allocateAmbulance.Value);
          }

          ModifyUnmakeMove(optimal, move.Value);
        }
      }

      // Outlier incident, simply cannot be handled no matter what.
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
}
