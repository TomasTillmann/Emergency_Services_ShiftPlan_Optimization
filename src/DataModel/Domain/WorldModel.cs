using System.Collections.Generic;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public record WorldModel
{
  public List<DepotModel> Depots { get; set; }
  public List<HospitalModel> Hospitals { get; set; }
  public List<MedicTeamModel> AvailableMedicTeams { get; set; }
  public List<AmbulanceModel> AvailableAmbulances { get; set; }
  public int GoldenTimeSec { get; set; }
}


