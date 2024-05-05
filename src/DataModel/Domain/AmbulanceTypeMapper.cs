using System.Collections.Generic;

namespace ESSP.DataModel;

public class AmbulanceTypeMapper
{
  public AmbulanceTypeModel Map(AmbulanceType type)
  {
    return new AmbulanceTypeModel
    {
      AllowedIncidentTypes = new HashSet<string>(type.AllowedIncidentTypes),
      Cost = type.Cost
    };
  }

  public AmbulanceType MapBack(AmbulanceTypeModel model)
  {
    return new AmbulanceType
    {
      AllowedIncidentTypes = new HashSet<string>(model.AllowedIncidentTypes),
      Cost = model.Cost
    };
  }
}


