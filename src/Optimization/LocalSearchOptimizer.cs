using ESSP.DataModel;

namespace Optimizing;

public abstract class LocalSearchOptimizer : MoveOptimizer
{
  public int NeighboursLimit { get; set; }
  public bool ShouldPermutate { get; set; }

  public LocalSearchOptimizer(World world, Constraints constraints, ShiftTimes shiftTimes, IObjectiveFunction loss, bool shouldPermutate = true, int neighboursLimit = int.MaxValue, Random? random = null)
  : base(world, constraints, shiftTimes, loss, random)
  {
    NeighboursLimit = neighboursLimit;
    ShouldPermutate = shouldPermutate;
  }

  /// <summary>
  /// Generates neighbouring moves in <see cref="NeighboursLimit"/> limit in randomly permutated order.
  /// Returns length of generated moves in <see cref="movesBuffer"/>.
  /// </summary>
  public void GetMovesToNeighbours(Weights weights)
  {
    Span<int> permutatedDepots = stackalloc int[World.Depots.Length];
    for (int i = 0; i < permutatedDepots.Length; ++i)
    {
      permutatedDepots[i] = i;
    }

    Span<int> permutatedShiftsOnDepot = stackalloc int[Constraints.MaxMedicTeamsOnDepotCount];
    for (int i = 0; i < permutatedShiftsOnDepot.Length; ++i)
    {
      permutatedShiftsOnDepot[i] = i;
    }

    if (ShouldPermutate)
    {
      Permutate(toPermutate: permutatedDepots);
      Permutate(toPermutate: permutatedShiftsOnDepot);
    }

    movesBuffer.Clear();
    int shiftChangesNeighboursCount = 0;
    int allocationNeighboursCount = 0;
    Move? move;

    for (int depotIndex = 0; shiftChangesNeighboursCount < NeighboursLimit && depotIndex < World.Depots.Length; ++depotIndex)
    {
      for (int shiftOnDepotIndex = 0; shiftOnDepotIndex < Constraints.MaxMedicTeamsOnDepotCount && shiftChangesNeighboursCount < NeighboursLimit; ++shiftOnDepotIndex)
      {
        if (TryGenerateMove(weights, permutatedDepots[depotIndex], permutatedShiftsOnDepot[shiftOnDepotIndex], MoveType.ShiftShorter, out move))
        {
          movesBuffer.Add(move.Value);
          shiftChangesNeighboursCount++;
        }

        if (TryGenerateMove(weights, permutatedDepots[depotIndex], permutatedShiftsOnDepot[shiftOnDepotIndex], MoveType.ShiftLonger, out move))
        {
          movesBuffer.Add(move.Value);
          shiftChangesNeighboursCount++;
        }

        if (TryGenerateMove(weights, permutatedDepots[depotIndex], permutatedShiftsOnDepot[shiftOnDepotIndex], MoveType.ShiftEarlier, out move))
        {
          movesBuffer.Add(move.Value);
          shiftChangesNeighboursCount++;
        }

        if (TryGenerateMove(weights, permutatedDepots[depotIndex], permutatedShiftsOnDepot[shiftOnDepotIndex], MoveType.ShiftLater, out move))
        {
          movesBuffer.Add(move.Value);
          shiftChangesNeighboursCount++;
        }

        if (TryGenerateMove(weights, permutatedDepots[depotIndex], permutatedShiftsOnDepot[shiftOnDepotIndex], MoveType.AllocateMedicTeam, out move))
        {
          movesBuffer.Add(move.Value);
          shiftChangesNeighboursCount++;
        }

        if (TryGenerateMove(weights, permutatedDepots[depotIndex], permutatedShiftsOnDepot[shiftOnDepotIndex], MoveType.DeallocateMedicTeam, out move))
        {
          movesBuffer.Add(move.Value);
          shiftChangesNeighboursCount++;
        }
      }

      if (TryGenerateMove(weights, permutatedDepots[depotIndex], -1, MoveType.AllocateAmbulance, out move))
      {
        movesBuffer.Add(move.Value);
        allocationNeighboursCount++;
      }

      if (TryGenerateMove(weights, permutatedDepots[depotIndex], -1, MoveType.DeallocateAmbulance, out move))
      {
        movesBuffer.Add(move.Value);
        allocationNeighboursCount++;
      }
    }

    //Debug.WriteLine("moves: " + string.Join(", ", movesBuffer));
    //Console.WriteLine($"Neighbours: {movesBuffer.Count}");
  }

  /// <summary>
  /// Fisher-Yates permutation algorithm with limit when to end the permutation.
  /// </summary>
  private void Permutate(Span<int> toPermutate)
  {
    for (int i = 0; i < toPermutate.Length; ++i)
    {
      int nextSwap = _random.Next(i, toPermutate.Length);

      int temp = toPermutate[i];
      toPermutate[i] = toPermutate[nextSwap];
      toPermutate[nextSwap] = temp;
    }
  }
}
