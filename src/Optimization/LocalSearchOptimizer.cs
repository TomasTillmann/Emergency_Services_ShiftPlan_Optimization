using System.Diagnostics.CodeAnalysis;
using ESSP.DataModel;

namespace Optimizing;

public abstract class LocalSearchOptimizer : Optimizer
{
  public int NeighboursLimit { get; set; }
  protected readonly Move[] movesBuffer;

  public LocalSearchOptimizer(World world, Constraints constraints, ILoss loss, int neighboursLimit, Random? random = null)
  : base(world, constraints, loss, random)
  {
    NeighboursLimit = neighboursLimit >= world.AllAmbulancesCount ? world.AllAmbulancesCount : neighboursLimit;

    int maxMovesCount = constraints.AllowedShiftDurationsSec.Count * constraints.AllowedShiftStartingTimesSec.Count * world.AllAmbulancesCount;
    movesBuffer = new Move[maxMovesCount];
  }

  /// <summary>
  /// Modifes <paramref name="weights"/> by <paramref name="move"/>.
  /// </summary>
  public void ModifyMakeMove(Weights weights, Move move)
  {
    Interval weight = weights.Value[move.WeightIndex];

    int durationSec;
    int startingTimeSec;

    switch (move.MoveType)
    {
      case MoveType.Shorter:
        durationSec = GetShorter(weight.DurationSec);
        weights.Value[move.WeightIndex] = Interval.GetByStartAndDuration(weight.StartSec, durationSec);
        break;

      case MoveType.Longer:
        durationSec = GetLonger(weight.DurationSec);
        weights.Value[move.WeightIndex] = Interval.GetByStartAndDuration(weight.StartSec, durationSec);
        break;

      case MoveType.Later:
        startingTimeSec = GetLater(weight.StartSec);
        weights.Value[move.WeightIndex] = Interval.GetByStartAndDuration(startingTimeSec, weight.DurationSec);
        break;

      case MoveType.Earlier:
        startingTimeSec = GetEarlier(weight.StartSec);
        weights.Value[move.WeightIndex] = Interval.GetByStartAndDuration(startingTimeSec, weight.DurationSec);
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
    switch (move.MoveType)
    {
      case MoveType.Shorter:
        ModifyMakeMove(weights, new Move
        {
          WeightIndex = move.WeightIndex,
          MoveType = MoveType.Longer
        });
        break;

      case MoveType.Longer:
        ModifyMakeMove(weights, new Move
        {
          WeightIndex = move.WeightIndex,
          MoveType = MoveType.Shorter
        });
        break;

      case MoveType.Earlier:
        ModifyMakeMove(weights, new Move
        {
          WeightIndex = move.WeightIndex,
          MoveType = MoveType.Later
        });
        break;

      case MoveType.Later:
        ModifyMakeMove(weights, new Move
        {
          WeightIndex = move.WeightIndex,
          MoveType = MoveType.Earlier
        });
        break;

      case MoveType.NoMove:
        break;

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
    Span<int> permutated = stackalloc int[weights.Value.Length];
    for (int i = 0; i < permutated.Length; ++i)
    {
      permutated[i] = i;
    }

    // No need to permutate if all neighbours will be traversed. It is more efficient not to permutate ofc.
    if (NeighboursLimit != weights.Value.Length)
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
        int durationSec = weights.Value[weightIndex].DurationSec;
        if (durationSec != Constraints.MinDurationSec)
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
        if (weights.Value[weightIndex].DurationSec != Constraints.MaxDurationSec)
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
        if (weights.Value[weightIndex].StartSec != Constraints.EarliestStartingTimeSec)
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
        if (weights.Value[weightIndex].StartSec != Constraints.LatestStartingTimeSec)
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
    int index = Array.BinarySearch(Constraints.AllowedDurationsSecSorted, durationSec);
    return Constraints.AllowedDurationsSecSorted[index - 1];
  }

  private int GetLonger(int durationSec)
  {
    int index = Array.BinarySearch(Constraints.AllowedDurationsSecSorted, durationSec);
    return Constraints.AllowedDurationsSecSorted[index + 1];
  }

  private int GetEarlier(int startTimeSec)
  {
    int index = Array.BinarySearch(Constraints.AllowedStartingTimesSecSorted, startTimeSec);
    return Constraints.AllowedStartingTimesSecSorted[index - 1];
  }

  private int GetLater(int startTimeSec)
  {
    int index = Array.BinarySearch(Constraints.AllowedStartingTimesSecSorted, startTimeSec);
    return Constraints.AllowedStartingTimesSecSorted[index + 1];
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
