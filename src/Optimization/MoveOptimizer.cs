using System.Diagnostics.CodeAnalysis;
using ESSP.DataModel;

namespace Optimizing;

public abstract class MoveOptimizer : Optimizer
{
  protected readonly List<Move> movesBuffer = new();

  protected MoveOptimizer(World world, Constraints constraints, ShiftTimes shiftTimes, ILoss loss, Random? random = null)
  : base(world, constraints, shiftTimes, loss, random)
  {
  }

  /// <summary>
  /// Modifes <paramref name="weights"/> by <paramref name="move"/>.
  /// </summary>
  public void ModifyMakeMove(Weights weights, Move move)
  {
    Interval medicTeamShift;
    int durationSec;
    int startingTimeSec;
    //Console.WriteLine($"ModifyMakeMove: {move}");

    switch (move.MoveType)
    {
      case MoveType.ShiftShorter:
        medicTeamShift = weights.MedicTeamAllocations[move.DepotIndex, move.MedicTeamOnDepotIndex];
        durationSec = medicTeamShift.DurationSec;
        startingTimeSec = medicTeamShift.StartSec;
        durationSec = GetShorter(medicTeamShift.DurationSec);
        weights.MedicTeamAllocations[move.DepotIndex, move.MedicTeamOnDepotIndex] = Interval.GetByStartAndDuration(startingTimeSec, durationSec);
        break;

      case MoveType.ShiftLonger:
        medicTeamShift = weights.MedicTeamAllocations[move.DepotIndex, move.MedicTeamOnDepotIndex];
        durationSec = medicTeamShift.DurationSec;
        startingTimeSec = medicTeamShift.StartSec;
        durationSec = GetLonger(medicTeamShift.DurationSec);
        weights.MedicTeamAllocations[move.DepotIndex, move.MedicTeamOnDepotIndex] = Interval.GetByStartAndDuration(startingTimeSec, durationSec);
        break;

      case MoveType.ShiftLater:
        medicTeamShift = weights.MedicTeamAllocations[move.DepotIndex, move.MedicTeamOnDepotIndex];
        durationSec = medicTeamShift.DurationSec;
        startingTimeSec = medicTeamShift.StartSec;
        startingTimeSec = GetLater(medicTeamShift.StartSec);
        weights.MedicTeamAllocations[move.DepotIndex, move.MedicTeamOnDepotIndex] = Interval.GetByStartAndDuration(startingTimeSec, durationSec);
        break;

      case MoveType.ShiftEarlier:
        medicTeamShift = weights.MedicTeamAllocations[move.DepotIndex, move.MedicTeamOnDepotIndex];
        durationSec = medicTeamShift.DurationSec;
        startingTimeSec = medicTeamShift.StartSec;
        startingTimeSec = GetEarlier(medicTeamShift.StartSec);
        weights.MedicTeamAllocations[move.DepotIndex, move.MedicTeamOnDepotIndex] = Interval.GetByStartAndDuration(startingTimeSec, durationSec);
        break;

      case MoveType.AllocateMedicTeam:
        medicTeamShift = weights.MedicTeamAllocations[move.DepotIndex, move.MedicTeamOnDepotIndex];
        startingTimeSec = medicTeamShift.StartSec;
        weights.MedicTeamAllocations[move.DepotIndex, move.MedicTeamOnDepotIndex] = Interval.GetByStartAndDuration(startingTimeSec, ShiftTimes.MinDurationSec);
        weights.MedicTeamsPerDepotCount[move.DepotIndex]++;
        weights.AllocatedMedicTeamsCount++;
        break;

      case MoveType.DeallocateMedicTeam:
        medicTeamShift = weights.MedicTeamAllocations[move.DepotIndex, move.MedicTeamOnDepotIndex];
        startingTimeSec = medicTeamShift.StartSec;
        weights.MedicTeamAllocations[move.DepotIndex, move.MedicTeamOnDepotIndex] = Interval.GetByStartAndDuration(startingTimeSec, 0);
        weights.MedicTeamsPerDepotCount[move.DepotIndex]--;
        weights.AllocatedMedicTeamsCount--;
        break;

      case MoveType.AllocateAmbulance:
        weights.AmbulancesPerDepotCount[move.DepotIndex]++;
        weights.AllocatedAmbulancesCount++;
        break;

      case MoveType.DeallocateAmbulance:
        weights.AmbulancesPerDepotCount[move.DepotIndex]--;
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
        MedicTeamOnDepotIndex = move.MedicTeamOnDepotIndex,
        DepotIndex = move.DepotIndex,
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
  /// Tries to generate <see cref="Move"/> of type <paramref name="move"/> on <paramref name="weights"/> on <see cref="Interval"/> on <paramref name="weightIndex"/>.
  /// </summary>
  public bool TryGenerateMove(Weights weights, int depotIndex, int medicTeamOnDepotIndex, MoveType type, [NotNullWhen(true)] out Move? move)
  {
    int durationSec;
    int startSec;
    move = default(Move);

    switch (type)
    {
      case MoveType.ShiftShorter:
        durationSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].DurationSec;
        startSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].StartSec;
        if (durationSec != ShiftTimes.MinDurationSec && durationSec != 0)
        {
          move = new Move
          {
            DepotIndex = depotIndex,
            MedicTeamOnDepotIndex = medicTeamOnDepotIndex,
            MoveType = MoveType.ShiftShorter
          };

          return true;
        }

        return false;

      case MoveType.ShiftLonger:
        durationSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].DurationSec;
        startSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].StartSec;
        if (durationSec != ShiftTimes.MaxDurationSec && durationSec != 0)
        {
          move = new Move
          {
            DepotIndex = depotIndex,
            MedicTeamOnDepotIndex = medicTeamOnDepotIndex,
            MoveType = MoveType.ShiftLonger
          };
          return true;
        }

