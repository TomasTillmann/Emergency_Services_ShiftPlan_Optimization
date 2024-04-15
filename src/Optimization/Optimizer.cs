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
  public ILoss Loss { get; set; }

  /// <inheritdoc/>
  public Weights StartWeights { get; set; }

  /// Random
  protected readonly Random _random;

  public Optimizer(World world, ShiftTimes shiftTimes, ILoss loss, Random? random = null)
  {
    _random = random ?? new Random();
    ShiftTimes = shiftTimes;
    World = world;
    Loss = loss;
    StartWeights = InitWeights(world.AvailableMedicTeams.Length, shiftTimes);
  }

  public abstract IEnumerable<Weights> FindOptimal(SuccessRatedIncidents incidents);

  protected Weights InitWeights(int allAmbulancesCount, ShiftTimes constraints)
  {
    Weights startWeights = new()
    {
      Shifts = new Interval[allAmbulancesCount]
    };

    for (int i = 0; i < allAmbulancesCount; ++i)
    {
      startWeights.Shifts[i] = Interval.GetByStartAndDuration
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
}

