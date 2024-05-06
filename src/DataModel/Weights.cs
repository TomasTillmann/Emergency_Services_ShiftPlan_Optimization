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

  public Weights(int depotsCount, int maxMedicTeamsOnDepotCount)
  {
    MedicTeamAllocations = new Interval[depotsCount, maxMedicTeamsOnDepotCount];
    MedicTeamsPerDepotCount = new int[depotsCount];
    AmbulancesPerDepotCount = new int[depotsCount];
  }

  public static Weights GetUniformlyRandom(World world, Constraints constraints, ShiftTimes shiftTimes, Random random = null)
  {
    random ??= new Random();

    Weights weights = new Weights(world.Depots.Length, constraints.MaxMedicTeamsOnDepotCount);

    // random teams allocation
    int teamsOnDepotCount = Math.Min(constraints.MaxMedicTeamsOnDepotCount, constraints.AvailableMedicTeamsCount / world.Depots.Length);
    int runningAvailableMedicTeamsCount = constraints.AvailableMedicTeamsCount;

    for (int i = 0; i < world.Depots.Length; ++i)
    {
      for (int j = 0; j < teamsOnDepotCount && runningAvailableMedicTeamsCount > 0; ++j)
      {
        int duration = shiftTimes.GetRandomDurationTimeSec(random);
        weights.MedicTeamAllocations[i, j] = Interval.GetByStartAndDuration
        (
          shiftTimes.GetRandomStartingTimeSec(random),
          duration
        );

        if (duration != 0)
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
      if (runningAvailableAmbulancesCount - ambulancesOnDepotCount < 0)
      {
        weights.AmbulancesPerDepotCount[i] = runningAvailableAmbulancesCount;
        break;
      }

      weights.AmbulancesPerDepotCount[i] = random.Next(0, ambulancesOnDepotCount);
      runningAvailableAmbulancesCount -= weights.AmbulancesPerDepotCount[i];
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
      for (int _ = 0; _ < AmbulancesPerDepotCount[depotIndex]; ++_)
      {
        plan.Depots[depotIndex].Ambulances.Add(plan.AvailableAmbulances[availableAmbIndex++]);
      }
    }

    plan.AllocatedAmbulancesCount = AllAllocatedAmbulancesCount;
    plan.AllocatedMedicTeamsCount = AllAllocatedMedicTeamsCount;
  }

  public static Weights GetFrom(EmergencyServicePlan plan, int maxMedicTeamsOnDepotCount)
  {
    Weights weights = new(plan.Depots.Length, maxMedicTeamsOnDepotCount);
    Depot lastDepot = plan.AvailableMedicTeams[0].Depot;
    if (lastDepot is null)
    {
      return weights;
    }

    int availableMedicTeamIndex = 0;
    for (int depotIndex = 0; depotIndex < plan.Depots.Length; ++depotIndex)
    {
      int medicTeamIndex = 0;
      for (; plan.AvailableMedicTeams[availableMedicTeamIndex].Depot == lastDepot; ++medicTeamIndex)
      {
        weights.MedicTeamAllocations[depotIndex, medicTeamIndex] = plan.AvailableMedicTeams[availableMedicTeamIndex].Shift;
        lastDepot = plan.AvailableMedicTeams[availableMedicTeamIndex].Depot;
        ++availableMedicTeamIndex;
      }
      weights.MedicTeamsPerDepotCount[depotIndex] = medicTeamIndex;
      weights.AllAllocatedMedicTeamsCount += medicTeamIndex;
      weights.AmbulancesPerDepotCount[depotIndex] = lastDepot.Ambulances.Count;
      weights.AllAllocatedAmbulancesCount += lastDepot.Ambulances.Count;

      lastDepot = plan.AvailableMedicTeams[availableMedicTeamIndex].Depot;
    }

    return weights;
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
      AllAllocatedAmbulancesCount = this.AllAllocatedAmbulancesCount
    };
  }

  public override string ToString()
  {
    string str = "";
    for (int i = 0; i < MedicTeamAllocations.GetLength(0); ++i)
    {
      for (int j = 0; j < MedicTeamAllocations.GetLength(1); ++j)
      {
        str += $"{MedicTeamAllocations[i, j]}, ";
      }
      str.Remove(str.Length - 2, 2);
      str += "\n";
    }
    str += $"medic teams: {string.Join(", ", MedicTeamsPerDepotCount)}\n";
    str += $"medic teams count: {AllAllocatedMedicTeamsCount}\n";
    str += $"ambulances: {string.Join(", ", AmbulancesPerDepotCount)}\n";
    str += $"ambulances count: {AllAllocatedAmbulancesCount}";
    return str;
  }
}


