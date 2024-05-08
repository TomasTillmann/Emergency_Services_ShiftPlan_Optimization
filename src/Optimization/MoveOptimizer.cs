using System.Diagnostics.CodeAnalysis;
using ESSP.DataModel;

namespace Optimizing;

public abstract class MoveOptimizer : Optimizer
{
  protected readonly List<Move> movesBuffer = new();

  protected MoveType[] MedicTeamMoveTypes { get; }
  protected MoveType[] AmbulanceMoveTypes { get; }
  protected MoveType[] MoveTypes { get; }

  protected MoveOptimizer(World world, Constraints constraints, ShiftTimes shiftTimes, IObjectiveFunction loss, Random? random = null)
  : base(world, constraints, shiftTimes, loss, random)
  {
    MoveTypes = (MoveType[])Enum.GetValues(typeof(MoveType));
    MedicTeamMoveTypes = new MoveType[] { MoveType.ShiftLater, MoveType.ShiftLonger, MoveType.ShiftEarlier, MoveType.ShiftLater, MoveType.AllocateMedicTeam, MoveType.DeallocateMedicTeam };
    AmbulanceMoveTypes = new MoveType[] { MoveType.AllocateAmbulance, MoveType.DeallocateAmbulance };
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
        medicTeamShift = weights.MedicTeamAllocations[move.DepotIndex, move.OnDepotIndex];
        durationSec = medicTeamShift.DurationSec;
        startingTimeSec = medicTeamShift.StartSec;
        durationSec = GetShorter(medicTeamShift.DurationSec);
        weights.MedicTeamAllocations[move.DepotIndex, move.OnDepotIndex] = Interval.GetByStartAndDuration(startingTimeSec, durationSec);
        break;

      case MoveType.ShiftLonger:
        medicTeamShift = weights.MedicTeamAllocations[move.DepotIndex, move.OnDepotIndex];
        durationSec = medicTeamShift.DurationSec;
        startingTimeSec = medicTeamShift.StartSec;
        durationSec = GetLonger(medicTeamShift.DurationSec);
        weights.MedicTeamAllocations[move.DepotIndex, move.OnDepotIndex] = Interval.GetByStartAndDuration(startingTimeSec, durationSec);
        break;

      case MoveType.ShiftLater:
        medicTeamShift = weights.MedicTeamAllocations[move.DepotIndex, move.OnDepotIndex];
        durationSec = medicTeamShift.DurationSec;
        startingTimeSec = medicTeamShift.StartSec;
        startingTimeSec = GetLater(medicTeamShift.StartSec);
        weights.MedicTeamAllocations[move.DepotIndex, move.OnDepotIndex] = Interval.GetByStartAndDuration(startingTimeSec, durationSec);
        break;

      case MoveType.ShiftEarlier:
        medicTeamShift = weights.MedicTeamAllocations[move.DepotIndex, move.OnDepotIndex];
        durationSec = medicTeamShift.DurationSec;
        startingTimeSec = medicTeamShift.StartSec;
        startingTimeSec = GetEarlier(medicTeamShift.StartSec);
        weights.MedicTeamAllocations[move.DepotIndex, move.OnDepotIndex] = Interval.GetByStartAndDuration(startingTimeSec, durationSec);
        break;

      case MoveType.AllocateMedicTeam:
        medicTeamShift = weights.MedicTeamAllocations[move.DepotIndex, move.OnDepotIndex];
        startingTimeSec = medicTeamShift.StartSec;
        weights.MedicTeamAllocations[move.DepotIndex, move.OnDepotIndex] = Interval.GetByStartAndDuration(startingTimeSec, ShiftTimes.MinDurationSec);
        weights.MedicTeamsPerDepotCount[move.DepotIndex]++;
        weights.AllAllocatedMedicTeamsCount++;
        break;

      case MoveType.DeallocateMedicTeam:
        medicTeamShift = weights.MedicTeamAllocations[move.DepotIndex, move.OnDepotIndex];
        startingTimeSec = medicTeamShift.StartSec;
        weights.MedicTeamAllocations[move.DepotIndex, move.OnDepotIndex] = Interval.GetByStartAndDuration(startingTimeSec, 0);
        weights.MedicTeamsPerDepotCount[move.DepotIndex]--;
        weights.AllAllocatedMedicTeamsCount--;
        break;

      case MoveType.AllocateAmbulance:
        weights.AmbulancesPerDepotCount[move.DepotIndex]++;
        weights.AllAllocatedAmbulancesCount++;
        break;

      case MoveType.DeallocateAmbulance:
        weights.AmbulancesPerDepotCount[move.DepotIndex]--;
        weights.AllAllocatedAmbulancesCount--;
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
        OnDepotIndex = move.OnDepotIndex,
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

  public int GetAllMoves(Weights weights, int depotIndex, int medicTeamOnDepotIndex, Move[] movesBuffer)
  {
    int length = GetAllMedicTeamMoves(weights, depotIndex, medicTeamOnDepotIndex, movesBuffer);
    length += GetAllAmbulanceMoves(weights, depotIndex, movesBuffer);
    return length;

  }

  public int GetAllMedicTeamMoves(Weights weights, int depotIndex, int medicTeamOnDepotIndex, Move[] movesBuffer)
  {
    int length = 0;
    for (int moveTypeIndex = 0; moveTypeIndex < MedicTeamMoveTypes.Length; ++moveTypeIndex)
    {
      if (TryGenerateMove(weights, depotIndex, medicTeamOnDepotIndex, MedicTeamMoveTypes[moveTypeIndex], out Move? move))
      {
        movesBuffer[length++] = move.Value;
      }
    }

    return length;
  }

  public int GetAllAmbulanceMoves(Weights weights, int depotIndex, Move[] movesBuffer)
  {
    int length = 0;
    for (int moveTypeIndex = 0; moveTypeIndex < AmbulanceMoveTypes.Length; ++moveTypeIndex)
    {
      if (TryGenerateMove(weights, depotIndex, -1, AmbulanceMoveTypes[moveTypeIndex], out Move? move))
      {
        movesBuffer[length++] = move.Value;
      }
    }

    return length;
  }

  /// <summary>
  /// Tries to generate <see cref="Move"/> of type <paramref name="move"/> on <paramref name="weights"/>.
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
            OnDepotIndex = medicTeamOnDepotIndex,
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
            OnDepotIndex = medicTeamOnDepotIndex,
            MoveType = MoveType.ShiftLonger
          };
          return true;
        }

        return false;

      case MoveType.ShiftEarlier:
        durationSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].DurationSec;
        startSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].StartSec;
        if (startSec != ShiftTimes.EarliestStartingTimeSec && durationSec != 0)
        {
          move = new Move
          {
            DepotIndex = depotIndex,
            OnDepotIndex = medicTeamOnDepotIndex,
            MoveType = MoveType.ShiftEarlier
          };
          return true;
        }

        return false;

      case MoveType.ShiftLater:
        durationSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].DurationSec;
        startSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].StartSec;
        if (startSec != ShiftTimes.LatestStartingTimeSec && durationSec != 0)
        {
          move = new Move
          {
            DepotIndex = depotIndex,
            OnDepotIndex = medicTeamOnDepotIndex,
            MoveType = MoveType.ShiftLater
          };
          return true;
        }

        return false;

      case MoveType.AllocateMedicTeam:
        durationSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].DurationSec;
        if (durationSec == 0 && weights.MedicTeamsPerDepotCount[depotIndex] < Constraints.MaxMedicTeamsOnDepotCount && weights.AllAllocatedMedicTeamsCount < Constraints.AvailableMedicTeamsCount)
        {
          move = new Move
          {
            DepotIndex = depotIndex,
            OnDepotIndex = medicTeamOnDepotIndex,
            MoveType = MoveType.AllocateMedicTeam
          };
          return true;
        }

        return false;

      case MoveType.DeallocateMedicTeam:
        durationSec = weights.MedicTeamAllocations[depotIndex, medicTeamOnDepotIndex].DurationSec;
        if (durationSec == ShiftTimes.MinDurationSec && weights.MedicTeamsPerDepotCount[depotIndex] > 0)
        {
          move = new Move
          {
            DepotIndex = depotIndex,
            OnDepotIndex = medicTeamOnDepotIndex,
            MoveType = MoveType.DeallocateMedicTeam
          };
          return true;
        }

        return false;

      case MoveType.AllocateAmbulance:
        if (weights.AmbulancesPerDepotCount[depotIndex] < Constraints.MaxAmbulancesOnDepotCount && weights.AllAllocatedAmbulancesCount < Constraints.AvailableAmbulancesCount)
        {
          move = new Move
          {
            DepotIndex = depotIndex,
            OnDepotIndex = -1,
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
            OnDepotIndex = -1,
            MoveType = MoveType.DeallocateAmbulance
          };
          return true;
        }

        return false;

      case MoveType.NoMove:
        move = new Move
        {
          DepotIndex = -1,
          OnDepotIndex = -1,
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

