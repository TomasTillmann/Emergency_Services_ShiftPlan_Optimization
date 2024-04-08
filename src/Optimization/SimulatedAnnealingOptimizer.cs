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
  public readonly StreamWriter _debug = new(@"/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/logs" + "/SA.log");

  #region Parameters

  public double LowestTemperature { get; set; }
  public double HighestTemperature { get; set; }
  public double TemperatureReductionFactor { get; set; }

  #endregion

  public IEnumerable<Weights> OptimalWeights => new List<Weights> { _globalBestWeights };

  private Weights _weights;
  private ImmutableArray<SuccessRatedIncidents> _incidentsSets;
  private Weights _globalBestWeights;
  private double _globalBestLoss;
  private Move _currentMove;
  private double _currentLoss;
  private double _currentTemperature;

  #region ExposedInternalState

  public int CurrStep { get; private set; }

  public Weights GlobalBestWeights => _globalBestWeights;

  public Weights CurrentWeights => _weights;

  public double GlobalBestLoss => _globalBestLoss;

  public Move CurrentMove => _currentMove;

  public double CurrentLoss => _currentLoss;

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
    _globalBestWeights = StartWeights;
    _weights = StartWeights;
    _globalBestLoss = Loss.Get(_globalBestWeights, incidentsSets);
    _currentLoss = _globalBestLoss;
    _currentTemperature = HighestTemperature;
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
    _debug.WriteLine($"global loss: {_globalBestLoss}");

    int neighboursCount = GetMovesToNeighbours(_weights);

    // Select uniformly randomly one neighbour.
    _currentMove = movesBuffer[_random.Next(0, neighboursCount)];

    // Move to the neighbour.
    ModifyMakeMove(_weights, _currentMove);

    // Get neighbour loss.
    double neighbourLoss = GetLossInternal(_weights);

    // Is the neighbour better than global best?
    if (neighbourLoss < _globalBestLoss)
    {
      _debug.WriteLine($"global loss updated to: {neighbourLoss}");
      _globalBestLoss = _currentLoss;
      _globalBestWeights = _weights.Copy();
    }

    // Should we move to the neighbour?
    if (Accept(neighbourLoss - _currentLoss, _currentTemperature))
    {
      _debug.WriteLine($"accepted move: {_currentMove}");
      _currentLoss = neighbourLoss;
    }
    else
    {
      ModifyUnmakeMove(_weights, _currentMove);
    }

    _currentTemperature *= TemperatureReductionFactor;
    _debug.WriteLine($"current temp: {_currentTemperature}");
  }

  private double GetLossInternal(Weights weights)
  {
    return Loss.Get(weights, _incidentsSets);
  }

  private bool Accept(double difference, double temperature)
  {
    // boltzman distribution
    const double boltzmanConstant = 1.00000000000000000000000380649;
    double probability = Math.Exp(-difference / (boltzmanConstant * temperature));

    double random = _random.Next(0, 100) / 100d;
    return random < probability;
  }
}
