using ESSP.DataModel;
using Logging;
using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Optimizing;

public sealed class TabuSearchOptimizer : Optimizer
{
    private class ShiftPlanTabu
    {
        public ShiftPlan Value;

        private ShiftPlanTabu(ShiftPlan shiftPlan)
        {
            Value = shiftPlan;
        }

        public static ShiftPlanTabu GetAllShiftHavingSameDuration(IReadOnlyList<Depot> depots, Seconds duration)
        {
            return new ShiftPlanTabu(ShiftPlan.ConstructFrom(depots, 0.ToSeconds(), duration));
        }
        public static ShiftPlanTabu GetRandom(IReadOnlyList<Depot> depots, List<Seconds> allowedStartingTimes, List<Seconds> allowedShiftDurations, Random? random = null)
        {
            ShiftPlan shiftPlanDefault = ShiftPlan.ConstructEmpty(depots);
            foreach(Shift shift in shiftPlanDefault.Shifts)
            {
                shift.Work = Interval.GetByStartAndDuration(allowedStartingTimes.GetRandomElement(random), allowedShiftDurations.GetRandomElement(random));
            }

            return new ShiftPlanTabu(shiftPlanDefault); 
        }

        internal ShiftPlanTabu Clone()
        {
            return new ShiftPlanTabu(Value.Clone());
        }

        public override string ToString()
        {
            return Value.Shifts.Select(shift => (shift.Work.Start.Value / 60/ 60, shift.Work.End.Value / 60/ 60)).Visualize(",");
        }

        public static bool operator ==(ShiftPlanTabu s1, ShiftPlanTabu s2)
        {
            List<Shift>.Enumerator thisEnumerator = s1.Value.Shifts.GetEnumerator();
            List<Shift>.Enumerator anotherEnumerator = s2.Value.Shifts.GetEnumerator();

            while (thisEnumerator.MoveNext() && anotherEnumerator.MoveNext())
            {
                if(thisEnumerator.Current.Work != anotherEnumerator.Current.Work)
                {
                    return false;
                }
            }

            // I know they will be same size - so I don't test that.
            return true;
        }

        public static bool operator !=(ShiftPlanTabu s1, ShiftPlanTabu s2) => !(s1 == s2);

        public override bool Equals(object? obj)
        {
            if(obj is ShiftPlanTabu s)
            {
                return this == s;
            }

            return false;
        }
    }

    private class Move
    {
        public int ShiftIndex { get; }
        public MoveType Type { get; }

        public Move(int shiftIndex, MoveType type)
        {
            ShiftIndex = shiftIndex;
            Type = type;
        }

        public override string ToString()
        {
            return $"({Type}, {ShiftIndex})"; 
        }
    }

    private enum MoveType
    {
        Shorter,
        Longer,
        Earlier,
        Later,
    }

    #region Params
    public readonly int Iterations;
    public readonly int TabuSize;
    public readonly int NeighboursLimit;
    public readonly Random Random;
    #endregion

    private Seconds MaxDuration;
    private Seconds MinDuration;
    private Seconds EarliestStartingTime;
    private Seconds LatestStartingTime;

    private List<Seconds> AllowedDurationsSorted;
    private List<Seconds> AllowedStartingTimesSorted;

