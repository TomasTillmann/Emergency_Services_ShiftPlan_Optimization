using System;

namespace ESSP.DataModel
{
  public readonly struct IncidentTypeOpt
  {
    public string Name { get; init; }
    public int MaximumResponseTimeSec { get; init; }

    public override int GetHashCode()
    {
      return MaximumResponseTimeSec.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
      throw new NotImplementedException("SLOW");
    }
  }
}

