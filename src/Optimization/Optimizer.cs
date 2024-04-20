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
    InitWeights();
    StartWeights.MapTo(Loss.Simulation.EmergencyServicePlan);
  }

  public abstract IEnumerable<Weights> FindOptimal(Incidents incidents);

  /// <summary>
  /// Initializes the weights.
  /// Called only once when initializing the optimizer => doesn't have to be efficient. 
  /// </summary>
  protected void InitWeights()
  {
    StartWeights = new()
    {
      MedicTeamAllocations = new Interval[World.Depots.Length, Constraints.MaxMedicTeamsOnDepotCount],
      MedicTeamsPerDepotCount = new int[World.Depots.Length],
      AmbulancesPerDepotCount = new int[World.Depots.Length],
    };

    // initialze team shifts to deallocated with random start times
    for (int i = 0; i < StartWeights.MedicTeamAllocations.GetLength(0); ++i)
    {
      for (int j = 0; j < StartWeights.MedicTeamAllocations.GetLength(1); ++j)
      {
        StartWeights.MedicTeamAllocations[i, j] = Interval.GetByStartAndDuration
        (
          ShiftTimes.GetRandomStartingTimeSec(this._random),
          0
        );
      }
    }

    // random teams allocation
    int teamsOnDepotCount = Math.Min(Constraints.MaxMedicTeamsOnDepotCount, Constraints.AvailableMedicTeamsCount / World.Depots.Length);
    int runningAvailableMedicTeamsCount = Constraints.AvailableMedicTeamsCount;

    for (int i = 0; i < World.Depots.Length; ++i)
    {
      for (int j = 0; j < teamsOnDepotCount && runningAvailableMedicTeamsCount > 0; ++j)
      {
        StartWeights.MedicTeamAllocations[i, j] = Interval.GetByStartAndDuration
        (
          ShiftTimes.GetRandomStartingTimeSec(this._random),
          ShiftTimes.GetRandomDurationTimeSec(this._random)
        );

        --runningAvailableMedicTeamsCount;
        ++StartWeights.MedicTeamsPerDepotCount[i];
      }
    }
    StartWeights.AllocatedMedicTeamsCount = Constraints.AvailableMedicTeamsCount - runningAvailableMedicTeamsCount;
    //

    // amb allocations
    int ambulancesOnDepotCount = Math.Min(Constraints.MaxAmbulancesOnDepotCount, Constraints.AvailableAmbulancesCount / World.Depots.Length);
    int runningAvailableAmbulancesCount = Constraints.AvailableAmbulancesCount;

    for (int i = 0; i < World.Depots.Length; ++i)
    {
      if (runningAvailableAmbulancesCount - ambulancesOnDepotCount < 0)
      {
        StartWeights.AmbulancesPerDepotCount[i] = runningAvailableAmbulancesCount;
        break;
      }

      StartWeights.AmbulancesPerDepotCount[i] = ambulancesOnDepotCount;
      runningAvailableAmbulancesCount -= ambulancesOnDepotCount;
    }
    StartWeights.AllocatedAmbulancesCount = StartWeights.AmbulancesPerDepotCount.Sum();
    //
  }
}

