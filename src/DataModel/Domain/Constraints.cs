namespace ESSP.DataModel;

public class Constraints
{
  public int AvailableMedicTeamsCount { get; init; }
  public int AvailableAmbulancesCount { get; init; }
  public int MaxAmbulancesOnDepotCount { get; init; }
  public int MinAmbulancesOnDepotCount { get; init; }
  public int MaxMedicTeamsOnDepotCount { get; init; }
}
