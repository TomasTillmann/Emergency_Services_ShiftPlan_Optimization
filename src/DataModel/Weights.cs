using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace ESSP.DataModel;

public class Weights
{
  public Interval[,] MedicTeamAllocations { get; init; }

  /// <summary>
  /// Is size of Depots. 
  /// i-th value represents how many medic teams are allocated to i-th depot. 
  /// Sum per i-th values equals <see cref="AllAllocatedMedicTeamsCount"/>.
  /// </summary>
  public int[] MedicTeamsPerDepotCount { get; init; }

  /// <summary>
  /// How many teams are allocated. Always has to be sum of <see cref="MedicTeamAllocations"/>.
  /// </summary>
  public int AllAllocatedMedicTeamsCount { get; set; }

  public AmbulanceType[,] AmbulanceTypeAllocations { get; init; }

  /// <summary>
  /// Is size of Depots. 
  /// i-th value represents how many ambulnaces are allocated to i-th depot. 
  /// Sum per i-th values equals <see cref="AllAllocatedAmbulancesCount"/>.
  /// </summary>
  public int[] AmbulancesPerDepotCount { get; init; }

  /// <summary>
  /// How many ambulances are allocated. Always has to be sum of <see cref="AmbulancesPerDepotCount"/>.
  /// </summary>
  public int AllAllocatedAmbulancesCount { get; set; }

  public Weights() { }

  public Weights(int depotsCount, int maxMedicTeamsOnDepotCount, int maxAmbulanceTeamsOnDepotCount)
  {
    MedicTeamAllocations = new Interval[depotsCount, maxMedicTeamsOnDepotCount];
    MedicTeamsPerDepotCount = new int[depotsCount];
    AmbulanceTypeAllocations = new AmbulanceType[depotsCount, maxAmbulanceTeamsOnDepotCount];
    AmbulancesPerDepotCount = new int[depotsCount];
  }

  // TODO: Permutate, otherwise not uniformly random
  public static Weights GetUniformlyRandom(World world, Constraints constraints, ShiftTimes shiftTimes, Random random = null)
  {
    random ??= new Random();

    Weights weights = new Weights(world.Depots.Length, constraints.MaxMedicTeamsOnDepotCount, constraints.MaxAmbulancesOnDepotCount);

    // random teams allocation
    int teamsOnDepotCount = Math.Min(constraints.MaxMedicTeamsOnDepotCount, constraints.AvailableMedicTeamsCount / world.Depots.Length);
    int runningAvailableMedicTeamsCount = constraints.AvailableMedicTeamsCount;

    for (int i = 0; i < world.Depots.Length; ++i)
    {
      for (int j = 0; j < teamsOnDepotCount && runningAvailableMedicTeamsCount > 0; ++j)
      {
        int durationSec = shiftTimes.GetRandomDurationTimeSec(random);
        weights.MedicTeamAllocations[i, j] = Interval.GetByStartAndDuration
        (
          shiftTimes.GetRandomStartingTimeSec(random),
          durationSec
        );

        if (durationSec != 0)
        {
          --runningAvailableMedicTeamsCount;
          ++weights.MedicTeamsPerDepotCount[i];
        }
      }
    }
    weights.AllAllocatedMedicTeamsCount = constraints.AvailableMedicTeamsCount - runningAvailableMedicTeamsCount;
    //

    // amb allocations
    int ambulancesOnDepotCount = Math.Min(constraints.MaxAmbulancesOnDepotCount, constraints.AvailableAmbulancesCount / world.Depots.Length);
    int runningAvailableAmbulancesCount = constraints.AvailableAmbulancesCount;

    for (int i = 0; i < world.Depots.Length; ++i)
    {
      for (int j = 0; j < ambulancesOnDepotCount && runningAvailableAmbulancesCount > 0; ++j)
      {
        int ambTypeIndex = random.Next(-1, world.AvailableAmbulanceTypes.Length);
        weights.AmbulanceTypeAllocations[i, j] = ambTypeIndex == -1 ? null : world.AvailableAmbulanceTypes[ambTypeIndex];

        if (ambTypeIndex != -1)
        {
          --runningAvailableAmbulancesCount;
          ++weights.AmbulancesPerDepotCount[i];
        }
      }
    }
    weights.AllAllocatedAmbulancesCount = weights.AmbulancesPerDepotCount.Sum();
    //

    return weights;
  }

