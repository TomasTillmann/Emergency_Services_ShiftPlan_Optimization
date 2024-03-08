namespace ESSP.DataModel;

public readonly struct ShiftPlanOpt
{
  public Shift[] Shifts { get; init; }

  public ShiftPlanOpt(int shiftsLength)
  {
    Shifts = new Shift[shiftsLength];
  }
}

