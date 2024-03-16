using ESSP.DataModel;
using System.Collections.Immutable;

namespace Optimizing;

//TODO: support for end the searh when converges to some minima and doesnt want to go anywhere, instead of just iterations
public sealed class TabuSearchOptimizer : LocalSearchOptimizer, IStepOptimizer
{
  #region Params

  public int Iterations { get; set; }
  public int TabuSize { get; set; }
  public int NeighboursLimit { get; set; }
  public Random Random { get; set; }

  #endregion

  private ImmutableArray<SuccessRatedIncidents> _incidentsSets;
  private Weights _weights;
  private Weights _globalBestWeights;
  private double _globalBestLoss;
  private double _currentBestLoss;
  private Move _currentBestMove;
  private LinkedList<Move> _tabu;

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
    int iterations = 150,
    int tabuSize = 50,
    int neighboursLimit = int.MaxValue,
    Random? random = null
  )
  : base(world, constraints, loss)
  {
    Iterations = iterations;
    TabuSize = tabuSize;
    NeighboursLimit = neighboursLimit;
    Random = random ?? new Random();
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
    _weights = StartWeights;
    _globalBestWeights = _weights;
    _globalBestLoss = GetLossInternal(_globalBestWeights);

    _tabu = new LinkedList<Move>();
  }

  public void Step()
  {
    StepInternal();
    CurrStep++;
  }

  public bool IsFinished()
  {
    return CurrStep == Iterations;
  }

  private void StepInternal()
  {
    //TODO: Something more sophisticated than just taking the first
    IEnumerable<Move> movesToNeighbours = GetMovesToNeighbours(_weights).Take(NeighboursLimit);

    // might be in local minima already 
    _currentBestMove = Move.Identity;
    _currentBestLoss = GetLossInternal(_weights);

    foreach (Move move in movesToNeighbours)
    {
      ModifyMakeMove(_weights, move);

      double neighbourLoss = GetLossInternal(_weights);

      // not in tabu, or aspiration criterion is satisfied (possibly in tabu)
      if (!_tabu.Contains(move) || neighbourLoss < _globalBestLoss)
      {
        // update current best move
        if (neighbourLoss < _currentBestLoss)
        {
          _currentBestMove = move;
          _currentBestLoss = neighbourLoss;
        }
      }

      ModifyUnmakeMove(_weights, move);
    }

    if (_currentBestLoss < _globalBestLoss)
    {
      _globalBestWeights = _weights.Copy();
      ModifyMakeMove(_globalBestWeights, _currentBestMove);

      _globalBestLoss = _currentBestLoss;
    }

    _tabu.AddLast(_currentBestMove);
    if (_tabu.Count > TabuSize)
    {
      _tabu.RemoveFirst();
    }
  }

  private double GetLossInternal(Weights weights)
  {
    return Loss.Get(weights, _incidentsSets);
  }
}
