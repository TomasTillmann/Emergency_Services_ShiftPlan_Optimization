using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;
using Simulating;

public interface ILoss
{
  ISimulation Simulation { get; }

  double GetCost(Weights weights);

  double Get(Weights weights, ReadOnlySpan<Incident> incidents);

  double Get(Weights weights, ImmutableArray<Incident> incidents);
}
