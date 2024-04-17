using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public interface ILoss
{
  ISimulation Simulation { get; }

  double GetEmergencyServicePlanCost(Weights weights);

  double Get(Weights weights, SuccessRatedIncidents incidents);
}
