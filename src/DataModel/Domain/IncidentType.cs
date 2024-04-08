namespace ESSP.DataModel
{
  public readonly struct IncidentType
  {
    public string Name { get; init; }
    public int MaximumResponseTimeSec { get; init; }

    public override string ToString()
    {
      return $"({Name})";
    }
  }
}

