using System.Collections.Immutable;
using ESSP.DataModel;

namespace Simulating;

public interface ISimulation
{
  EmergencyServicePlan EmergencyServicePlan { get; }

  World World { get; }

  int TotalIncidents { get; }

  int NotHandledIncidents { get; }

  int HandledIncidents { get; }

  double SuccessRate { get; }

  List<Incident> UnhandledIncidents { get; }

  void Run(ReadOnlySpan<Incident> incidents);
}

