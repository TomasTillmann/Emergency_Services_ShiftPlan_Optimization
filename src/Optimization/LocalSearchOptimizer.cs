using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using ESSP.DataModel;

namespace Optimizing;

public abstract class LocalSearchOptimizer : Optimizer
{
  /// <summary>
  /// Calculated from passed <see cref="Constraints"/>.
  /// </summary>
  private readonly int _minDurationSec;

  /// <summary>
  /// Calculated from passed <see cref="Constraints"/>.
  /// </summary>
  private readonly int _maxDurationSec;

  /// <summary>
  /// Calculated from passed <see cref="Constraints"/>.
  /// </summary>
  private readonly int _earliestStartingTimeSec;

  /// <summary>
  /// Calculated from passed <see cref="Constraints"/>.
  /// </summary>
  private readonly int _latestStartingTimeSec;

  private readonly int[] _allowedDurationsSecSorted;

  private readonly int[] _allowedStartingTimesSecSorted;

  public LocalSearchOptimizer(World world, Constraints constraints, ILoss loss)
  : base(world, constraints, loss)
  {
    _allowedDurationsSecSorted = constraints.AllowedShiftDurationsSec.ToList().OrderBy(duration => duration).ToArray();
    _minDurationSec = _allowedDurationsSecSorted.First();
    _maxDurationSec = _allowedDurationsSecSorted.Last();

    _allowedStartingTimesSecSorted = constraints.AllowedShiftStartingTimesSec.ToList().OrderBy(duration => duration).ToArray();
    _earliestStartingTimeSec = _allowedStartingTimesSecSorted.First();
    _latestStartingTimeSec = _allowedStartingTimesSecSorted.Last();
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
  /// Enumerates all legal <see cref="Move"/>s, except <see cref="MoveType.NoMove"/>, to neighbouring <see cref="Weights"/> of <paramref name="weights"/>.
  /// </summary>
  public IEnumerable<Move> GetMovesToNeighbours(Weights weights)
  {
    for (int weightIndex = 0; weightIndex < weights.Value.Length; ++weightIndex)
    {
      Interval weight = weights.Value[weightIndex];

      Move? move;
      if (TryGenerateMove(weights, weightIndex, MoveType.Shorter, out move))
      {
        yield return move.Value;
      }

      if (TryGenerateMove(weights, weightIndex, MoveType.Longer, out move))
      {
        yield return move.Value;
      }

      if (TryGenerateMove(weights, weightIndex, MoveType.Later, out move))
      {
        yield return move.Value;
      }

      if (TryGenerateMove(weights, weightIndex, MoveType.Earlier, out move))
      {
        yield return move.Value;
      }
    }
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
        if (weights.Value[weightIndex].DurationSec != _minDurationSec)
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
        if (weights.Value[weightIndex].DurationSec != _maxDurationSec)
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
        if (weights.Value[weightIndex].StartSec != _earliestStartingTimeSec)
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
        if (weights.Value[weightIndex].StartSec != _latestStartingTimeSec)
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
    int index = Array.BinarySearch(_allowedDurationsSecSorted, durationSec);
    return _allowedDurationsSecSorted[index - 1];
  }

  private int GetLonger(int durationSec)
  {
    int index = Array.BinarySearch(_allowedDurationsSecSorted, durationSec);
    return _allowedDurationsSecSorted[index + 1];
  }

  private int GetEarlier(int startTimeSec)
  {
    int index = Array.BinarySearch(_allowedStartingTimesSecSorted, startTimeSec);
    return _allowedStartingTimesSecSorted[index - 1];
  }

  private int GetLater(int startTimeSec)
  {
    int index = Array.BinarySearch(_allowedStartingTimesSecSorted, startTimeSec);
    return _allowedStartingTimesSecSorted[index + 1];
  }
}