        return false;

      case MoveType.ShiftEarlier:
        durationSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].DurationSec;
        startSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].StartSec;
        if (startSec != ShiftTimes.EarliestStartingTimeSec)
        {
          move = new Move
          {
            DepotIndex = depotIndex,
            MedicTeamOnDepotIndex = medicTeamOnDepotIndex,
            MoveType = MoveType.ShiftEarlier
          };
          return true;
        }

        return false;

      case MoveType.ShiftLater:
        durationSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].DurationSec;
        startSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].StartSec;
        if (startSec != ShiftTimes.LatestStartingTimeSec)
        {
          move = new Move
          {
            DepotIndex = depotIndex,
            MedicTeamOnDepotIndex = medicTeamOnDepotIndex,
            MoveType = MoveType.ShiftLater
          };
          return true;
        }

        return false;

      case MoveType.AllocateMedicTeam:
        durationSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].DurationSec;
        startSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].StartSec;
        if (durationSec == 0 && weights.MedicTeamsPerDepotCount[depotIndex] < Constraints.MaxMedicTeamsOnDepotCount && weights.AllocatedMedicTeamsCount < Constraints.AvailableMedicTeamsCount)
        {
          move = new Move
          {
            DepotIndex = depotIndex,
            MedicTeamOnDepotIndex = medicTeamOnDepotIndex,
            MoveType = MoveType.AllocateMedicTeam
          };
          return true;
        }

        return false;

      case MoveType.DeallocateMedicTeam:
        durationSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].DurationSec;
        startSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].StartSec;
        if (durationSec == ShiftTimes.MinDurationSec && weights.MedicTeamsPerDepotCount[depotIndex] > 0)
        {
          move = new Move
          {
            DepotIndex = depotIndex,
            MedicTeamOnDepotIndex = medicTeamOnDepotIndex,
            MoveType = MoveType.DeallocateMedicTeam
          };
          return true;
        }

        return false;

      case MoveType.AllocateAmbulance:
        if (weights.AmbulancesPerDepotCount[depotIndex] < Constraints.MaxAmbulancesOnDepotCount && weights.AllocatedAmbulancesCount < Constraints.AvailableAmbulancesCount)
        {
          move = new Move
          {
            DepotIndex = depotIndex,
            MedicTeamOnDepotIndex = -1,
            MoveType = MoveType.AllocateAmbulance
          };
          return true;
        }

        return false;

      case MoveType.DeallocateAmbulance:
        if (weights.AmbulancesPerDepotCount[depotIndex] > Constraints.MinAmbulancesOnDepotCount)
        {
          move = new Move
          {
            DepotIndex = depotIndex,
            MedicTeamOnDepotIndex = -1,
            MoveType = MoveType.DeallocateAmbulance
          };
          return true;
        }

        return false;

      case MoveType.NoMove:
        move = new Move
        {
          DepotIndex = -1,
          MedicTeamOnDepotIndex = -1,
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
}

