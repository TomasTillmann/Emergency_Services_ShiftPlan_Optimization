namespace ESSP.DataModel;

public class MedicTeam
{
  public Interval Shift { get; set; } = Interval.GetByStartAndDuration(0, 0);
  
  public MedicTeam() { }

  public MedicTeam(Interval shift)
  {
    Shift = shift;
  }
}

