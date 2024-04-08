using ESSP.DataModel;
using System.Collections.Immutable;

namespace Optimizing;

//TODO: support for end the searh when converges to some minima and doesnt want to go anywhere, instead of just iterations
public sealed class TabuSearchOptimizer : LocalSearchOptimizer, IStepOptimizer, IDisposable
{
  private readonly StreamWriter _debug = new(@"/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/logs" + "/tabu.log");

  #region Params

  public int Iterations { get; set; }
  public int TabuSize { get; set; }
  public Random Random => _random;

  #endregion

  private ImmutableArray<SuccessRatedIncidents> _incidentsSets;
  private Weights _globalBestWeights;
  private double _globalBestLoss;
  private double _currentBestLoss;
  private Move _currentBestMove;
  private LinkedList<Move> _tabu;
  private bool _isStuck;

  #region ExposedInternalState

  public Weights GlobalBestWeights => _globalBestWeights;
  public double GlobalBestLoss => _globalBestLoss;
  public double CurrentBestLoss => _currentBestLoss;
  public Move CurrentBestMove => _currentBestMove;
  public LinkedList<Move> Tabu => _tabu;

  #endregion

  public IEnumerable<Weights> OptimalWeights => new List<Weights> { _globalBestWeights };

  public int CurrStep { get; private set; }

  /// <param name="tabuSize">If set too high, it could happen, that all neighbours are tabu (and aspiration criterion is not satisfied either).
  /// <param name="neighboursLimit">If count of neighbours is exceeded, only first <paramref name="neighboursLimit"/> neighbours will be tried.
  public TabuSearchOptimizer
  (
    World world,
    Constraints constraints,
    ILoss loss,
    int iterations = 50,
    int tabuSize = 15,
    int neighboursLimit = int.MaxValue,
    Random? random = null
  )
  : base(world, constraints, loss, neighboursLimit, random)
  {
    Iterations = iterations;
    TabuSize = tabuSize;
    _tabu = new LinkedList<Move>();
    _isStuck = false;
  }

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
    _globalBestLoss = GetLossInternal(_globalBestWeights);

    _tabu.Clear();
  }

  public void Step()
  {
    StepInternal();
    CurrStep++;
  }

  public bool IsFinished()
  {
    return CurrStep == Iterations || _isStuck;
  }

  private void StepInternal()
  {
    int neighboursCount = GetMovesToNeighbours(_globalBestWeights);
    Console.WriteLine($"neighbours: {neighboursCount}");

    //_debug.WriteLine("moves: " + string.Join(", ", movesBuffer));
    //_debug.WriteLine("global best loss: " + _globalBestLoss);
    //_debug.WriteLine("global best weights: " + string.Join(", ", _globalBestWeights));

    _currentBestLoss = double.MaxValue;
    for (int i = 0; i < neighboursCount; ++i)
    {
      Move move = movesBuffer[i];

      ModifyMakeMove(_globalBestWeights, move);
      //_debug.WriteLine("neighbour: " + string.Join(", ", _globalBestWeights));

      double neighbourLoss = GetLossInternal(_globalBestWeights);
      //_debug.WriteLine("neighbour loss: " + neighbourLoss);

      // Not in tabu, or aspiration criterion is satisfied (possibly in tabu).
      if (!_tabu.Contains(move) || neighbourLoss < _globalBestLoss)
      {
        // Is better than current? Always true when aspiration criterion was satisfied.
        if (neighbourLoss < _currentBestLoss)
        {
          _currentBestMove = move;
          //_debug.WriteLine("current best move updated to: " + _currentBestMove);

          _currentBestLoss = neighbourLoss;
          //_debug.WriteLine("current best loss updated to: " + _currentBestLoss);
        }
      }

      ModifyUnmakeMove(_globalBestWeights, move);
    }

    // This happens iff all neighbours are in tabu and worse than already found global best.
    if (_currentBestLoss == double.MaxValue)
    {
      _isStuck = true;
      return;
    }

    if (_currentBestLoss < _globalBestLoss)
    {
      ModifyMakeMove(_globalBestWeights, _currentBestMove);
      //_debug.WriteLine("global best weights updated to: " + string.Join(", ", _globalBestWeights));
      //_debug.WriteLine("updated by move: " + _currentBestMove);

      _globalBestLoss = _currentBestLoss;
      //_debug.WriteLine("global best loss updated to: " + _globalBestLoss);
    }

    _tabu.AddLast(_currentBestMove);
    if (_tabu.Count > TabuSize)
    {
      _tabu.RemoveFirst();
    }

    //_debug.WriteLine("==================");
  }

  private double GetLossInternal(Weights weights)
  {
    return Loss.Get(weights, _incidentsSets);
  }

  public override void Dispose()
  {
    //_debug.Dispose();
  }
}
