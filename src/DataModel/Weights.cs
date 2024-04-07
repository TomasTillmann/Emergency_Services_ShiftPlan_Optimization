using System.Collections.Immutable;
using System.Text;

namespace ESSP.DataModel;

public class Weights
{
  //TODO: make init to force reusing the already allocated memory?
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

  public override string ToString()
  {
    StringBuilder str = new();
    return str.AppendJoin(',', Value).ToString();
  }
}


