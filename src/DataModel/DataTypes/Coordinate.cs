using System;

namespace ESSP.DataModel
{
  public readonly struct Coordinate : IComparable<Coordinate>
  {
    public int XMet { get; init; }
    public int YMet { get; init; }

    /// lexicographical comparision - first by X and than by Y
    public int CompareTo(Coordinate other)
    {
      int comp = XMet.CompareTo(other.XMet);
      if (comp == 0)
      {
        return YMet.CompareTo(other.YMet);
      }
      else
      {
        return comp;
      }
    }
  }
}

