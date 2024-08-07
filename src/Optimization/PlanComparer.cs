using ESSP.DataModel;

namespace Optimizing;

public class PlanComparer : IEqualityComparer<EmergencyServicePlan>
{
  public bool Equals(EmergencyServicePlan x, EmergencyServicePlan y)
  {
    if (x.AmbulancesCount != y.AmbulancesCount
        || x.MedicTeamsCount != y.MedicTeamsCount
        || x.TotalShiftDuration != y.TotalShiftDuration
       )
    {
      return false;
    }

    for (int depotIndex = 0; depotIndex < x.Assignments.Length; ++depotIndex)
    {
      if (x.Assignments[depotIndex].MedicTeams.Count != y.Assignments[depotIndex].MedicTeams.Count)
      {
        return false;
      }

      if (x.Assignments[depotIndex].Ambulances.Count != y.Assignments[depotIndex].Ambulances.Count)
      {
        return false;
      }

      for (int teamIndex = 0; teamIndex < x.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
      {
        if (x.Assignments[depotIndex].MedicTeams[teamIndex].Shift !=
            y.Assignments[depotIndex].MedicTeams[teamIndex].Shift)
        {
          return false;
        }
      }
    }

    return true;
  }

  public int GetHashCode(EmergencyServicePlan obj)
  {
    return HashCode.Combine(obj.AmbulancesCount, obj.MedicTeamsCount, obj.TotalShiftDuration);
  }
}

