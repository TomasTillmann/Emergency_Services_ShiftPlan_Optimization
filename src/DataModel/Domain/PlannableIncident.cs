using System;
using System.Collections.Immutable;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public partial class PlannableIncident
{
  public Incident Incident { get; private set; }
  public int OnDepotAmbulanceIndex { get; private set; }
  public Hospital NearestHospital { get; private set; }
  public Interval ToIncidentDrive { get; private set; }
  public Interval OnSceneDuration { get; private set; }
  public Interval ToHospitalDrive { get; private set; }
  public Interval InHospitalDelivery { get; private set; }
  public Interval ToDepotDrive { get; private set; }

  private PlannableIncident() { }

  // copy constructor
  private PlannableIncident(PlannableIncident toCopy)
  {
    this.Incident = toCopy.Incident;
    this.NearestHospital = toCopy.NearestHospital;
    this.OnDepotAmbulanceIndex = toCopy.OnDepotAmbulanceIndex;
    this.ToIncidentDrive = toCopy.ToIncidentDrive;
    this.OnSceneDuration = toCopy.OnSceneDuration;
    this.ToHospitalDrive = toCopy.ToHospitalDrive;
    this.InHospitalDelivery = toCopy.InHospitalDelivery;
    this.ToDepotDrive = toCopy.ToDepotDrive;
  }

  public Interval IncidentHandling => Interval.GetByStartAndEnd(ToIncidentDrive.StartSec, ToDepotDrive.StartSec);
  public Interval WholeInterval => Interval.GetByStartAndEnd(ToIncidentDrive.StartSec, ToDepotDrive.EndSec);

  public override string ToString()
  {
    return $"({Incident.OccurenceSec}), {ToIncidentDrive}, {OnSceneDuration}, {ToHospitalDrive}, {InHospitalDelivery}, {ToDepotDrive}";
  }
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
    public PlannableIncident GetCopy(in Incident incident, MedicTeam shift)
    {
      return new PlannableIncident(Get(in incident, shift));
    }

    /// In order not to allocate memory for PlannableIncidentOpt all the time, factory always returns shared instance.
    /// Only this factory can modify its attributes and client has only read only access.
    /// This is helpful for intermmidiate calculations.
    /// If you would like to get instance, where factory would not change its attributes, use <see cref="GetCopy(in Incident, MedicTeam)"/>.
    public PlannableIncident Get(in Incident incident, MedicTeam shift)
    {
      // by ref?
      _instance.Incident = incident;

      _instance.NearestHospital = distanceCalculator.GetNearestHospital(incident.Location);

      _instance.ToIncidentDrive = GetToIncidentDriveAndAmbIndex(incident.OccurenceSec, incident.Location, shift, incident.Type, out int ambIndex);

      _instance.OnDepotAmbulanceIndex = ambIndex;

      _instance.OnSceneDuration = Interval.GetByStartAndDuration(_instance.ToIncidentDrive.EndSec, incident.OnSceneDurationSec);

      int toHospitalTravelDurationSec = distanceCalculator.GetTravelDurationSec(incident.Location, _instance.NearestHospital.Location);
      _instance.ToHospitalDrive = Interval.GetByStartAndDuration(_instance.OnSceneDuration.EndSec, toHospitalTravelDurationSec);

      _instance.InHospitalDelivery = Interval.GetByStartAndDuration(_instance.ToHospitalDrive.EndSec, incident.InHospitalDeliverySec);

      int toDepotDriveDurationSec = distanceCalculator.GetTravelDurationSec(_instance.NearestHospital.Location, shift.Depot.Location);
      _instance.ToDepotDrive = Interval.GetByStartAndDuration(_instance.InHospitalDelivery.EndSec, toDepotDriveDurationSec);

      return _instance;
    }

    public Interval GetToIncidentDrive(int incidentOccurenceTimeSec, Coordinate incidentLocation, MedicTeam medicTeam)
    {
      return GetToIncidentDriveAndAmbIndex(incidentOccurenceTimeSec, incidentLocation, medicTeam, null, out _);
    }

    private Interval GetToIncidentDriveAndAmbIndex(int incidentOccurenceTimeSec, Coordinate incidentLocation, MedicTeam medicTeam, string incidentType, out int ambIndex)
    {
      CalculateStartTimeAndAmbulanceStartingLocation(medicTeam, incidentOccurenceTimeSec, incidentType, out int startTimeSec, out Coordinate ambStartLoc, out ambIndex);

      int toIncidentTravelDurationSec = distanceCalculator.GetTravelDurationSec(ambStartLoc, incidentLocation);

      return Interval.GetByStartAndDuration(startTimeSec, toIncidentTravelDurationSec);
    }

    private void CalculateStartTimeAndAmbulanceStartingLocation(MedicTeam medicTeam, int incidentOccurenceTimeSec, string incidentType, out int startTimeSec, out Coordinate ambStartLoc, out int ambIndex)
    {
      int firstPossibleStartTimeSec = Math.Max(incidentOccurenceTimeSec, medicTeam.Shift.StartSec);

      // Is in depot.
      if (medicTeam.IsInDepot(firstPossibleStartTimeSec))
      {
        int whenAmbulanceFree = int.MaxValue;
        int possibleStartingTime = int.MaxValue;
        ambIndex = -1; // will always be reassigned for the earliest one if exists, otherwise remains -1, meaning no ambulance could be allocated 

        // Finds ambulance which has compatible type and is available the earliest, and sets startTimeSec to first possible starting time.
        // That cannot be time before the shift starts, and it's either the time the shift starts or when the earliest ambulance is available.
        for (int i = 0; i < medicTeam.Depot.Ambulances.Count; ++i)
        {
          if (medicTeam.Depot.Ambulances[i].Type.AllowedIncidentTypes.Contains(incidentType)
              && whenAmbulanceFree > medicTeam.Depot.Ambulances[i].WhenFreeSec)
          {
            whenAmbulanceFree = medicTeam.Depot.Ambulances[i].WhenFreeSec;
            possibleStartingTime = Math.Max(firstPossibleStartTimeSec, whenAmbulanceFree);
            ambIndex = i;
          }
        }

        startTimeSec = possibleStartingTime;
        ambStartLoc = medicTeam.Depot.Location;
        return;
      }

      PlannableIncident currentlyHandlingIncident = medicTeam.GetCurrentlyHandlingIncident();
      Coordinate hospitalLocation = currentlyHandlingIncident.NearestHospital.Location;

      // Medic team will use the same ambulance.
      ambIndex = currentlyHandlingIncident.OnDepotAmbulanceIndex;

      // Driving to depot from hospital after handling incident.
      if (currentlyHandlingIncident.ToDepotDrive.IsInInterval(firstPossibleStartTimeSec))
      {
        startTimeSec = firstPossibleStartTimeSec + Ambulance.ReroutePenaltySec;

        int durationDrivingSec = incidentOccurenceTimeSec - currentlyHandlingIncident.ToDepotDrive.StartSec;
        ambStartLoc = distanceCalculator.GetNewLocation(hospitalLocation, medicTeam.Depot.Location, durationDrivingSec, firstPossibleStartTimeSec);

        return;
      }

      // At hospital after handling incident.
      startTimeSec = currentlyHandlingIncident.InHospitalDelivery.EndSec + Ambulance.ReroutePenaltySec;
      ambStartLoc = hospitalLocation;
    }
  }
}
