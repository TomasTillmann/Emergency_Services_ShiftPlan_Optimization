using System.Collections.Immutable;
using ESSP.DataModel;

public interface IInputParametrization
{
  World GetWorld();
  ImmutableArray<Incident> GetIncidents();
  ShiftTimes GetShiftTimes();
  Constraints GetConstraints();
}