  public void MapTo(EmergencyServicePlan plan)
  {
    int availableMedicTeamIndex = 0;
    int availableAmbIndex = 0;

    for (int depotIndex = 0; depotIndex < MedicTeamAllocations.GetLength(0); ++depotIndex)
    {
      for (int medicTeamIndex = 0; medicTeamIndex < MedicTeamAllocations.GetLength(1); ++medicTeamIndex)
      {
        if (MedicTeamAllocations[depotIndex, medicTeamIndex].DurationSec != 0)
        {
          plan.AvailableMedicTeams[availableMedicTeamIndex].Shift = MedicTeamAllocations[depotIndex, medicTeamIndex];
          plan.AvailableMedicTeams[availableMedicTeamIndex].Depot = plan.Depots[depotIndex];
          ++availableMedicTeamIndex;
        }
      }

      plan.Depots[depotIndex].Ambulances.Clear();
      for (int ambulanceIndex = 0; ambulanceIndex < AmbulanceTypeAllocations.GetLength(1); ++ambulanceIndex)
      {
        if (AmbulanceTypeAllocations[depotIndex, ambulanceIndex] is not null)
        {
          plan.AvailableAmbulances[availableAmbIndex].Type = AmbulanceTypeAllocations[depotIndex, ambulanceIndex];
          plan.Depots[depotIndex].Ambulances.Add(plan.AvailableAmbulances[availableAmbIndex]);
          ++availableAmbIndex;
        }
      }
    }

    plan.AllocatedAmbulancesCount = AllAllocatedAmbulancesCount;
    plan.AllocatedMedicTeamsCount = AllAllocatedMedicTeamsCount;
  }

  public Weights Copy()
  {
    Interval[,] medicTeamAllocations = new Interval[MedicTeamAllocations.GetLength(0), MedicTeamAllocations.GetLength(1)];
    for (int i = 0; i < medicTeamAllocations.GetLength(0); ++i)
    {
      for (int j = 0; j < medicTeamAllocations.GetLength(1); ++j)
      {
        medicTeamAllocations[i, j] = MedicTeamAllocations[i, j];
      }
    }

    AmbulanceType[,] ambulanceTypeAllocations = new AmbulanceType[AmbulanceTypeAllocations.GetLength(0), AmbulanceTypeAllocations.GetLength(1)];
    for (int i = 0; i < ambulanceTypeAllocations.GetLength(0); ++i)
    {
      for (int j = 0; j < ambulanceTypeAllocations.GetLength(1); ++j)
      {
        ambulanceTypeAllocations[i, j] = AmbulanceTypeAllocations[i, j];
      }
    }

    int[] medicTeamsPerDepotCount = new int[MedicTeamsPerDepotCount.Length];
    for (int i = 0; i < medicTeamsPerDepotCount.Length; ++i)
    {
      medicTeamsPerDepotCount[i] = MedicTeamsPerDepotCount[i];
    }

    int[] ambulancesPerDepotCount = new int[AmbulancesPerDepotCount.Length];
    for (int i = 0; i < AmbulancesPerDepotCount.Length; ++i)
    {
      ambulancesPerDepotCount[i] = AmbulancesPerDepotCount[i];
    }

    return new Weights
    {
      MedicTeamAllocations = medicTeamAllocations,
      MedicTeamsPerDepotCount = medicTeamsPerDepotCount,
      AllAllocatedMedicTeamsCount = this.AllAllocatedMedicTeamsCount,
      AmbulancesPerDepotCount = ambulancesPerDepotCount,
      AllAllocatedAmbulancesCount = this.AllAllocatedAmbulancesCount,
      AmbulanceTypeAllocations = ambulanceTypeAllocations
    };
  }
}


