using System;
using System.Collections.Immutable;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public partial class PlannableIncident
{
  public Incident Incident { get; private set; }
  public Hospital NearestHospital { get; private set; }
  public Interval ToIncidentDrive { get; private set; }
  public Interval OnSceneDuration { get; private set; }
  public Interval ToHospitalDrive { get; private set; }
  public Interval InHospitalDelivery { get; private set; }
  public Interval ToDepotDrive { get; private set; }

  public PlannableIncident() { }

  // copy constructor
  private PlannableIncident(PlannableIncident toCopy)
  {
    this.Incident = toCopy.Incident;
    this.NearestHospital = toCopy.NearestHospital;
    this.ToIncidentDrive = toCopy.ToIncidentDrive;
    this.OnSceneDuration = toCopy.OnSceneDuration;
    this.ToHospitalDrive = toCopy.ToHospitalDrive;
    this.InHospitalDelivery = toCopy.InHospitalDelivery;
    this.ToDepotDrive = toCopy.ToDepotDrive;
  }

  public Interval IncidentHandling => Interval.GetByStartAndEnd(ToIncidentDrive.StartSec, ToDepotDrive.StartSec);
  public Interval WholeInterval => Interval.GetByStartAndEnd(ToIncidentDrive.StartSec, ToDepotDrive.EndSec);
}


public partial class PlannableIncident
{
  public class Factory
  {
    private DistanceCalculator distanceCalculator;
    private ImmutableArray<Hospital> hospitals;

    private PlannableIncident _instance;

    public Factory(DistanceCalculator distanceCalculator, ImmutableArray<Hospital> hospitals)
    {
      this.distanceCalculator = distanceCalculator;
      this.hospitals = hospitals;

      _instance = new PlannableIncident();
    }

    /// Creates new instance of the shared instance.
    public PlannableIncident GetCopy(in Incident incident, Shift shift)
    {
      return new PlannableIncident(Get(in incident, shift));
    }

    /// In order not to allocate memory for PlannableIncidentOpt all the time, factory always returns shared instance.
    /// Only this factory can modify its attributes and client has only read only access.
    /// This is helpful for intermmidiate calculations.
    /// If you would like to get instance, where factory would not change its attributes, use <see cref="GetCopy(in Incident, Shift)"/>.
    public PlannableIncident Get(in Incident incident, Shift shift)
    {
      //TODO: Cache?

      // by ref?
      _instance.Incident = incident;

      _instance.NearestHospital = distanceCalculator.GetNearestHospital(incident.Location);

      _instance.ToIncidentDrive = GetToIncidentDrive(incident.OccurenceSec, incident.Location, shift);

      _instance.OnSceneDuration = Interval.GetByStartAndDuration(_instance.ToIncidentDrive.EndSec, incident.OnSceneDurationSec);

      int toHospitalTravelDurationSec = distanceCalculator.GetTravelDurationSec(incident.Location, _instance.NearestHospital.Location);
      _instance.ToHospitalDrive = Interval.GetByStartAndDuration(_instance.OnSceneDuration.EndSec, toHospitalTravelDurationSec);

      _instance.InHospitalDelivery = Interval.GetByStartAndDuration(_instance.ToHospitalDrive.EndSec, incident.InHospitalDeliverySec);

      int toDepotDriveDurationSec = distanceCalculator.GetTravelDurationSec(_instance.NearestHospital.Location, shift.Depot.Location);
      _instance.ToDepotDrive = Interval.GetByStartAndDuration(_instance.InHospitalDelivery.EndSec, toDepotDriveDurationSec);

      return _instance;
    }

    public Interval GetToIncidentDrive(int incidentOccurenceTimeSec, Coordinate incidentLocation, Shift shift)
    {
      CalculateStartTimeAndAmbulanceStartingLocation(shift, incidentOccurenceTimeSec, out int startTimeSec, out Coordinate ambStartLoc);

      int toIncidentTravelDurationSec = distanceCalculator.GetTravelDurationSec(ambStartLoc, incidentLocation);

      return Interval.GetByStartAndDuration(startTimeSec, toIncidentTravelDurationSec);
    }

    private void CalculateStartTimeAndAmbulanceStartingLocation(Shift shift, int incidentOccurenceTimeSec, out int startTimeSec, out Coordinate ambStartLoc)
    {
      int firstPossibleStartTimeSec = Math.Max(incidentOccurenceTimeSec, shift.Work.StartSec);
      if (shift.IsInDepot(firstPossibleStartTimeSec))
      {
        startTimeSec = firstPossibleStartTimeSec;
        ambStartLoc = shift.Depot.Location;
        return;
      }

      PlannableIncident currentlyHandlingIncident = shift.GetCurrentlyHandlingIncident();
      Coordinate hospitalLocation = currentlyHandlingIncident.NearestHospital.Location;

      if (currentlyHandlingIncident.ToDepotDrive.IsInInterval(firstPossibleStartTimeSec))
      {
        startTimeSec = firstPossibleStartTimeSec + Ambulance.ReroutePenaltySec;

        int durationDrivingSec = incidentOccurenceTimeSec - currentlyHandlingIncident.ToDepotDrive.StartSec;
        ambStartLoc = distanceCalculator.GetNewLocation(hospitalLocation, shift.Depot.Location, durationDrivingSec, firstPossibleStartTimeSec);

        return;
      }

      startTimeSec = currentlyHandlingIncident.InHospitalDelivery.EndSec + Ambulance.ReroutePenaltySec;
      ambStartLoc = hospitalLocation;
    }
  }
}
