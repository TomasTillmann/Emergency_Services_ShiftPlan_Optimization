using System.Collections.Immutable;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public class WorldOpt
{
  public ImmutableArray<DepotOpt> Depots { get; set; }
  public ImmutableArray<HospitalOpt> Hospitals { get; set; }
  public DistanceCalculatorOpt DistanceCalculator { get; set; }
  public IncTypeToAllowedAmbTypesTable IncTypeToAllowedAmbTypesTable { get; set; }
}
