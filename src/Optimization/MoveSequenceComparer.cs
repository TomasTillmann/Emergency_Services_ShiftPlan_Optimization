using System.Diagnostics.CodeAnalysis;

namespace Optimizing;

public class MoveSequenceComparer : IEqualityComparer<MoveSequence>
{
  public bool Equals(MoveSequence x, MoveSequence y)
  {
    if (x.Count != y.Count)
    {
      return false;
    }

    for (int i = 0; i < x.Count; ++i)
    {
      if (x.MovesBuffer[i] != y.MovesBuffer[i])
      {
        return false;
      }
    }

    return true;
  }

  public int GetHashCode([DisallowNull] MoveSequence obj)
  {
    return obj.GetHashCode();
  }
}


