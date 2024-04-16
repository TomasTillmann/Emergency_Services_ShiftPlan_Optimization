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
  public ILoss Loss { get; set; }

  /// <inheritdoc/>
  public Weights StartWeights { get; set; }

  /// Random
  protected readonly Random _random;

  public Optimizer(World world, Constraints constraints, ShiftTimes shiftTimes, ILoss loss, Random? random = null)
  {
    _random = random ?? new Random();
    ShiftTimes = shiftTimes;
    World = world;
    Loss = loss;
    Constraints = constraints;
    StartWeights = InitWeights();
    Loss.Map(StartWeights);
  }

  public abstract IEnumerable<Weights> FindOptimal(SuccessRatedIncidents incidents);

  /// <summary>
  /// Initializes the weights.
  /// Called only once when initializing the optimizer => doesn't have to be efficient. 
  /// </summary>
  protected Weights InitWeights()
  {
    Weights startWeights = new()
    {
      MedicTeamShifts = new Interval[World.AvailableMedicTeams.Length],
      MedicTeamAllocations = new int[World.Depots.Length],
      AmbulancesAllocations = new int[World.Depots.Length]
    };

    // teams allocation
    int teamsOnDepotCount = Math.Min(Constraints.MaxMedicTeamsOnDepotCount, Constraints.AvailableMedicTeamsCount / World.Depots.Length);
    int runningAvailableMedicTeamsCount = Constraints.AvailableMedicTeamsCount;

    for (int i = 0; i < World.Depots.Length; ++i)
    {
      if (runningAvailableMedicTeamsCount - teamsOnDepotCount < 0)
      {
        startWeights.MedicTeamAllocations[i] = runningAvailableMedicTeamsCount;
        break;
      }

      startWeights.MedicTeamAllocations[i] = teamsOnDepotCount;
      runningAvailableMedicTeamsCount -= teamsOnDepotCount;
    }
    startWeights.AllocatedTeamsCount = startWeights.MedicTeamAllocations.Sum();
    //

    // amb allocations
    int ambulancesOnDepotCount = Math.Min(Constraints.MaxAmbulancesOnDepotCount, Constraints.AvailableAmbulancesCount / World.Depots.Length);
    int runningAvailableAmbulancesCount = Constraints.AvailableAmbulancesCount;

    for (int i = 0; i < World.Depots.Length; ++i)
    {
      if (runningAvailableAmbulancesCount - ambulancesOnDepotCount < 0)
      {
        startWeights.AmbulancesAllocations[i] = runningAvailableAmbulancesCount;
        break;
      }

      startWeights.AmbulancesAllocations[i] = ambulancesOnDepotCount;
      runningAvailableAmbulancesCount -= ambulancesOnDepotCount;
    }
    startWeights.AllocatedAmbulancesCount = startWeights.AmbulancesAllocations.Sum();
    //

    // shift assigment to allocated teams
    for (int i = 0; i < startWeights.AllocatedTeamsCount; ++i)
    {
      startWeights.MedicTeamShifts[i] = Interval.GetByStartAndDuration
      (
        ShiftTimes.GetRandomStartingTimeSec(this._random),
        ShiftTimes.GetRandomDurationTimeSec(this._random)
      );
    }
    //

    return startWeights;
  }
}

