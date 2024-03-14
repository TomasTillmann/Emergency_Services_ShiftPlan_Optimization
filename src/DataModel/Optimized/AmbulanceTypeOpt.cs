namespace ESSP.DataModel;

public readonly struct AmbulanceTypeOpt
{
  public string Name { get; init; }
  public int Cost { get; init; }

  public override int GetHashCode()
  {
    return Cost.GetHashCode();
  }
}

