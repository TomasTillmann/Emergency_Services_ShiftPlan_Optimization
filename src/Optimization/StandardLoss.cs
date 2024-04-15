using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public class StandardLoss : Loss
{
  private readonly double _maxEmergencyServicePlanCost;

  public StandardLoss(Simulation simulation, ShiftTimes shiftTimes)
  : base(simulation)
  {
    _maxEmergencyServicePlanCost = simulation.World.AvailableMedicTeams.Length * shiftTimes.MaxDurationSec / 60;
  }

  public override double Get(Weights weights, SuccessRatedIncidents incidents)
  {
    RunSimulation(weights, incidents);

    double cost = Simulation.EmergencyServicePlan.GetShiftDurationsSum() / _maxEmergencyServicePlanCost;
    double handled = Simulation.SuccessRate;
    double thresh = incidents.SuccessRate;

    double eps = 0.001;
    double handledPart = (thresh + eps) * handled;
    double costPart = (1 - thresh + eps) * cost;
    double loss = costPart - handledPart;

    return loss;
  }
}
