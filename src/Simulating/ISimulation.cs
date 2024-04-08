using System.Collections.Immutable;
using ESSP.DataModel;

namespace Simulating;

public interface ISimulation
{
  double SuccessRate { get; }

  int TotalIncidents { get; }

  int NotHandledIncidents { get; }

  int HandledIncidents { get; }

  void Run(ImmutableArray<Incident> incidents, ShiftPlan simulateOnThisShiftPlan);
}

