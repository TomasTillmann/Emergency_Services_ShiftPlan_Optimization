using System.Collections.Generic;

namespace ESSP.DataModel
{

  public readonly struct IncidentType
  {
    public string Name { get; init; }
    public Seconds MaximumResponseTime { get; init; }
    public HashSet<AmbulanceType> AllowedAmbulanceTypes { get; init; }

    public static IncidentType Default = new IncidentType("Default", 1.ToHours().ToSeconds(), new HashSet<AmbulanceType>());

    public IncidentType(string name, Seconds maximumResponseTime, HashSet<AmbulanceType> allowedAmbulanceTypes)
    {
      Name = name;
      MaximumResponseTime = maximumResponseTime;
      AllowedAmbulanceTypes = allowedAmbulanceTypes;
    }

    public override string ToString()
    {
      string allowedAmbTypes = "";
      foreach (var type in AllowedAmbulanceTypes)
      {
        allowedAmbTypes += type.ToString() + " | ";
      }

      return $"INCIDENT TYPE: {{ Name: {Name}, AllowedAmbTypes: {allowedAmbTypes} }}";
    }
  }
}
