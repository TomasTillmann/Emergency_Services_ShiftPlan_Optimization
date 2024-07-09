using System.Collections.Immutable;
using ESSP.DataModel;

public interface IInputParametrization
{
  World GetWorld();
  ImmutableArray<Incident> GetIncidents(int count);
  ShiftTimes GetShiftTimes();
  Constraints GetConstraints();
}

