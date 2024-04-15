using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public interface ILoss
{
  ISimulation Simulation { get; }

  void Map(Weights weights);

  double GetEmergencyServicePlanCost(Weights weights);

  double Get(Weights weights, SuccessRatedIncidents incidents);
}
