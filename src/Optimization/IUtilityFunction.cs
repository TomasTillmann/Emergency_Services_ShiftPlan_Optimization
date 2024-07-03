using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public interface IUtilityFunction
{
  double Get(EmergencyServicePlan plan, ReadOnlySpan<Incident> incidents);
}
