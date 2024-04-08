using System.Collections.Immutable;
using ESSP.DataModel;
using Microsoft.Win32.SafeHandles;
using Model.Extensions;
using Simulating;

namespace Optimizing;

public abstract class Optimizer : IOptimizer
{
  /// <inheritdoc/>
  public Constraints Constraints { get; }

  /// <inheritdoc/>
  public World World { get; }

  /// <inheritdoc/>
  public ILoss Loss { get; set; }

  /// <inheritdoc/>
  public Weights StartWeights { get; set; }

  /// Random
  protected readonly Random _random;

  public Optimizer(World world, Constraints constraints, ILoss loss, Random? random = null)
  {
    _random = random ?? new Random();
    Constraints = constraints;
    World = world;
    Loss = loss;
    StartWeights = InitWeights(world.AllAmbulancesCount, constraints);
  }

  public abstract IEnumerable<Weights> FindOptimal(ImmutableArray<SuccessRatedIncidents> incidentsSets);

  protected Weights InitWeights(int allAmbulancesCount, Constraints constraints)
  {
    Weights startWeights = new()
    {
      Value = new Interval[allAmbulancesCount]
    };

    for (int i = 0; i < allAmbulancesCount; ++i)
    {
      startWeights.Value[i] = Interval.GetByStartAndDuration
      (
       constraints.GetRandomStartingTimeSec(this._random),
       constraints.GetRandomDurationTimeSec(this._random)
      );
    }

    // for (int i = 0; i < startWeights.Value.Length; ++i)
    // {
    //   startWeights.Value[i] = Interval.Empty;
    // }

    return startWeights;
  }

  public virtual void Dispose()
  {
  }
}

