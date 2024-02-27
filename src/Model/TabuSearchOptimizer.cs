using ESSP.DataModel;
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

public sealed class TabuSearchOptimizer : LocalSearchOptimizer, ILocalSearchStepOptimizer
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
    public int Iterations { get; set; }
    public int TabuSize { get; set; }
    public int NeighboursLimit { get; set; }
    
    public readonly Random Random;
    
    #endregion

    private List<SuccessRatedIncidents> _incidentsSets;
    private ShiftPlanTabu _startShiftPlanTabu; 
    private ShiftPlanTabu _globalBest;
    private int _globalBestFitness;
    private ShiftPlanTabu _bestCandidate;
    private int? _bestCandidateFitness;
    private ShiftPlanTabu _candidate;
    private Move? _bestMove;
    private LinkedList<Move> _tabu;
    
    # region InternalState
    
    public ShiftPlan GlobalBest => _globalBest.Value;
    public int GlobalBestFitness => _globalBestFitness;
    public ShiftPlan BestCandidate => _bestCandidate.Value;
    public int? BestCandidateFitness => _bestCandidateFitness;
    public Move? BestMove => _bestMove;
    public LinkedList<Move> Tabu => _tabu;
    
    #endregion
    
    public ShiftPlan StartShiftPlan { get; set; }

    public IEnumerable<ShiftPlan> OptimalShiftPlans => new List<ShiftPlan> { _globalBest.Value };
    public int CurrStep { get; private set; }

    /// <param name="world"></param>
    /// <param name="constraints"></param>
    /// <param name="iterations"></param>
    /// <param name="tabuSize">If set too high, it could happen, that all neighbours are tabu (and aspiration criterion is not satisfied either).
    /// <see cref="Exception"/> will be thrown.</param>
    /// <param name="seed">Seed used for random sample of neighbours list.</param>
    /// <param name="neighboursLimit">If count of neighbours is exceeded, uniformly random sample of this size will be taken as representants of all neighbours.
    /// The more the shifts, the more the neighbours. Running hundreads of simulations in one iteration can be too expensive. This helps this issue.</param>
    /// <param name="seed">Initial <see cref="ShiftPlan"/> is selected randomly. When limiting neighbours to search by <paramref name="neighboursLimit"/>, neighbours to search are selected at random.</param>
    public TabuSearchOptimizer(World world, Domain constraints, int iterations = 150, int tabuSize = 50, int neighboursLimit = int.MaxValue, Random? random = null) : base(world, constraints)
    {
        Random = random ?? new Random(); 
        Iterations = iterations;
        TabuSize = tabuSize;
        NeighboursLimit = neighboursLimit;
    }

    public override IEnumerable<ShiftPlan> FindOptimal(List<SuccessRatedIncidents> incidentsSets)
    {
        StartShiftPlan
            = ShiftPlan.ConstructRandom(World.Depots, Constraints.AllowedShiftStartingTimes.ToList(),
                Constraints.AllowedShiftDurations.ToList(), Random);
        
        InitStepThroughOptimizer(incidentsSets);
        (this as IStepOptimizer).Run();
        return OptimalShiftPlans;
    }
    
    public override IEnumerable<ShiftPlan> FindOptimalFrom(ShiftPlan startShiftPlan, List<SuccessRatedIncidents> incidentsSets)
    {
        StartShiftPlan = startShiftPlan;
        InitStepThroughOptimizer(incidentsSets);
        (this as IStepOptimizer).Run();
        return OptimalShiftPlans;
    }
    
    public void InitStepThroughOptimizer(List<SuccessRatedIncidents> incidentsSets)
    {
        if (StartShiftPlan is null)
        {
            StartShiftPlan
                = ShiftPlan.ConstructRandom(World.Depots, Constraints.AllowedShiftStartingTimes.ToList(),
                    Constraints.AllowedShiftDurations.ToList(), Random);
        }
        
        _startShiftPlanTabu = new ShiftPlanTabu(StartShiftPlan); 
        _incidentsSets = incidentsSets;
        _globalBest = _startShiftPlanTabu;
        _bestCandidate = _startShiftPlanTabu;
        
        _globalBestFitness = Fitness(_globalBest.Value, incidentsSets);
        _bestCandidateFitness = _globalBestFitness;

        _tabu = new LinkedList<Move>();
        // TODO: Init tabu?
    }

    public void Run()
    {
        while (!IsFinished())
        {
            Step();
        }
    }
    
    public void Step()
    {
        int Fitness(ShiftPlanTabu shiftPlanTabu)
        {
            return DampedFitness(shiftPlanTabu.Value, _incidentsSets);
        }

        List<Move> neighbourHoodMoves = GetNeighborhoodMoves(_bestCandidate).ToList();
        if (neighbourHoodMoves.Count > NeighboursLimit)
        {
            neighbourHoodMoves = neighbourHoodMoves.GetRandomSamples(NeighboursLimit, Random);
        }

        _bestMove = null;
        _bestCandidateFitness = null;

        foreach (Move move in neighbourHoodMoves)
        {
            _candidate = ModifyMakeMove(_bestCandidate, move);

            int candidateFitness = Fitness(_candidate);

            if (_tabu.Contains(move))
            {
                // aspiration criterion
                if (candidateFitness < _globalBestFitness)
                {
                    if (_bestCandidateFitness is null || candidateFitness < _bestCandidateFitness)
                    {
                        _bestMove = move;
                        _bestCandidateFitness = candidateFitness;
                    }
                }
            }
            else
            {
                if (_bestCandidateFitness is null || candidateFitness < _bestCandidateFitness)
                {
                    _bestMove = move;
                    _bestCandidateFitness = candidateFitness;
                }
            }

            ModifyUnmakeMove(_bestCandidate, move);
        }

        if (_bestMove is null || _bestCandidateFitness is null)
        {
            throw new ArgumentException("All neighbours were tabu and none of them also satisfied aspiration criterion. Perhaps you set tabu size too high?");
        }

        _bestCandidate = ModifyMakeMove(_bestCandidate.Copy(), _bestMove);

        if (_bestCandidateFitness < _globalBestFitness)
        {
            _globalBest = _bestCandidate;
            _globalBestFitness = _bestCandidateFitness.Value;
        }

        _tabu.AddLast(_bestMove);
        if (_tabu.Count > TabuSize)
        {
            _tabu.RemoveFirst();
        }

        CurrStep++;
    }
    
    public bool IsFinished()
    {
        return CurrStep == Iterations;
    }
}
