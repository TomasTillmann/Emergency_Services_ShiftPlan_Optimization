namespace ESSP.DataModel;

public class AmbulanceState
{
  public int WhenFreeSec { get; set; }

  public void Clear()
  {
    WhenFreeSec = 0;
  }
}


