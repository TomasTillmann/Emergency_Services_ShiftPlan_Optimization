using System.Diagnostics.CodeAnalysis;
using ESSP.DataModel;

namespace Optimizing;

public abstract class LocalSearchOptimizer : Optimizer
{
  public int ShiftChangesLimit { get; set; }
  public int AllocationsLimit { get; set; }

  protected readonly Move[] movesBuffer;

  public LocalSearchOptimizer(World world, Constraints constraints, ShiftTimes shiftPlans, ILoss loss, int shiftChangesLimit = int.MaxValue, int allocationsLimit = int.MaxValue, Random? random = null)
  : base(world, constraints, shiftPlans, loss, random)
  {
    ShiftChangesLimit = shiftChangesLimit;
    AllocationsLimit = allocationsLimit;

    // All possible combinations of shift durations + allocation of ambulances to depots, meaning increase count or decrease count move, and
    // the same for allocation of medic teams. Hence 2 * 2 * depots count. 
    int maxMovesCount = shiftPlans.AllowedShiftDurationsSec.Count * shiftPlans.AllowedShiftStartingTimesSec.Count * World.AvailableMedicTeams.Length + 4 * World.Depots.Length;
    movesBuffer = new Move[maxMovesCount];
  }

  /// <summary>
  /// Modifes <paramref name="weights"/> by <paramref name="move"/>.
  /// </summary>
  public void ModifyMakeMove(Weights weights, Move move)
  {
    Interval weight = weights.MedicTeamShifts[move.WeightIndex];

    int durationSec;
    int startingTimeSec;

    switch (move.MoveType)
    {
      case MoveType.ShiftShorter:
        durationSec = GetShorter(weight.DurationSec);
        weights.MedicTeamShifts[move.WeightIndex] = Interval.GetByStartAndDuration(weight.StartSec, durationSec);
        break;

      case MoveType.ShiftLonger:
        durationSec = GetLonger(weight.DurationSec);
        weights.MedicTeamShifts[move.WeightIndex] = Interval.GetByStartAndDuration(weight.StartSec, durationSec);
        break;

      case MoveType.ShiftLater:
        startingTimeSec = GetLater(weight.StartSec);
        weights.MedicTeamShifts[move.WeightIndex] = Interval.GetByStartAndDuration(startingTimeSec, weight.DurationSec);
        break;

      case MoveType.ShiftEarlier:
        startingTimeSec = GetEarlier(weight.StartSec);
        weights.MedicTeamShifts[move.WeightIndex] = Interval.GetByStartAndDuration(startingTimeSec, weight.DurationSec);
        break;

      case MoveType.AllocateMedicTeam:
        weights.MedicTeamAllocations[move.WeightIndex]++;
        weights.AllocatedTeamsCount++;
        break;

      case MoveType.DeallocateMedicTeam:
        weights.MedicTeamAllocations[move.WeightIndex]--;
        weights.AllocatedTeamsCount--;
        break;

      case MoveType.AllocateAmbulance:
        weights.AmbulancesAllocations[move.WeightIndex]++;
        weights.AllocatedAmbulancesCount++;
        break;

      case MoveType.DeallocateAmbulance:
        weights.AmbulancesAllocations[move.WeightIndex]--;
        weights.AllocatedAmbulancesCount--;
        break;

      case MoveType.NoMove:
        break;

      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  /// <summary>
  /// Modifes <paramref name="weights"/> by opposite <see cref="Move"/> of <paramref name="move"/>.
  /// </summary>
  public void ModifyUnmakeMove(Weights weights, Move move)
  {
    MoveType inverseMoveType = GetInverseMoveType(move.MoveType);

    ModifyMakeMove(
      weights,
      new Move
      {
        WeightIndex = move.WeightIndex,
        MoveType = inverseMoveType
      }
    );
  }

  public static MoveType GetInverseMoveType(MoveType moveType)
  {
    switch (moveType)
    {
      case MoveType.ShiftShorter:
        return MoveType.ShiftLonger;

      case MoveType.ShiftLonger:
        return MoveType.ShiftShorter;

      case MoveType.ShiftEarlier:
        return MoveType.ShiftLater;

      case MoveType.ShiftLater:
        return MoveType.ShiftEarlier;

      case MoveType.AllocateMedicTeam:
        return MoveType.DeallocateMedicTeam;

      case MoveType.DeallocateMedicTeam:
        return MoveType.AllocateMedicTeam;

      case MoveType.AllocateAmbulance:
        return MoveType.DeallocateAmbulance;

      case MoveType.DeallocateAmbulance:
        return MoveType.AllocateAmbulance;

      case MoveType.NoMove:
        return MoveType.NoMove;

      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  /// <summary>
  /// Generates neighbouring moves in <see cref="ShiftChangesLimit"/> limit in randomly permutated order.
  /// Returns length of generated moves in <see cref="movesBuffer"/>.
  /// </summary>
  public int GetMovesToNeighbours(Weights weights)
  {
    Span<int> permutatedShifts = stackalloc int[weights.MedicTeamShifts.Length];
    for (int i = 0; i < permutatedShifts.Length; ++i)
    {
      permutatedShifts[i] = i;
    }
    Permutate(toPermutate: ref permutatedShifts, ShiftChangesLimit);
    //Console.WriteLine(string.Join(",", permutatedShifts.ToArray()));

    int bufferIndex = 0;

    int shiftChangesNeighboursCount = 0;
    for (int weightIndex = 0; shiftChangesNeighboursCount < ShiftChangesLimit && weightIndex < permutatedShifts.Length; ++weightIndex)
    {
      Move? move;
      if (TryGenerateMove(weights, permutatedShifts[weightIndex], MoveType.ShiftShorter, out move))
      {
        movesBuffer[bufferIndex++] = move.Value;
        shiftChangesNeighboursCount++;
      }

      if (TryGenerateMove(weights, permutatedShifts[weightIndex], MoveType.ShiftLonger, out move))
      {
        movesBuffer[bufferIndex++] = move.Value;
        shiftChangesNeighboursCount++;
      }

      if (TryGenerateMove(weights, permutatedShifts[weightIndex], MoveType.ShiftLater, out move))
      {
        movesBuffer[bufferIndex++] = move.Value;
        shiftChangesNeighboursCount++;
      }

      if (TryGenerateMove(weights, permutatedShifts[weightIndex], MoveType.ShiftEarlier, out move))
      {
        movesBuffer[bufferIndex++] = move.Value;
        shiftChangesNeighboursCount++;
      }
    }

    Span<int> permutatedDepots = stackalloc int[World.Depots.Length];
    for (int i = 0; i < permutatedDepots.Length; ++i)
    {
      permutatedDepots[i] = i;
    }
    Permutate(toPermutate: ref permutatedDepots, AllocationsLimit);
    //Console.WriteLine(string.Join(",", permutatedDepots.ToArray()));

    int allocationNeighboursCount = 0;
    for (int weightIndex = 0; allocationNeighboursCount < AllocationsLimit && weightIndex < World.Depots.Length; ++weightIndex)
    {
      Move? move;
      if (TryGenerateMove(weights, permutatedDepots[weightIndex], MoveType.AllocateMedicTeam, out move))
      {
        movesBuffer[bufferIndex++] = move.Value;
        allocationNeighboursCount++;
      }

      if (TryGenerateMove(weights, permutatedDepots[weightIndex], MoveType.DeallocateMedicTeam, out move))
      {
        movesBuffer[bufferIndex++] = move.Value;
        allocationNeighboursCount++;
      }

      if (TryGenerateMove(weights, permutatedDepots[weightIndex], MoveType.AllocateAmbulance, out move))
      {
        movesBuffer[bufferIndex++] = move.Value;
        allocationNeighboursCount++;
      }

      if (TryGenerateMove(weights, permutatedDepots[weightIndex], MoveType.DeallocateAmbulance, out move))
      {
        movesBuffer[bufferIndex++] = move.Value;
        allocationNeighboursCount++;
      }
    }

    return bufferIndex;
  }

  /// <summary>
  /// Tries to generate <see cref="Move"/> of type <paramref name="move"/> on <paramref name="weights"/> on <see cref="Interval"/> on <paramref name="weightIndex"/>.
  /// </summary>
  public bool TryGenerateMove(Weights weights, int weightIndex, MoveType type, [NotNullWhen(true)] out Move? move)
  {
    move = null;
    switch (type)
    {
      case MoveType.ShiftShorter:
        int durationSec = weights.MedicTeamShifts[weightIndex].DurationSec;
        if (durationSec != ShiftTimes.MinDurationSec)
        {
          move = new Move
          {
            WeightIndex = weightIndex,
            MoveType = MoveType.ShiftShorter
          };

          return true;
        }

        return false;

      case MoveType.ShiftLonger:
        if (weights.MedicTeamShifts[weightIndex].DurationSec != ShiftTimes.MaxDurationSec)
        {
          move = new Move
          {
            WeightIndex = weightIndex,
            MoveType = MoveType.ShiftLonger
          };
          return true;
        }

        return false;

      case MoveType.ShiftEarlier:
        if (weights.MedicTeamShifts[weightIndex].StartSec != ShiftTimes.EarliestStartingTimeSec)
        {
          move = new Move
          {
            WeightIndex = weightIndex,
            MoveType = MoveType.ShiftEarlier
          };
          return true;
        }

        return false;

      case MoveType.ShiftLater:
        if (weights.MedicTeamShifts[weightIndex].StartSec != ShiftTimes.LatestStartingTimeSec)
        {
          move = new Move
          {
            WeightIndex = weightIndex,
            MoveType = MoveType.ShiftLater
          };
          return true;
        }

        return false;

      case MoveType.AllocateMedicTeam:
        if (weights.MedicTeamAllocations[weightIndex] < Constraints.MaxMedicTeamsOnDepotCount && weights.AllocatedTeamsCount < Constraints.AvailableMedicTeamsCount)
        {
          move = new Move
          {
            WeightIndex = weightIndex,
            MoveType = MoveType.AllocateMedicTeam
          };
          return true;
        }

        return false;

      case MoveType.DeallocateMedicTeam:
        if (weights.MedicTeamAllocations[weightIndex] > 0)
        {
          move = new Move
          {
            WeightIndex = weightIndex,
            MoveType = MoveType.DeallocateMedicTeam
          };
          return true;
        }

        return false;

      case MoveType.AllocateAmbulance:
        if (weights.AmbulancesAllocations[weightIndex] < Constraints.MaxAmbulancesOnDepotCount && weights.AllocatedAmbulancesCount < Constraints.AvailableAmbulancesCount)
        {
          move = new Move
          {
            WeightIndex = weightIndex,
            MoveType = MoveType.AllocateAmbulance
          };
          return true;
        }

        return false;

      case MoveType.DeallocateAmbulance:
        if (weights.AmbulancesAllocations[weightIndex] > 0)
        {
          move = new Move
          {
            WeightIndex = weightIndex,
            MoveType = MoveType.DeallocateAmbulance
          };
          return true;
        }

        return false;

      case MoveType.NoMove:
        move = new Move
        {
          WeightIndex = weightIndex,
          MoveType = MoveType.NoMove
        };

        return true;

      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  private int GetShorter(int durationSec)
  {
    int index = Array.BinarySearch(ShiftTimes.AllowedDurationsSecSorted, durationSec);
    return ShiftTimes.AllowedDurationsSecSorted[index - 1];
  }

  private int GetLonger(int durationSec)
  {
    int index = Array.BinarySearch(ShiftTimes.AllowedDurationsSecSorted, durationSec);
    return ShiftTimes.AllowedDurationsSecSorted[index + 1];
  }

  private int GetEarlier(int startTimeSec)
  {
    int index = Array.BinarySearch(ShiftTimes.AllowedStartingTimesSecSorted, startTimeSec);
    return ShiftTimes.AllowedStartingTimesSecSorted[index - 1];
  }

  private int GetLater(int startTimeSec)
  {
    int index = Array.BinarySearch(ShiftTimes.AllowedStartingTimesSecSorted, startTimeSec);
    return ShiftTimes.AllowedStartingTimesSecSorted[index + 1];
  }

  /// <summary>
  /// Fisher-Yates permutation algorithm with limit when to end the permutation
  /// </summary>
  private void Permutate(ref Span<int> toPermutate, int limit)
  {
    limit = Math.Min(toPermutate.Length, limit);

    for (int i = 0; i < limit; ++i)
    {
      int nextSwap = _random.Next(i, toPermutate.Length);

      int temp = toPermutate[i];
      toPermutate[i] = toPermutate[nextSwap];
      toPermutate[nextSwap] = temp;
    }
  }
}
