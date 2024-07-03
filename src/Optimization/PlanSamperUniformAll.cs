using ESSP.DataModel;

namespace Optimization;

public class PlanSamperUniformAll : PlanSampler
{
  public PlanSamperUniformAll(World world, ShiftTimes shiftTimes, Constraints constraints, Random? random = null)
  : base(world, shiftTimes, constraints, random) { }

  public override EmergencyServicePlan Sample()
  {
    EmergencyServicePlan plan = EmergencyServicePlan.GetNewEmpty(World);
    int[] depotIndeces = Enumerable.Range(0, World.Depots.Length).ToArray();

    depotIndeces.Shuffle(Random);
    int[] allowedDurationsWithZero = new HashSet<int>(ShiftTimes.AllowedShiftDurationsSec) { 0 }.ToArray();

    while (plan.AvailableMedicTeams.Count > 0)
    {
      for (int depotIndex = 0; depotIndex < depotIndeces.Length; ++depotIndex)
      {
        int start = ShiftTimes.AllowedStartingTimesSecSorted[Random.Next(0, ShiftTimes.AllowedStartingTimesSecSorted.Length - 1)];
        int duration = allowedDurationsWithZero[Random.Next(0, allowedDurationsWithZero.Length - 1)];

        if (duration != 0 && plan.CanAllocateTeam(depotIndeces[depotIndex], Constraints))
        {
          plan.AllocateTeam(depotIndeces[depotIndex], Interval.GetByStartAndDuration(start, duration));
        }
        else
        {
          if (plan.AvailableMedicTeams.Count > 0)
          {
            break;
          }
        }
      }

      depotIndeces.Shuffle(Random);
    }

    while (plan.AvailableAmbulances.Count > 0)
    {
      for (int depotIndex = 0; depotIndex < depotIndeces.Length; ++depotIndex)
      {
        if (Random.Next(0, 1) == 0 && plan.CanAllocateAmbulance(depotIndeces[depotIndex], Constraints))
        {
          plan.AllocateAmbulance(depotIndeces[depotIndex]);
        }
        else
        {
          if (plan.AvailableAmbulances.Count > 0)
          {
            break;
          }
        }
      }

      depotIndeces.Shuffle(Random);
    }
  }
}