    private readonly int maxShiftPlanCost;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="world"></param>
    /// <param name="constraints"></param>
    /// <param name="iterations"></param>
    /// <param name="tabuSize">If set too high, it could happen, that all neighbours are tabu (and aspiration criterion is not satisfied either).
    /// <see cref="Exception"/> will be thrown.</param>
    /// <param name="seed">Seed used for random sample of neighbours list.</param>
    /// <param name="neighboursLimit">If count of neighbours is exceeded, uniformly random sample of this size will be taken as representants of all neighbours.
    /// The more the shifts, the more the neighbours. Running hundreads of simulations in one iteration can be too expensive. This helps this issue.</param>
    public TabuSearchOptimizer(World world, Constraints constraints, int iterations, int tabuSize, int? seed = null, int neighboursLimit = int.MaxValue) : base(world, constraints)
    {
        Iterations = iterations;
        TabuSize = tabuSize;
        NeighboursLimit = neighboursLimit;
        Random = seed is null ? new Random() : new Random(seed.Value);

        AllowedDurationsSorted = Constraints.AllowedShiftDurations.OrderBy(d => d.Value).ToList();

        // empty interval - ambulance not in use at all
        AllowedDurationsSorted.Add(0.ToSeconds());

        MinDuration = AllowedDurationsSorted.First();
        MaxDuration = AllowedDurationsSorted.Last();

        AllowedStartingTimesSorted = Constraints.AllowedShiftStartingTimes.OrderBy(startingTime => startingTime.Value).ToList();
        EarliestStartingTime = AllowedStartingTimesSorted.First();
        LatestStartingTime = AllowedStartingTimesSorted.Last();

        ShiftPlan maximalShiftPlan = ShiftPlanTabu.GetAllShiftHavingSameDuration(world.Depots, MaxDuration).Value;
        this.maxShiftPlanCost = maximalShiftPlan.GetCost();
    }

    public override IEnumerable<ShiftPlan> FindOptimal(List<SuccessRatedIncidents> incidentsSets)
    {
        int Fitness(ShiftPlanTabu shiftPlanTabu)
        {
            return this.Fitness(shiftPlanTabu.Value, incidentsSets);
        }

        ShiftPlanTabu initShiftPlan
            = ShiftPlanTabu.GetRandom(world.Depots, Constraints.AllowedShiftStartingTimes.ToList(), Constraints.AllowedShiftDurations.ToList());

        ShiftPlanTabu globalBest = initShiftPlan;
        int globalBestFitness = Fitness(globalBest);

        ShiftPlanTabu bestCandidate = initShiftPlan;
        int? bestCandidateFitness = globalBestFitness;

        ShiftPlanTabu candidate;
        Move? bestMove;

        LinkedList<Move> tabu = new();
        // TODO: Init tabu?

        Stopwatch sw = new Stopwatch();
        for(int i = 0; i < Iterations; i++)
        {
            sw.Start();
            List<Move> neighbourHoodMoves = GetNeighborhoodMoves(bestCandidate).ToList();
            if(neighbourHoodMoves.Count > NeighboursLimit)
            {
                neighbourHoodMoves = neighbourHoodMoves.GetRandomSamples(NeighboursLimit);
            }

            bestMove = null;
            bestCandidateFitness = null;

            foreach (Move move in neighbourHoodMoves)
            {
                candidate = ModifyMakeMove(bestCandidate, move);

                int candidateFitness = Fitness(candidate);

                if (tabu.Contains(move))
                {
                    // aspiration criterion
                    if(candidateFitness < globalBestFitness)
                    {
                        if(bestCandidateFitness is null || candidateFitness < bestCandidateFitness)
                        {
                            bestMove = move; 
                            bestCandidateFitness = candidateFitness;
                        }
                    }
                }
                else
                {
                    if(bestCandidateFitness is null || candidateFitness < bestCandidateFitness)
                    {
                        bestMove = move; 
                        bestCandidateFitness = candidateFitness;
                    }
                }

                ModifyUnmakeMove(bestCandidate, move);
            }

            if(bestMove is null || bestCandidateFitness is null)
            {
                throw new ArgumentException("All neighbours were tabu and none of them also satisfied aspiration criterion. Perhaps you set tabu size too high?");
            }

            bestCandidate = ModifyMakeMove(bestCandidate.Clone(), bestMove);

            if (bestCandidateFitness < globalBestFitness)
            {
                globalBest = bestCandidate;
                globalBestFitness = bestCandidateFitness.Value;
            }

            tabu.AddLast(bestMove);
            if (tabu.Count > TabuSize)
            {
                tabu.RemoveFirst();
            }

            Logger.Instance.WriteLineForce($"One step took: {sw.ElapsedMilliseconds}ms"); sw.Restart();
            Logger.Instance.WriteLineForce($"Global best: {globalBestFitness} ({globalBest.Value})");
            Logger.Instance.WriteLineForce($"Best candidate: {bestCandidateFitness} ({bestCandidate.Value})");
            Logger.Instance.WriteLineForce();
        }

        return new List<ShiftPlan> { globalBest.Value };
    }

