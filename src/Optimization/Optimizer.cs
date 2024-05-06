using System.Collections.Immutable;
using ESSP.DataModel;
using Microsoft.Win32.SafeHandles;
using Model.Extensions;
using Simulating;

namespace Optimizing;

public abstract class Optimizer : IOptimizer
{
  public TextWriter Debug { get; set; }

  /// <inheritdoc/>
  public ShiftTimes ShiftTimes { get; }

  /// <inheritdoc/>
  public World World { get; }

  /// <inheritdoc/>
  public Constraints Constraints { get; }

  /// <inheritdoc/>
  public IObjectiveFunction ObjectiveFunction { get; set; }

  /// <inheritdoc/>
  public Weights StartWeights { get; set; }

  /// Random
  protected readonly Random _random;

  public Optimizer(World world, Constraints constraints, ShiftTimes shiftTimes, IObjectiveFunction objectiveFunction, Random? random = null)
  {
    _random = random ?? new Random();
    ShiftTimes = shiftTimes;
    World = world;
    ObjectiveFunction = objectiveFunction;
    Constraints = constraints;
    InitWeights();
    StartWeights.MapTo(ObjectiveFunction.Simulation.EmergencyServicePlan);
  }

  public abstract IEnumerable<Weights> FindOptimal(ImmutableArray<Incident> incidents);

  /// <summary>
  /// Initializes the weights.
  /// Called only once when initializing the optimizer => doesn't have to be efficient. 
  /// </summary>
  protected void InitWeights()
  {
    StartWeights = Weights.GetUniformlyRandom(World, Constraints, ShiftTimes, _random);
  }
}

