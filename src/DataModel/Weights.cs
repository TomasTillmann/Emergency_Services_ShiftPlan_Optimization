using System;
using System.Collections.Immutable;
using System.Text;

namespace ESSP.DataModel;

public class Weights
{
  public Interval[] MedicTeamShifts { get; init; }

  /// <summary>
  /// Is size of Depots. 
  /// i-th value represents how many medic teams are allocated to i-th depot. 
  /// </summary>
  public int[] MedicTeamAllocations { get; init; }

  /// <summary>
  /// How many teams are allocated. Always has to be sum of <see cref="MedicTeamAllocations"/>.
  /// </summary>
  public int AllocatedTeamsCount { get; set; }

  /// <summary>
  /// Is size of Depots. 
  /// i-th value represents how many ambulnaces are allocated to i-th depot. 
  /// </summary>
  public int[] AmbulancesAllocations { get; init; }

  /// <summary>
  /// How many ambulances are allocated. Always has to be sum of <see cref="AmbulancesAllocations"/>.
  /// </summary>
  public int AllocatedAmbulancesCount { get; set; }

  public Weights Copy()
  {
    Interval[] value = new Interval[MedicTeamShifts.Length];
    for (int i = 0; i < MedicTeamShifts.Length; ++i)
    {
      value[i] = MedicTeamShifts[i];
    }

    int[] medicTeamAllocations = new int[MedicTeamAllocations.Length];
    for (int i = 0; i < MedicTeamAllocations.Length; ++i)
    {
      medicTeamAllocations[i] = MedicTeamAllocations[i];
    }

    int[] ambulancesAllocations = new int[AmbulancesAllocations.Length];
    for (int i = 0; i < AmbulancesAllocations.Length; ++i)
    {
      ambulancesAllocations[i] = AmbulancesAllocations[i];
    }

    return new Weights
    {
      MedicTeamShifts = value,
      MedicTeamAllocations = medicTeamAllocations
    };
  }

  public override string ToString()
  {
    StringBuilder str = new();
    str.AppendJoin(',', MedicTeamShifts).Append(Environment.NewLine).AppendJoin(',', MedicTeamAllocations);
    str.AppendJoin(',', MedicTeamShifts).Append(Environment.NewLine).AppendJoin(',', AmbulancesAllocations);
    return str.ToString();
  }
}


