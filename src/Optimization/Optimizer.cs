using System.Collections.Immutable;
using ESSP.DataModel;
using Microsoft.Win32.SafeHandles;
using SimulatingOptimized;

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

  protected SimulationOptimized Simulation;

  public Optimizer(World world, Constraints constraints, ILoss loss)
  {
    Constraints = constraints;
    World = world;
    Loss = loss;
    StartWeights = InitWeights();

    Simulation = new SimulationOptimized(world);
  }

  public abstract IEnumerable<Weights> FindOptimal(ImmutableArray<SuccessRatedIncidents> incidentsSets);

  protected Weights InitWeights()
  {
    // TODO: Construct initial weights cleverly 
    throw new NotImplementedException("InitWeights not implemented yet.");
  }
}