    internal int Fitness(ShiftPlan shiftPlan, List<SuccessRatedIncidents> successRatedIncidents)
    {
        int eval = base.Fitness(shiftPlan, successRatedIncidents, out double meanSuccessRate);

        // damping, to better navigate
        if(eval == int.MaxValue)
        {
            return maxShiftPlanCost + (int)((1 - meanSuccessRate) * 100);
            //return int.MaxValue;
        }

        return eval;
    } 

    private ShiftPlanTabu ModifyMakeMove(ShiftPlanTabu shiftPlanTabu, Move move)
    {
        Shift shift = shiftPlanTabu.Value.Shifts[move.ShiftIndex];

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

        return shiftPlanTabu;
    }

    private ShiftPlanTabu ModifyUnmakeMove(ShiftPlanTabu shiftPlanTabu, Move move)
    {
        switch (move.Type)
        {
            case MoveType.Shorter:
            {
                ModifyMakeMove(shiftPlanTabu, new Move(move.ShiftIndex, MoveType.Longer));
                break;
            }

            case MoveType.Longer:
            {
                ModifyMakeMove(shiftPlanTabu, new Move(move.ShiftIndex, MoveType.Shorter));
                break;
            }

            case MoveType.Earlier:
            {
                ModifyMakeMove(shiftPlanTabu, new Move(move.ShiftIndex, MoveType.Later));
                break;
            }

            case MoveType.Later:
            {
                ModifyMakeMove(shiftPlanTabu, new Move(move.ShiftIndex, MoveType.Earlier));
                break;
            }
        }

        return shiftPlanTabu;
    }

    private IEnumerable<Move> GetNeighborhoodMoves(ShiftPlanTabu shiftPlanTabu)
    {
        for (int shiftIndex = 0; shiftIndex < shiftPlanTabu.Value.Shifts.Count; shiftIndex++)
        {
            Interval shiftWork = shiftPlanTabu.Value.Shifts[shiftIndex].Work;

            Move? move;
            if(TryGenerateMove(shiftWork, shiftIndex, MoveType.Shorter, out move))
            {
                //moves.Add(move);
                yield return move;
            }

            if(TryGenerateMove(shiftWork, shiftIndex, MoveType.Longer, out move))
            {
                //moves.Add(move);
                yield return move;
            }

            if(TryGenerateMove(shiftWork, shiftIndex, MoveType.Later, out move))
            {
                //moves.Add(move);
                yield return move;
            }

            if(TryGenerateMove(shiftWork, shiftIndex, MoveType.Earlier, out move))
            {
                //moves.Add(move);
                yield return move;
            }
        }
        //return moves;
    }

    private bool TryGenerateMove(Interval work, int shiftIndex, MoveType type, [NotNullWhen(true)] out Move? move)
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
        return AllowedDurationsSorted[index-1];
    }

    private Seconds GetLonger(Seconds duration)
    {
        int index = AllowedDurationsSorted.IndexOf(duration);
        return AllowedDurationsSorted[index+1];
    }

    private Seconds GetEarlier(Seconds startTime)
    {
        int index = AllowedStartingTimesSorted.IndexOf(startTime);
        return AllowedStartingTimesSorted[index-1];
    }

    private Seconds GetLater(Seconds startTime)
    {
        int index = AllowedStartingTimesSorted.IndexOf(startTime);
        return AllowedStartingTimesSorted[index+1];
    }
}
