using ESSP.DataModel;
using Logging;
using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Optimizing;

public sealed class TabuSearchOptimizer : LocalSearchOptimizer
{
    private class ShiftPlanTabu : IShifts
    {
        public ShiftPlan Value;

        public int Count => Value.Shifts.Count; 
        public Shift this[int index] { get => Value.Shifts[index]; set => Value.Shifts[index] = value; }

        public ShiftPlanTabu(ShiftPlan shiftPlan)
        {
            Value = shiftPlan;
        }

        public static ShiftPlanTabu GetAllShiftHavingSameDuration(IReadOnlyList<Depot> depots, Seconds duration)
        {
            return new ShiftPlanTabu(ShiftPlan.ConstructFrom(depots, 0.ToSeconds(), duration));
        }

        public void ClearAllPlannedIncidents()
        {
            Value.Shifts.ForEach(shift => shift.ClearPlannedIncidents());
        }

        internal ShiftPlanTabu Copy()
        {
            return new ShiftPlanTabu(Value.Copy());
        }
        public int GetCost()
        {
            return Value.GetCost();
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

    #region Params
    public readonly int Iterations;
    public readonly int TabuSize;
    public readonly int NeighboursLimit;
    public readonly Random Random;
    #endregion

    /// <param name="world"></param>
    /// <param name="constraints"></param>
    /// <param name="iterations"></param>
    /// <param name="tabuSize">If set too high, it could happen, that all neighbours are tabu (and aspiration criterion is not satisfied either).
    /// <see cref="Exception"/> will be thrown.</param>
    /// <param name="seed">Seed used for random sample of neighbours list.</param>
    /// <param name="neighboursLimit">If count of neighbours is exceeded, uniformly random sample of this size will be taken as representants of all neighbours.
    /// The more the shifts, the more the neighbours. Running hundreads of simulations in one iteration can be too expensive. This helps this issue.</param>
    /// <param name="seed">Initial <see cref="ShiftPlan"/> is selected randomly. When limiting neighbours to search by <paramref name="neighboursLimit"/>, neighbours to search are selected at random.</param>
    public TabuSearchOptimizer(World world, Domain constraints, int iterations, int tabuSize, int neighboursLimit = int.MaxValue, int? seed = null) : base(world, constraints)
    {
        Random = seed is null ? new Random() : new Random(seed.Value);
        Iterations = iterations;
        TabuSize = tabuSize;
        NeighboursLimit = neighboursLimit;
    }

    public override IEnumerable<ShiftPlan> FindOptimal(List<SuccessRatedIncidents> incidentsSets)
    {
        int Fitness(ShiftPlanTabu shiftPlanTabu)
        {
            return DampedFitness(shiftPlanTabu.Value, incidentsSets);
        }

        ShiftPlanTabu initShiftPlan
            = new ShiftPlanTabu(ShiftPlan.ConstructRandom(World.Depots, Constraints.AllowedShiftStartingTimes.ToList(),
            Constraints.AllowedShiftDurations.ToList(), Random));

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
            if (neighbourHoodMoves.Count > NeighboursLimit)
            {
                neighbourHoodMoves = neighbourHoodMoves.GetRandomSamples(NeighboursLimit, Random);
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

            bestCandidate = ModifyMakeMove(bestCandidate.Copy(), bestMove);

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
}
