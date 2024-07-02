namespace ESSP.DataModel;

public struct AmbulanceId
{
  public int DepotIndex { get; init; }
  public int OnDepotIndex { get; init; }

  public AmbulanceId(int depotIndex, int onDepotIndex)
  {
    DepotIndex = depotIndex;
    OnDepotIndex = onDepotIndex;
  }
}


