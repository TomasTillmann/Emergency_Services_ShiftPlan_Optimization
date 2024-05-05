using System.Collections.Generic;

namespace ESSP.DataModel;

/// World
public record WorldModel
{
  public List<DepotModel> Depots { get; set; }
  public List<HospitalModel> Hospitals { get; set; }
  public List<MedicTeam> AvailableMedicTeams { get; set; }
  public List<Ambulance> AvailableAmbulances { get; set; }
  public List<AmbulanceTypeModel> AvailableAmbulanceTypes { get; set; }
  public int GoldenTimeSec { get; set; }
}


