namespace ESSP.DataModel;

public class PlannableIncidentOpt
{
  public IncidentOpt Incident { get; init; }
  public HospitalOpt NearestHospital { get; set; }
  public IntervalOpt ToIncidentDrive { get; set; }
  public IntervalOpt OnSceneDuration { get; set; }
  public IntervalOpt ToHospitalDrive { get; set; }
  public IntervalOpt InHospitalDelivery { get; set; }
  public IntervalOpt ToDepotDrive { get; set; }

  public IntervalOpt IncidentHandling => IntervalOpt.GetByStartAndEnd(ToIncidentDrive.StartSec, ToDepotDrive.StartSec);
  public IntervalOpt WholeInterval => IntervalOpt.GetByStartAndEnd(ToIncidentDrive.StartSec, ToDepotDrive.EndSec);
}

