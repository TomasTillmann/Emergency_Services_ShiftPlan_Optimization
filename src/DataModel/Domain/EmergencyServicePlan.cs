using System.Collections.Immutable;

namespace ESSP.DataModel;

public class EmergencyServicePlan
{
  public ImmutableArray<MedicTeam> Teams { get; init; }
  public int AllocatedTeamsCount { get; set; }

  public ImmutableArray<Ambulance> Ambulances { get; init; }

  /// <summary>
  /// <returns> Total shift duration of all teams in minutes. </returns>
  /// </summary>
  public double GetShiftDurationsSum()
  {
    double sum = 0;

    for (int i = 0; i < Teams.Length; ++i)
    {
      sum += Teams[i].Shift.DurationSec / 60;
    }

    return sum;
  }
}
