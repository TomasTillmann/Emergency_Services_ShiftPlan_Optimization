using System.Collections.Immutable;
using ESSP.DataModel;
using Optimizing;

public interface ILoss
{
  public double Get(Weights weights, ImmutableArray<SuccessRatedIncidents> incidentsSet);
}
