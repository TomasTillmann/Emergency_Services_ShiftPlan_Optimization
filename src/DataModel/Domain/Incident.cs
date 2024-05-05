namespace ESSP.DataModel;

// NOTE: In majority of cases, structs should not be this big.
// But in this case, it is desirable to make <see cref="Incident"/> a struct,
// since it is used only as an input to simulation in an immutable array.
// Making it a struct, the continues iterations are much faster thanks to caching - principle of locality. 
// That's because struct are value types, and are stored directly in the array.
// Instead of just a reference to heap, which would be the case if <see cref="Incident"/> would be a class.
// Using the <see langword="in"/> keyword when passing as method parameter with combination of <see cref="Incident"/>
// beining a readonly struct, this struct is never copied and is always passed by reference.
// The <see langword="in"/> also prevents the compiler from making defensive copies.
// This approach hence guarantees very fast iteration of the incidents.
// One must just be generally careful when dealing with <see cref="Incident"/>, for example, do not ever pass it by value etc ...
public readonly struct Incident
{
  public Coordinate Location { get; init; }
  public int OccurenceSec { get; init; }
  public int OnSceneDurationSec { get; init; }
  public int InHospitalDeliverySec { get; init; }
  public string Type { get; init; }

  public override string ToString()
  {
    return $"({Location}, {OccurenceSec}, {Type})";
  }
}

