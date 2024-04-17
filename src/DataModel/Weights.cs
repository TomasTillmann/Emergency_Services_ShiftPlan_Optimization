using System;
using System.Collections.Immutable;
using System.Text;

namespace ESSP.DataModel;

public class Weights
{
  public Interval[,] MedicTeamAllocations { get; init; }

  /// <summary>
  /// Is size of Depots. 
  /// i-th value represents how many medic teams are allocated to i-th depot. 
  /// Sum per i-th values equals <see cref="AllocatedMedicTeamsCount"/>.
  /// </summary>
  public int[] MedicTeamsPerDepotCount { get; init; }

  /// <summary>
  /// How many teams are allocated. Always has to be sum of <see cref="MedicTeamAllocations"/>.
  /// </summary>
  public int AllocatedMedicTeamsCount { get; set; }

  /// <summary>
  /// Is size of Depots. 
  /// i-th value represents how many ambulnaces are allocated to i-th depot. 
  /// Sum per i-th values equals <see cref="AllocatedAmbulancesCount"/>.
  /// </summary>
  public int[] AmbulancesPerDepotCount { get; init; }

  /// <summary>
  /// How many ambulances are allocated. Always has to be sum of <see cref="AmbulancesPerDepotCount"/>.
  /// </summary>
  public int AllocatedAmbulancesCount { get; set; }

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

    plan.AllocatedAmbulancesCount = AllocatedAmbulancesCount;
    plan.AllocatedMedicTeamsCount = AllocatedMedicTeamsCount;
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
      AllocatedMedicTeamsCount = this.AllocatedMedicTeamsCount,
      AmbulancesPerDepotCount = ambulancesPerDepotCount,
      AllocatedAmbulancesCount = this.AllocatedAmbulancesCount
    };
  }
}


