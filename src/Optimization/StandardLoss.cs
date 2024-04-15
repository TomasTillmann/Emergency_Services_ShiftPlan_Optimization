using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public class StandardLoss : Loss
{
  private readonly double _maxEmergencyServicePlanShiftDuration;
  private readonly double _availableAmbulancesTotalCount;

  public StandardLoss(Simulation simulation, ShiftTimes shiftTimes)
  : base(simulation)
  {
    _maxEmergencyServicePlanShiftDuration = simulation.World.AvailableMedicTeams.Length * shiftTimes.MaxDurationSec / 60;
    _availableAmbulancesTotalCount = simulation.World.AvailableAmbulances.Length;
  }

  public override double Get(Weights weights, SuccessRatedIncidents incidents)
  {
    RunSimulation(weights, incidents);

    double shiftDurations = Simulation.EmergencyServicePlan.GetShiftDurationsSum() / _maxEmergencyServicePlanShiftDuration;
    double ambulancesCount = Simulation.EmergencyServicePlan.AllocatedAmbulancesCount / _availableAmbulancesTotalCount;
    double planCost = (shiftDurations + ambulancesCount) / 2;
    double handled = Simulation.SuccessRate;
    double thresh = incidents.SuccessRate;

    double eps = 0.001;
    double handledPart = (thresh + eps) * handled;
    double costPart = (1 - thresh + eps) * planCost;
    double loss = costPart - handledPart;

    return loss;
  }
}
