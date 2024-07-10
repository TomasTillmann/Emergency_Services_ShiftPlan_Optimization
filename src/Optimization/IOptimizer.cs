using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public interface IOptimizer
{
  IUtilityFunction UtilityFunction { get; set; }

  List<EmergencyServicePlan> GetBest(ImmutableArray<Incident> incidents);
}

