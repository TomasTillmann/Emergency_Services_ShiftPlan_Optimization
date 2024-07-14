using System.Reflection.Emit;
using ESSP.DataModel;
using MyExtensions;

namespace Optimizing;

/// <summary>
/// Samples random plan satisfying constraints in uniform fashion, allcoating teams and vehicles given by a percentage.
/// </summary>
public class PlanSamplerUniform : PlanSampler
{
  public double Percentage { get; set; }

  /// <param name="percentage">Ratio of how many teams and vehicles to allocate from available.</param>
  public PlanSamplerUniform(World world, ShiftTimes shiftTimes, Constraints constraints, double percentage = 1.0,
    Random? random = null)
    : base(world, shiftTimes, constraints, random)
  {
    if (Percentage is < 0 or > 1) throw new ArgumentException();
    Percentage = percentage;
  }

  /// <inheritdoc /> 
  public override EmergencyServicePlan Sample()
  {
    EmergencyServicePlan plan = EmergencyServicePlan.GetNewEmpty(World);
    double teamsThreshold = plan.AvailableMedicTeams.Count * Percentage;
    double ambulancesThreshold = plan.AvailableAmbulances.Count * Percentage;
    int[] depotIndeces = Enumerable.Range(0, World.Depots.Length).ToArray();
    int[] allowedDurationsWithZero = new HashSet<int>(ShiftTimes.AllowedShiftDurationsSec) { 0 }.ToArray();

    while (true)
    {
      depotIndeces.Shuffle(Random);
      for (int depotIndex = 0; depotIndex < depotIndeces.Length; ++depotIndex)
      {
        int start = ShiftTimes.AllowedStartingTimesSecSorted[Random.Next(0, ShiftTimes.AllowedStartingTimesSecSorted.Length - 1)];
        int duration = allowedDurationsWithZero[Random.Next(0, allowedDurationsWithZero.Length - 1)];

        if (duration != 0 && plan.CanAllocateTeam(depotIndeces[depotIndex], Constraints))
        {
          plan.AllocateTeam(depotIndeces[depotIndex], Interval.GetByStartAndDuration(start, duration));
          if (plan.MedicTeamsCount > teamsThreshold || plan.AvailableMedicTeams.Count == 0)
          {
            goto ambulances;
          }
        }
      }
    }

    ambulances:
    while (plan.AvailableAmbulances.Count > 0)
    {
      depotIndeces.Shuffle(Random);
      for (int depotIndex = 0; depotIndex < depotIndeces.Length; ++depotIndex)
      {
        if (Random.Next(0, 1) == 0 && plan.CanAllocateAmbulance(depotIndeces[depotIndex], Constraints))
        {
          plan.AllocateAmbulance(depotIndeces[depotIndex]);
          if (plan.AmbulancesCount > ambulancesThreshold || plan.AvailableAmbulances.Count == 0)
          {
            goto r;
          }
        }
      }
    }

    r: return plan;
  }
}

