using ESSP.DataModel;

namespace Optimizing;

public class OptimalMovesPlanComparer : IEqualityComparer<(int K, EmergencyServicePlan Plan)>
{
    public bool Equals((int K, EmergencyServicePlan Plan) x, (int K, EmergencyServicePlan Plan) y)
    {
        if (x.K != y.K)
        {
            return false;
        }

        if (x.Plan.AmbulancesCount != y.Plan.AmbulancesCount
            || x.Plan.MedicTeamsCount != y.Plan.MedicTeamsCount
            || x.Plan.TotalShiftDuration != y.Plan.TotalShiftDuration
           )
        {
            return false;
        }

        for (int depotIndex = 0; depotIndex < x.Plan.Assignments.Length; ++depotIndex)
        {
            if (x.Plan.Assignments[depotIndex].MedicTeams.Count != y.Plan.Assignments[depotIndex].MedicTeams.Count)
            {
                return false;
            }

            if (x.Plan.Assignments[depotIndex].Ambulances.Count != y.Plan.Assignments[depotIndex].Ambulances.Count)
            {
                return false;
            }

            for (int teamIndex = 0; teamIndex < x.Plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
            {
                if (x.Plan.Assignments[depotIndex].MedicTeams[teamIndex].Shift !=
                    y.Plan.Assignments[depotIndex].MedicTeams[teamIndex].Shift)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public int GetHashCode((int K, EmergencyServicePlan Plan) obj)
    {
        return HashCode.Combine(obj.K, obj.Plan.AmbulancesCount, obj.Plan.MedicTeamsCount, obj.Plan.TotalShiftDuration, obj.Plan.Assignments);
    }
}