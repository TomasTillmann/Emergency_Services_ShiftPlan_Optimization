using ESSP.DataModel;
using System.Collections.Immutable;

namespace Optimizing;

//TODO: support for end the searh when converges to some minima and doesnt want to go anywhere, instead of just iterations
public sealed class TabuSearchOptimizer : LocalSearchOptimizer, IStepOptimizer
{
  #region Params

  public int Iterations { get; set; }
  public int TabuSize { get; set; }
  public Random Random => _random;

  #endregion

  private SuccessRatedIncidents _incidentsSets;
  private Weights _globalBestWeights;
  private Weights _weights;
  private double _globalBestLoss;
  private double _currentBestLoss;
  private Move _currentBestMove;
  private LinkedList<Move> _tabu;
  private bool _isStuck;

  #region ExposedInternalState

  public Weights GlobalBestWeights => _globalBestWeights;
  public Weights CurrentWeights => _weights;
  public double GlobalBestLoss => _globalBestLoss;
  public double CurrentBestLoss => _currentBestLoss;
  public Move CurrentBestMove => _currentBestMove;
  public LinkedList<Move> Tabu => _tabu;

  #endregion

  public IEnumerable<Weights> OptimalWeights => new List<Weights> { _globalBestWeights };

  public int CurrStep { get; private set; }

  /// <param name="tabuSize">If set too high, it could happen, that all neighbours are tabu (and aspiration criterion is not satisfied either).
  /// <param name="shiftChangesLimit">If count of neighbours is exceeded, only first <paramref name="shiftChangesLimit"/> neighbours will be tried.
  public TabuSearchOptimizer
  (
    World world,
    Constraints constraints,
    ShiftTimes shiftTimes,
    ILoss loss,
    int iterations = 50,
    int tabuSize = 15,
    int shiftChangesLimit = int.MaxValue,
    int allocationsLimit = int.MaxValue,
    Random? random = null
  )
  : base(world, constraints, shiftTimes, loss, shiftChangesLimit, allocationsLimit, random)
  {
    Iterations = iterations;
    TabuSize = tabuSize;
    _tabu = new LinkedList<Move>();
    _isStuck = false;
  }

  public override IEnumerable<Weights> FindOptimal(SuccessRatedIncidents incidents)
  {
    InitStepOptimizer(incidents);

    while (!IsFinished())
    {
      Debug.WriteLine($"Step: {CurrStep}");
      Step();
    }

    return OptimalWeights;
  }

  public void InitStepOptimizer(SuccessRatedIncidents incidents)
  {
    _incidentsSets = incidents;
    _globalBestWeights = StartWeights;
    _weights = StartWeights;
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
    GetMovesToNeighbours(_weights);

    //_debug.WriteLine("moves: " + string.Join(", ", movesBuffer));
    Debug.WriteLine("global best loss: " + _globalBestLoss);
    //_debug.WriteLine("global best weights: " + string.Join(", ", _globalBestWeights));

    _currentBestLoss = double.MaxValue;
    for (int i = 0; i < movesBuffer.Count; ++i)
    {
      Move move = movesBuffer[i];

      ModifyMakeMove(_weights, move);
      //_debug.WriteLine("neighbour: " + string.Join(", ", _globalBestWeights));

      double neighbourLoss = GetLossInternal(_weights);
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
          Debug.WriteLine("current best loss updated to: " + _currentBestLoss);
        }
      }

      ModifyUnmakeMove(_weights, move);
    }

    // This happens iff all neighbours are in tabu and worse than already found global best.
    if (_currentBestLoss == double.MaxValue)
    {
      Debug.WriteLine("STUCK");
      _isStuck = true;
      return;
    }

    // move in the best direction
    ModifyMakeMove(_weights, _currentBestMove);

    // update global best
    if (_currentBestLoss < _globalBestLoss)
    {
      _globalBestWeights = _weights.Copy();
      Debug.WriteLine("global best weights updated to: " + string.Join(", ", _globalBestWeights));
      Debug.WriteLine("global best move updated to: " + _currentBestMove);

      _globalBestLoss = _currentBestLoss;
      //_debug.WriteLine("global best loss updated to: " + _globalBestLoss);
    }

    // Tabu the inverse move, not the actual move. Only this way, it will not "move back".
    _tabu.AddLast
    (
      new Move
      {
        MedicTeamOnDepotIndex = _currentBestMove.MedicTeamOnDepotIndex,
        MoveType = LocalSearchOptimizer.GetInverseMoveType(_currentBestMove.MoveType)
      }
    );

    if (_tabu.Count > TabuSize)
    {
      _tabu.RemoveFirst();
    }

    Debug.WriteLine("==================");
  }

  private double GetLossInternal(Weights weights)
  {
    return Loss.Get(weights, _incidentsSets);
  }
}
