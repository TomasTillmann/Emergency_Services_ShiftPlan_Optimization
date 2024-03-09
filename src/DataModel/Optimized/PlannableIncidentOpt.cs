using System;
using System.Collections.Immutable;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public partial class PlannableIncidentOpt
{
  public IncidentOpt Incident { get; private set; }
  public HospitalOpt NearestHospital { get; private set; }
  public IntervalOpt ToIncidentDrive { get; private set; }
  public IntervalOpt OnSceneDuration { get; private set; }
  public IntervalOpt ToHospitalDrive { get; private set; }
  public IntervalOpt InHospitalDelivery { get; private set; }
  public IntervalOpt ToDepotDrive { get; private set; }

  public PlannableIncidentOpt() { }

  // copy constructor
  private PlannableIncidentOpt(PlannableIncidentOpt toCopy)
  {
    this.Incident = toCopy.Incident;
    this.NearestHospital = toCopy.NearestHospital;
    this.ToIncidentDrive = toCopy.ToIncidentDrive;
    this.OnSceneDuration = toCopy.OnSceneDuration;
    this.ToHospitalDrive = toCopy.ToHospitalDrive;
    this.InHospitalDelivery = toCopy.InHospitalDelivery;
    this.ToDepotDrive = toCopy.ToDepotDrive;
  }

  public IntervalOpt IncidentHandling => IntervalOpt.GetByStartAndEnd(ToIncidentDrive.StartSec, ToDepotDrive.StartSec);
  public IntervalOpt WholeInterval => IntervalOpt.GetByStartAndEnd(ToIncidentDrive.StartSec, ToDepotDrive.EndSec);
}


public partial class PlannableIncidentOpt
{
  public class Factory
  {
    private DistanceCalculatorOpt distanceCalculator;
    private ImmutableArray<HospitalOpt> hospitals;

    private PlannableIncidentOpt _instance;

    public Factory(DistanceCalculatorOpt distanceCalculator, ImmutableArray<HospitalOpt> hospitals)
    {
      this.distanceCalculator = distanceCalculator;
      this.hospitals = hospitals;

      _instance = new PlannableIncidentOpt();
    }

    /// Creates new instance of the shared instance.
    public PlannableIncidentOpt GetCopy(in IncidentOpt incident, ShiftOpt shift)
    {
      return new PlannableIncidentOpt(Get(in incident, shift));
    }

    /// In order not to allocate memory for PlannableIncidentOpt all the time, factory always returns shared instance.
    /// Only this factory can modify its attributes and client has only read only access.
    /// This is helpful for intermmidiate calculations.
    /// If you would like to get instance, where factory would not change its attributes, use <see cref="GetCopy(in IncidentOpt, ShiftOpt)"/>.
    public PlannableIncidentOpt Get(in IncidentOpt incident, ShiftOpt shift)
    {
      //TODO: Cache?

      // by ref?
      _instance.Incident = incident;

      _instance.NearestHospital = distanceCalculator.GetNearestHospital(incident.Location);

      _instance.ToIncidentDrive = GetToIncidentDrive(incident.OccurenceSec, incident.Location, shift);

      _instance.OnSceneDuration = IntervalOpt.GetByStartAndDuration(_instance.ToIncidentDrive.EndSec, incident.OnSceneDurationSec);

      int toHospitalTravelDurationSec = distanceCalculator.GetTravelDurationSec(incident.Location, _instance.NearestHospital.Location);
      _instance.ToHospitalDrive = IntervalOpt.GetByStartAndDuration(_instance.OnSceneDuration.EndSec, toHospitalTravelDurationSec);

      _instance.InHospitalDelivery = IntervalOpt.GetByStartAndDuration(_instance.ToHospitalDrive.EndSec, incident.InHospitalDeliverySec);

      int toDepotDriveDurationSec = distanceCalculator.GetTravelDurationSec(_instance.NearestHospital.Location, shift.Depot.Location);
      _instance.ToDepotDrive = IntervalOpt.GetByStartAndDuration(_instance.InHospitalDelivery.EndSec, toDepotDriveDurationSec);

      return _instance;
    }

    public IntervalOpt GetToIncidentDrive(int incidentOccurenceTimeSec, CoordinateOpt incidentLocation, ShiftOpt shift)
    {
      CalculateStartTimeAndAmbulanceStartingLocation(shift, incidentOccurenceTimeSec, out int startTimeSec, out CoordinateOpt ambStartLoc);

      int toIncidentTravelDurationSec = distanceCalculator.GetTravelDurationSec(ambStartLoc, incidentLocation);

      return IntervalOpt.GetByStartAndDuration(startTimeSec, toIncidentTravelDurationSec);
    }

    private void CalculateStartTimeAndAmbulanceStartingLocation(ShiftOpt shift, int incidentOccurenceTimeSec, out int startTimeSec, out CoordinateOpt ambStartLoc)
    {
      int firstPossibleStartTimeSec = Math.Max(incidentOccurenceTimeSec, shift.Work.StartSec);
      if (shift.IsInDepot(firstPossibleStartTimeSec))
      {
        startTimeSec = firstPossibleStartTimeSec;
        ambStartLoc = shift.Depot.Location;
        return;
      }

      PlannableIncidentOpt currentlyHandlingIncident = shift.GetCurrentlyHandlingIncident();
      CoordinateOpt hospitalLocation = currentlyHandlingIncident.NearestHospital.Location;

      if (currentlyHandlingIncident.ToDepotDrive.IsInInterval(firstPossibleStartTimeSec))
      {
        startTimeSec = firstPossibleStartTimeSec + AmbulanceOpt.ReroutePenaltySec;

        int durationDrivingSec = incidentOccurenceTimeSec - currentlyHandlingIncident.ToDepotDrive.StartSec;
        ambStartLoc = distanceCalculator.GetNewLocation(hospitalLocation, shift.Depot.Location, durationDrivingSec, firstPossibleStartTimeSec);

        return;
      }

      startTimeSec = currentlyHandlingIncident.InHospitalDelivery.EndSec + AmbulanceOpt.ReroutePenaltySec;
      ambStartLoc = hospitalLocation;
    }
  }
}
