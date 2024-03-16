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

  public Optimizer(World world, Constraints constraints, ILoss loss)
  {
    Constraints = constraints;
    World = world;
    Loss = loss;
    StartWeights = InitWeights();
  }

  public abstract IEnumerable<Weights> FindOptimal(ImmutableArray<SuccessRatedIncidents> incidentsSets);

  protected Weights InitWeights()
  {
    // TODO: Construct initial weights cleverly 
    // HACK: 10_000
    return ShiftPlan.GetFrom(World.Depots, 10_000).ToWeights();
  }
}

