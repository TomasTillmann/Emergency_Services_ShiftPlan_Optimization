using System.Collections.Immutable;
using ESSP.DataModel;

namespace Optimizing;

public class Weights
{
  public Interval[] Value { get; set; }

  public Weights Copy()
  {
    Interval[] value = new Interval[Value.Length];
    for (int i = 0; i < Value.Length; ++i)
    {
      value[i] = Value[i];
    }

    return new Weights
    {
      Value = value
    };
  }
}


