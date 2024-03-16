using System.Collections.Generic;

namespace ESSP.DataModel;

public class IncTypeToAllowedAmbTypesTable
{
  private readonly Dictionary<string, HashSet<string>> _table;

  public IncTypeToAllowedAmbTypesTable(Dictionary<string, HashSet<string>> table)
  {
    _table = table;
  }

  public bool IsAllowed(IncidentType incType, AmbulanceType ambType)
  {
    // open world principle
    if (!_table.ContainsKey(incType.Name))
    {
      return true;
    }

    return _table[incType.Name].Contains(ambType.Name);
  }

  public Dictionary<string, HashSet<string>> GetTable()
  {
    return new(_table);
  }
}
