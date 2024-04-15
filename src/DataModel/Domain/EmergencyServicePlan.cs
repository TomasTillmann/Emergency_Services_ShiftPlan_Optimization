using System.Collections.Immutable;

namespace ESSP.DataModel;

public class EmergencyServicePlan
{
  /// <summary>
  /// All available medic teams. How many are used is determined by <see cref="AllocatedTeamsCount"/>. 
  /// </summary>
  public ImmutableArray<MedicTeam> MedicTeams { get; init; }

  /// <summary>
  /// Determines how many teams from <see cref="MedicTeams"/> are allocated.
  /// </summary>
  public int AllocatedTeamsCount { get; set; }

  /// <summary>
  /// All available ambulances. How many are used is determined by <see cref="AllocatedAmbulancesCount"/>. 
  /// </summary>
  public ImmutableArray<Ambulance> Ambulances { get; init; }

  /// <summary>
  /// Determines how many ambulances from <see cref="MedicTeams"/> are used / allocated.
  /// </summary>
  public int AllocatedAmbulancesCount { get; set; }

  /// <summary>
  /// <returns> Total shift duration of all teams in minutes. </returns>
  /// </summary>
  public double GetShiftDurationsSum()
  {
    double sum = 0;

    for (int i = 0; i < MedicTeams.Length; ++i)
    {
      sum += MedicTeams[i].Shift.DurationSec / 60;
    }

    return sum;
  }
}
