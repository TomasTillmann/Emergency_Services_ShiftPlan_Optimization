using System.Diagnostics.CodeAnalysis;
using ESSP.DataModel;

namespace Optimizing;

public abstract class LocalSearchOptimizer : Optimizer
{
  public int NeighboursLimit { get; set; }
  protected readonly Move[] movesBuffer;

  public LocalSearchOptimizer(World world, Constraints constraints, ShiftTimes shiftPlans, ILoss loss, int neighboursLimit = int.MaxValue, Random? random = null)
  : base(world, constraints, shiftPlans, loss, random)
  {
    NeighboursLimit = neighboursLimit >= world.AvailableMedicTeams.Length ? world.AvailableMedicTeams.Length : neighboursLimit;

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
      case MoveType.Shorter:
        durationSec = GetShorter(weight.DurationSec);
        weights.MedicTeamShifts[move.WeightIndex] = Interval.GetByStartAndDuration(weight.StartSec, durationSec);
        break;

      case MoveType.Longer:
        durationSec = GetLonger(weight.DurationSec);
        weights.MedicTeamShifts[move.WeightIndex] = Interval.GetByStartAndDuration(weight.StartSec, durationSec);
        break;

      case MoveType.Later:
        startingTimeSec = GetLater(weight.StartSec);
        weights.MedicTeamShifts[move.WeightIndex] = Interval.GetByStartAndDuration(startingTimeSec, weight.DurationSec);
        break;

      case MoveType.Earlier:
        startingTimeSec = GetEarlier(weight.StartSec);
        weights.MedicTeamShifts[move.WeightIndex] = Interval.GetByStartAndDuration(startingTimeSec, weight.DurationSec);
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
      case MoveType.Shorter:
        return MoveType.Longer;

      case MoveType.Longer:
        return MoveType.Shorter;

      case MoveType.Earlier:
        return MoveType.Later;

      case MoveType.Later:
        return MoveType.Earlier;

      case MoveType.NoMove:
        return MoveType.NoMove;

      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  /// <summary>
  /// Generates neighbouring moves in <see cref="NeighboursLimit"/> limit in randomly permutated order.
  /// Returns length of generated moves in <see cref="movesBuffer"/>.
  /// </summary>
  public int GetMovesToNeighbours(Weights weights)
  {
    Span<int> permutated = stackalloc int[weights.MedicTeamShifts.Length];
    for (int i = 0; i < permutated.Length; ++i)
    {
      permutated[i] = i;
    }

    // No need to permutate if all neighbours will be traversed. It is more efficient not to permutate ofc.
    if (NeighboursLimit != weights.MedicTeamShifts.Length)
    {
      Permutate(toPermutate: permutated, NeighboursLimit);
      Console.WriteLine(string.Join(",", permutated.ToArray()));
    }

    int bufferIndex = 0;
    for (int weightIndex = 0; weightIndex < NeighboursLimit; ++weightIndex)
    {
      Move? move;
      if (TryGenerateMove(weights, permutated[weightIndex], MoveType.Shorter, out move))
      {
        movesBuffer[bufferIndex++] = move.Value;
      }

      if (TryGenerateMove(weights, permutated[weightIndex], MoveType.Longer, out move))
      {
        movesBuffer[bufferIndex++] = move.Value;
      }

      if (TryGenerateMove(weights, permutated[weightIndex], MoveType.Later, out move))
      {
        movesBuffer[bufferIndex++] = move.Value;
      }

      if (TryGenerateMove(weights, permutated[weightIndex], MoveType.Earlier, out move))
      {
        movesBuffer[bufferIndex++] = move.Value;
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
      case MoveType.Shorter:
        int durationSec = weights.MedicTeamShifts[weightIndex].DurationSec;
        if (durationSec != ShiftTimes.MinDurationSec)
        {
          move = new Move
          {
            WeightIndex = weightIndex,
            MoveType = MoveType.Shorter
          };

          return true;
        }

        return false;

      case MoveType.Longer:
        if (weights.MedicTeamShifts[weightIndex].DurationSec != ShiftTimes.MaxDurationSec)
        {
          move = new Move
          {
            WeightIndex = weightIndex,
            MoveType = MoveType.Longer
          };
          return true;
        }

        return false;

      case MoveType.Earlier:
        if (weights.MedicTeamShifts[weightIndex].StartSec != ShiftTimes.EarliestStartingTimeSec)
        {
          move = new Move
          {
            WeightIndex = weightIndex,
            MoveType = MoveType.Earlier
          };
          return true;
        }

        return false;

      case MoveType.Later:
        if (weights.MedicTeamShifts[weightIndex].StartSec != ShiftTimes.LatestStartingTimeSec)
        {
          move = new Move
          {
            WeightIndex = weightIndex,
            MoveType = MoveType.Later
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
  private void Permutate(Span<int> toPermutate, int limit)
  {
    for (int i = 0; i < limit; ++i)
    {
      int nextSwap = _random.Next(i, toPermutate.Length);

      int temp = toPermutate[i];
      toPermutate[i] = toPermutate[nextSwap];
      toPermutate[nextSwap] = temp;
    }
  }
}
