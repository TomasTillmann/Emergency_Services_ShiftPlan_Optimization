namespace ESSP.DataModel;

public struct MedicTeamId
{
  public int DepotIndex { get; init; }
  public int OnDepotIndex { get; init; }

  public MedicTeamId(int depotIndex, int onDepotIndex)
  {
    DepotIndex = depotIndex;
    OnDepotIndex = onDepotIndex;
  }
}

