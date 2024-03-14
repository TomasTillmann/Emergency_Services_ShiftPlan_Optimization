using ESSP.DataModel;
using Model.Extensions;
using Optimizing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization;

public class SimulatedAnnealingOptimizer : LocalSearchOptimizer, IStepOptimizer
{
    #region Parameters
   
    public double LowestTemperature { get; set; }
    public double HighestTemperature { get; set; }
    public double TemperatureReductionFactor { get; set; }
    public int NeighboursLimit { get; set; }
    
    #endregion
    
    public int CurrStep { get; protected set; }
    
    public ShiftPlan StartShiftPlan { get; set; }
    public IEnumerable<ShiftPlan> OptimalShiftPlans => new List<ShiftPlan> { _globalBest };


    public readonly Random Random;

    private List<SuccessRatedIncidents> _incidentsSets;
    private ShiftPlan _globalBest;
    private int _globalBestFitness;
    private Move? _currentBestMove;
    private ShiftPlan _currentBest;
    private int _currentBestFitness;
    private double _currentTemperature;
    
    #region InternalState
    
    public ShiftPlan GlobalBest => _globalBest;
    public int GlobalBestFitness => _globalBestFitness;
    public Move? CurrentBestMove => _currentBestMove;
    public ShiftPlan CurrentBest => _currentBest;
    public int CurrentBestFitness => _currentBestFitness;
    public double CurrentTemperature => _currentTemperature;
    
    #endregion

    public SimulatedAnnealingOptimizer(World world, Domain constraints, double lowestTemperature = 0.1, double highestTemperature = 50, double temperatureReductionFactor = 0.2, int neighbourLimit = int.MaxValue, Random? random = null) : base(world, constraints)
    {
        Random = random ?? new Random();
        LowestTemperature = lowestTemperature;
        HighestTemperature = highestTemperature;
        TemperatureReductionFactor = temperatureReductionFactor;
        NeighboursLimit = neighbourLimit;
    }

    public override IEnumerable<ShiftPlan> FindOptimal(List<SuccessRatedIncidents> incidentsSets)
    {
        StartShiftPlan
            = ShiftPlan.ConstructRandom(World.Depots, Constraints.AllowedShiftStartingTimes.ToList(),
                Constraints.AllowedShiftDurations.ToList(), Random);
        

        InitStepThroughOptimizer(incidentsSets);
        Run();
        return OptimalShiftPlans;
    }
    
    public override IEnumerable<ShiftPlan> FindOptimalFrom(ShiftPlan startShiftPlan, List<SuccessRatedIncidents> incidentsSets)
    {
        StartShiftPlan = startShiftPlan;
        InitStepThroughOptimizer(incidentsSets);
        Run();
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
        
        _incidentsSets = incidentsSets;

        _globalBest = StartShiftPlan; 
        _globalBestFitness = Fitness(_globalBest, incidentsSets);

        _currentBestMove = null;
        _currentBest = StartShiftPlan; 
        _currentBestFitness = _globalBestFitness;
        _currentTemperature = HighestTemperature;
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
        int Fitness(ShiftPlan shiftPlan)
        {
            return DampedFitness(shiftPlan, _incidentsSets);
        }
        
        List<Move> moves = GetNeighborhoodMoves(_currentBest).ToList();
        if (moves.Count > NeighboursLimit)
        {
            moves = moves.GetRandomSamples(NeighboursLimit, Random);
        }

        foreach (Move move in moves)
        {
            ModifyMakeMove(_currentBest, move);
            int neighbourFitness = Fitness(_currentBest);

            if (neighbourFitness < _currentBestFitness)
            {
                _currentBestMove = move;
                _currentBestFitness = neighbourFitness;

                if (_currentBestFitness < _globalBestFitness)
                {
                    _globalBest = _currentBest.Copy();
                    _globalBestFitness = _currentBestFitness;
                }
            }
            else if (Accept(_currentBestFitness - neighbourFitness, _currentTemperature))
            {
                _currentBestMove = move;
                _currentBestFitness = neighbourFitness;
            }

            ModifyUnmakeMove(_currentBest, move);
        }

        if (_currentBestMove is null || _currentBest is null)
        {
            throw new ArgumentException("All neighbours either have worse fitness and even none was accepted, leading to no move being selected.");
        }

        _currentBest = ModifyMakeMove(_currentBest.Copy(), _currentBestMove);
        
        _currentTemperature *= TemperatureReductionFactor;
        CurrStep++;
    }

    public bool IsFinished()
    {

        return _currentTemperature <= LowestTemperature;
    }

    private bool Accept(double difference, double temperature)
    {
        const double boltzmanConstant = 1.00000000000000000000000380649;
        double probability = Math.Exp(-difference / (boltzmanConstant * temperature));
        double random = Random.Next(0, 100) / 100d;

        return random < probability;
    }
}
