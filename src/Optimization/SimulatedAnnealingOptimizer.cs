using ESSP.DataModel;
using Model.Extensions;
using Optimizing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
  public Random Random => _random;

  #endregion

  public IEnumerable<Weights> OptimalWeights => new List<Weights> { _globalBestWeights };

  private Weights _weights;
  private ImmutableArray<SuccessRatedIncidents> _incidentsSets;
  private Weights _globalBestWeights;
  private double _globalBestLoss;
  private Move _currentBestMove;
  private double _currentBestLoss;
  private double _currentTemperature;

  #region ExposedInternalState

  public int CurrStep { get; private set; }

  public Weights GlobalBestWeights => _globalBestWeights;

  public Weights Weights => _weights;

  public double GlobalBestFitness => _globalBestLoss;

  public Move CurrentBestMove => _currentBestMove;

  public double CurrentBestFitness => _currentBestLoss;

  public double CurrentTemperature => _currentTemperature;

  #endregion

  public SimulatedAnnealingOptimizer(
      World world,
      Constraints constraints,
      ILoss loss,
      double lowestTemperature = 0.001,
      double highestTemperature = 100,
      double temperatureReductionFactor = 0.98,
      int neighboursLimit = int.MaxValue,
      Random? random = null
  ) : base(world, constraints, loss, neighboursLimit, random)
  {
    LowestTemperature = lowestTemperature;
    HighestTemperature = highestTemperature;
    TemperatureReductionFactor = temperatureReductionFactor;
    NeighboursLimit = neighboursLimit;
  }

  /// <inheritdoc/>
  public override IEnumerable<Weights> FindOptimal(ImmutableArray<SuccessRatedIncidents> incidentsSets)
  {
    InitStepOptimizer(incidentsSets);

    while (!IsFinished())
    {
      StepInternal();
    }

    return OptimalWeights;
  }

  public void InitStepOptimizer(ImmutableArray<SuccessRatedIncidents> incidentsSets)
  {
    _incidentsSets = incidentsSets;
    _weights = StartWeights;
    _globalBestWeights = StartWeights;
    _globalBestLoss = Loss.Get(_globalBestWeights, incidentsSets);
    _currentBestLoss = _globalBestLoss;
    _currentTemperature = HighestTemperature;
    _currentBestMove = Move.Identity;
  }

  public void Step()
  {
    StepInternal();
    CurrStep++;
  }

  public bool IsFinished()
  {
    return _currentTemperature <= LowestTemperature;
  }

  private void StepInternal()
  {
    //TODO: do some random sampling, instead of just choosing first n GetMovesToNeighbours
    //TODO: optimize. Dont generate moves which you will discard by Take(). Just generate _neigbourLimit_ number of moves.
    int neighboursCount = GetMovesToNeighbours(_weights);

    for (int i = 0; i < neighboursCount; ++i)
    {
      Move move = movesBuffer[i];

      ModifyMakeMove(_weights, move);
      double neighbourLoss = GetLossInternal(_weights);

      if (neighbourLoss < _currentBestLoss)
      {
        _currentBestMove = move;
        _currentBestLoss = neighbourLoss;

        if (_currentBestLoss < _globalBestLoss)
        {
          _globalBestWeights = _weights.Copy();

          Console.WriteLine("Global best update:");
          Console.WriteLine(_globalBestWeights);

          ModifyMakeMove(_globalBestWeights, _currentBestMove);

          _globalBestLoss = _currentBestLoss;
        }
      }
      else if (Accept(_currentBestLoss - neighbourLoss, _currentTemperature))
      {
        _currentBestMove = move;
        _currentBestLoss = neighbourLoss;
      }

      ModifyUnmakeMove(_weights, move);
    }

    // Go to the neigbour either with best loss, or the one accepted with possibly worse loss.
    // With higher temperature, there is higher chance of exploration of neighbours instead of converting to local minima.
    ModifyMakeMove(_weights, _currentBestMove);

    // Reduce the temperature, to increase chance of exploiting best neighbours, instead of exploring the neighbours.
    _currentTemperature *= TemperatureReductionFactor;
  }

  private double GetLossInternal(Weights weights)
  {
    return Loss.Get(weights, _incidentsSets);
  }

  private bool Accept(double difference, double temperature)
  {
    const double boltzmanConstant = 1.00000000000000000000000380649;
    double probability = Math.Exp(-difference / (boltzmanConstant * temperature));
    double random = Random.Next(0, 100) / 100d;

    return random < probability;
  }
}
