using System;
using System.Collections.Immutable;

namespace ESSP.DataModel;

public class EmergencyServicePlan
{
  public ImmutableArray<MedicTeam> AvailableMedicTeams { get; set; }
  public int AllocatedMedicTeamsCount { get; set; }

  public ImmutableArray<Ambulance> AvailableAmbulances { get; set; }
  public int AllocatedAmbulancesCount { get; set; }

  public ImmutableArray<Depot> Depots { get; init; }

  /// <summary>
  /// <returns> Total shift duration of all teams in minutes. </returns>
  /// </summary>
  public double GetShiftDurationsSum()
  {
    double sum = 0;

    for (int i = 0; i < AllocatedMedicTeamsCount; ++i)
    {
      sum += AvailableMedicTeams[i].Shift.DurationSec / 60;
    }

    return sum;
  }

  public double GetExhaustionSum()
  {
    double exhaustionSum = 0;

    for (int i = 0; i < AllocatedMedicTeamsCount; ++i)
    {
      exhaustionSum += AvailableMedicTeams[i].TimeActiveSec / AvailableMedicTeams[i].Shift.DurationSec;
    }

    return exhaustionSum;
  }

  public double GetCost()
  {
    return GetShiftDurationsSum() + AllocatedAmbulancesCount;
  }
}
