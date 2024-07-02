using System;
using System.Collections.Immutable;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public partial class PlannableIncident
{
  public Incident Incident { get; set; }
  public int AmbulanceIndex { get; set; }
  public Hospital NearestHospital { get; set; }
  public Interval ToIncidentDrive { get; set; }
  public Interval OnSceneDuration { get; set; }
  public Interval ToHospitalDrive { get; set; }
  public Interval InHospitalDelivery { get; set; }
  public Interval ToDepotDrive { get; set; }

  private PlannableIncident() { }

  // copy constructor
  public PlannableIncident(PlannableIncident toCopy)
  {
    this.Incident = toCopy.Incident;
    this.NearestHospital = toCopy.NearestHospital;
    this.AmbulanceIndex = toCopy.AmbulanceIndex;
    this.ToIncidentDrive = toCopy.ToIncidentDrive;
    this.OnSceneDuration = toCopy.OnSceneDuration;
    this.ToHospitalDrive = toCopy.ToHospitalDrive;
    this.InHospitalDelivery = toCopy.InHospitalDelivery;
    this.ToDepotDrive = toCopy.ToDepotDrive;
  }

  public void FillFrom(PlannableIncident source)
  {
    this.Incident = source.Incident;
    this.NearestHospital = source.NearestHospital;
    this.AmbulanceIndex = source.AmbulanceIndex;
    this.ToIncidentDrive = source.ToIncidentDrive;
    this.OnSceneDuration = source.OnSceneDuration;
    this.ToHospitalDrive = source.ToHospitalDrive;
    this.InHospitalDelivery = source.InHospitalDelivery;
    this.ToDepotDrive = source.ToDepotDrive;
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
    public PlannableIncident WorkingInstance { get; }
    public SimulationState State { get; set; }
    public EmergencyServicePlan Plan { get; set; }

    private DistanceCalculator distanceCalculator;
    private ImmutableArray<Hospital> hospitals;

    public Factory(DistanceCalculator distanceCalculator, ImmutableArray<Hospital> hospitals)
    {
      this.distanceCalculator = distanceCalculator;
      this.hospitals = hospitals;

      WorkingInstance = new PlannableIncident();
    }

    public static readonly PlannableIncident Empty = new PlannableIncident();

    /// Creates new instance of the shared instance.
    public PlannableIncident GetCopy(in Incident incident, MedicTeamId teamId, Depot homeDepot)
    {
      return new PlannableIncident(Get(teamId, in incident));
    }

    /// In order not to allocate memory for PlannableIncidentOpt all the time, factory always returns shared instance.
    /// Only this factory can modify its attributes and client has only read only access.
    /// This is helpful for intermmidiate calculations.
    /// If you would like to get instance, where factory would not change its attributes, use <see cref="GetCopy(in Incident, MedicTeam)"/>.
    public PlannableIncident Get(MedicTeamId teamId, in Incident incident)
    {
      WorkingInstance.Incident = incident;

      WorkingInstance.NearestHospital = distanceCalculator.GetNearestHospital(incident.Location);

      WorkingInstance.ToIncidentDrive = GetToIncidentDriveAndAmbIndex(incident.OccurenceSec, incident.Location, teamId, out int ambIndex);

      WorkingInstance.AmbulanceIndex = ambIndex;

      WorkingInstance.OnSceneDuration = Interval.GetByStartAndDuration(WorkingInstance.ToIncidentDrive.EndSec, incident.OnSceneDurationSec);

      int toHospitalTravelDurationSec = distanceCalculator.GetTravelDurationSec(incident.Location, WorkingInstance.NearestHospital.Location);
      WorkingInstance.ToHospitalDrive = Interval.GetByStartAndDuration(WorkingInstance.OnSceneDuration.EndSec, toHospitalTravelDurationSec);

      WorkingInstance.InHospitalDelivery = Interval.GetByStartAndDuration(WorkingInstance.ToHospitalDrive.EndSec, incident.InHospitalDeliverySec);

      int toDepotDriveDurationSec = distanceCalculator.GetTravelDurationSec(WorkingInstance.NearestHospital.Location, Plan.Depots[teamId.DepotIndex].Location);
      WorkingInstance.ToDepotDrive = Interval.GetByStartAndDuration(WorkingInstance.InHospitalDelivery.EndSec, toDepotDriveDurationSec);

      return WorkingInstance;
    }

    public Interval GetToIncidentDrive(int incidentOccurenceTimeSec, Coordinate incidentLocation, MedicTeamId teamId)
    {
      return GetToIncidentDriveAndAmbIndex(incidentOccurenceTimeSec, incidentLocation, teamId, out _);
    }

    private Interval GetToIncidentDriveAndAmbIndex(int incidentOccurenceTimeSec, Coordinate incidentLocation, MedicTeamId teamId, out int ambIndex)
    {
      CalculateStartTimeAndAmbulanceStartingLocation(teamId, incidentOccurenceTimeSec, out int startTimeSec, out Coordinate ambStartLoc, out ambIndex);

      int toIncidentTravelDurationSec = distanceCalculator.GetTravelDurationSec(ambStartLoc, incidentLocation);

      return Interval.GetByStartAndDuration(startTimeSec, toIncidentTravelDurationSec);
    }

    private void CalculateStartTimeAndAmbulanceStartingLocation(MedicTeamId teamId, int incidentOccurenceTimeSec, out int startTimeSec, out Coordinate ambStartLoc, out int ambIndex)
    {
      MedicTeamState teamState = State.TeamState(teamId);
      MedicTeam team = Plan.Team(teamId);

      int firstPossibleStartTimeSec = Math.Max(incidentOccurenceTimeSec, team.Shift.StartSec);
      Depot homeDepot = Plan.Depots[teamId.DepotIndex];

      // Is in depot.
      if (teamState.IsInDepot(firstPossibleStartTimeSec))
      {
        int whenAmbulanceFree = int.MaxValue;
        int possibleStartingTime = int.MaxValue;
        ambIndex = -1; // will always be reassigned for the earliest one

        // Finds ambulance which is available the earliest, and sets startTimeSec to first possible starting time.
        // That cannot be time before the shift starts, and it's either the time the shift starts or when the earliest ambulance is available.
        for (int i = 0; i < homeDepot.Ambulances.Count; ++i)
        {
          if (whenAmbulanceFree > homeDepot.Ambulances[i].WhenFreeSec)
          {
            whenAmbulanceFree = homeDepot.Ambulances[i].WhenFreeSec;
            possibleStartingTime = Math.Max(firstPossibleStartTimeSec, whenAmbulanceFree);
            ambIndex = i;
          }
        }

        startTimeSec = possibleStartingTime;
        ambStartLoc = homeDepot.Location;
        return;
      }

      PlannableIncident currentlyHandlingIncident = teamState.LastPlannedIncident;
      Coordinate hospitalLocation = currentlyHandlingIncident.NearestHospital.Location;

      // Medic team will use the same ambulance.
      ambIndex = currentlyHandlingIncident.AmbulanceIndex;

      // Driving to depot from hospital after handling incident.
      if (currentlyHandlingIncident.ToDepotDrive.IsInInterval(firstPossibleStartTimeSec))
      {
        startTimeSec = firstPossibleStartTimeSec + Ambulance.ReroutePenaltySec;

        int durationDrivingSec = incidentOccurenceTimeSec - currentlyHandlingIncident.ToDepotDrive.StartSec;
        ambStartLoc = distanceCalculator.GetNewLocation(hospitalLocation, homeDepot.Location, durationDrivingSec, firstPossibleStartTimeSec);

        return;
      }

      // At hospital after handling incident.
      startTimeSec = currentlyHandlingIncident.InHospitalDelivery.EndSec + Ambulance.ReroutePenaltySec;
      ambStartLoc = hospitalLocation;
    }
  }
}
