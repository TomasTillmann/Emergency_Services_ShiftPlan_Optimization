using ESSP.DataModel;
using Optimizing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization;

public class ShiftsTravel
{
    protected Seconds MaxDuration;
    protected Seconds MinDuration;
    protected Seconds EarliestStartingTime;
    protected Seconds LatestStartingTime;

    protected List<Seconds> AllowedDurationsSorted;
    protected List<Seconds> AllowedStartingTimesSorted;

    public ShiftsTravel(Domain constraints)
    {
        AllowedDurationsSorted = constraints.AllowedShiftDurations.OrderBy(duration => duration.Value).ToList();
        MinDuration = AllowedDurationsSorted.First();
        MaxDuration = AllowedDurationsSorted.Last();

        AllowedStartingTimesSorted = constraints.AllowedShiftStartingTimes.OrderBy(startingTime => startingTime.Value).ToList();
        EarliestStartingTime = AllowedStartingTimesSorted.First();
        LatestStartingTime = AllowedStartingTimesSorted.Last();
    }

    public TShifts ModifyMakeMove<TShifts>(TShifts movable, Move move) where TShifts : IShifts
    {
        Shift shift = movable[move.ShiftIndex];

        switch (move.Type)
        {
            case MoveType.Shorter:
                {
                    Seconds duration = GetShorter(shift.Work.Duration);
                    shift.Work = Interval.GetByStartAndDuration(shift.Work.Start, duration);

                    break;
                }

            case MoveType.Longer:
                {
                    Seconds duration = GetLonger(shift.Work.Duration);
                    shift.Work = Interval.GetByStartAndDuration(shift.Work.Start, duration);

                    break;
                }
            case MoveType.Later:
                {

                    Seconds startingTime = GetLater(shift.Work.Start);
                    shift.Work = Interval.GetByStartAndDuration(startingTime, shift.Work.Duration);

                    break;
                }
            case MoveType.Earlier:
                {
                    Seconds startingTime = GetEarlier(shift.Work.Start);
                    shift.Work = Interval.GetByStartAndDuration(startingTime, shift.Work.Duration);

                    break;
                }

            default:
                {
                    throw new ArgumentException("Missing case!");
                }
        }

        return movable;
    }

    public TShifts ModifyUnmakeMove<TShifts>(TShifts movable, Move move) where TShifts : IShifts
    {
        switch (move.Type)
        {
            case MoveType.Shorter:
                {
                    ModifyMakeMove(movable, new Move(move.ShiftIndex, MoveType.Longer));
                    break;
                }

            case MoveType.Longer:
                {
                    ModifyMakeMove(movable, new Move(move.ShiftIndex, MoveType.Shorter));
                    break;
                }

            case MoveType.Earlier:
                {
                    ModifyMakeMove(movable, new Move(move.ShiftIndex, MoveType.Later));
                    break;
                }

            case MoveType.Later:
                {
                    ModifyMakeMove(movable, new Move(move.ShiftIndex, MoveType.Earlier));
                    break;
                }
        }

        return movable;
    }

    public IEnumerable<Move> GetNeighborhoodMoves(IShifts movable)
    {
        for (int shiftIndex = 0; shiftIndex < movable.Count; shiftIndex++)
        {
            Interval shiftWork = movable[shiftIndex].Work;

            Move? move;
            if (TryGenerateMove(shiftWork, shiftIndex, MoveType.Shorter, out move))
            {
                yield return move;
            }

            if (TryGenerateMove(shiftWork, shiftIndex, MoveType.Longer, out move))
            {
                yield return move;
            }

            if (TryGenerateMove(shiftWork, shiftIndex, MoveType.Later, out move))
            {
                yield return move;
            }

            if (TryGenerateMove(shiftWork, shiftIndex, MoveType.Earlier, out move))
            {
                yield return move;
            }
        }
    }

    public bool TryGenerateMove(Interval work, int shiftIndex, MoveType type, [NotNullWhen(true)] out Move? move)
    {
        move = null;
        switch (type)
        {
            case MoveType.Shorter:
                {
                    if (work.Duration != MinDuration)
                    {
                        move = new Move(shiftIndex, MoveType.Shorter);
                        return true;
                    }

                    return false;
                }

            case MoveType.Longer:
                {
                    if (work.Duration != MaxDuration)
                    {
                        move = new Move(shiftIndex, MoveType.Longer);
                        return true;
                    }

                    return false;
                }

            case MoveType.Earlier:
                {
                    if (work.Start != EarliestStartingTime)
                    {
                        move = new Move(shiftIndex, MoveType.Earlier);
                        return true;
                    }

                    return false;
                }

            case MoveType.Later:
                {
                    if (work.Start != LatestStartingTime)
                    {
                        move = new Move(shiftIndex, MoveType.Later);
                        return true;
                    }

                    return false;
                }

            default:
                {
                    throw new ArgumentException("Missing case statement!");
                }
        }
    }

    private Seconds GetShorter(Seconds duration)
    {
        int index = AllowedDurationsSorted.IndexOf(duration);
        return AllowedDurationsSorted[index - 1];
    }

    private Seconds GetLonger(Seconds duration)
    {
        int index = AllowedDurationsSorted.IndexOf(duration);
        return AllowedDurationsSorted[index + 1];
    }

    private Seconds GetEarlier(Seconds startTime)
    {
        int index = AllowedStartingTimesSorted.IndexOf(startTime);
        return AllowedStartingTimesSorted[index - 1];
    }

    private Seconds GetLater(Seconds startTime)
    {
        int index = AllowedStartingTimesSorted.IndexOf(startTime);
        return AllowedStartingTimesSorted[index + 1];
    }
}
