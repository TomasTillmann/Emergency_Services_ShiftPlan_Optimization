using System;
using System.Collections.Generic;
using System.Linq;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public partial class PlannableIncident
{
    public Incident Incident { get; private set; }
    public Hospital NearestHospital { get; set; }
    public Interval ToIncidentDrive { get; set; }
    public Interval OnSceneDuration { get; set; }
    public Interval ToHospitalDrive { get; set; }
    public Interval InHospitalDelivery { get; set; }
    public Interval ToDepotDrive { get; set; }
    public Interval IncidentHandling => Interval.GetByStartAndEnd(ToIncidentDrive.Start, ToDepotDrive.Start);
    public Interval WholeInterval => Interval.GetByStartAndEnd(ToIncidentDrive.Start, ToDepotDrive.End);

    public PlannableIncident(Incident incident)
    {
        Incident = incident;
    }

    public override string ToString()
    {
        return
            $"{Incident}, " +
            $"Hospital: {NearestHospital.Location}, " +
            $"ToIncidentDrive: {ToIncidentDrive.Start}, " +
            $"OnSceneDuration: {OnSceneDuration.Start}, ToHospitalDrive: {ToHospitalDrive.Start}, " +
            $"InHospitalDelivery: {InHospitalDelivery.Start}, " +
            $"ToDepotDrive: {ToDepotDrive.Start}, " +
            $"InDepot: {ToDepotDrive.End}";
    }
}

partial class PlannableIncident
{
    public class Factory
    {
        private IDistanceCalculator distanceCalculator;
        private IReadOnlyList<Hospital> hospitals;

        public Factory(IDistanceCalculator distanceCalculator, IReadOnlyList<Hospital> hospitals)
        {
            this.distanceCalculator = distanceCalculator;
            this.hospitals = hospitals;
        }

        public PlannableIncident Get(Incident incident, Shift shift)
        {
            PlannableIncident value = new(incident);
            value.NearestHospital = distanceCalculator.GetNearestLocatable(incident, hospitals).First();

            value.ToIncidentDrive = GetToIncidentDrive(incident, shift);

            value.OnSceneDuration = Interval.GetByStartAndDuration(value.ToIncidentDrive.End, incident.OnSceneDuration);

            Seconds toHospitalTravelDuration = distanceCalculator.GetTravelDuration(incident, value.NearestHospital, value.OnSceneDuration.End);
            value.ToHospitalDrive = Interval.GetByStartAndDuration(value.OnSceneDuration.End, toHospitalTravelDuration);

            value.InHospitalDelivery = Interval.GetByStartAndDuration(value.ToHospitalDrive.End, incident.InHospitalDelivery);

            Seconds toDepotDriveDuration = distanceCalculator.GetTravelDuration(value.NearestHospital, shift.Depot, value.InHospitalDelivery.End);
            value.ToDepotDrive = Interval.GetByStartAndDuration(value.InHospitalDelivery.End, toDepotDriveDuration);

            return value;
        }

        private Interval GetToIncidentDrive(Incident incident, Shift shift)
        {
            CalculateStartTimeAndAmbulanceStartingLocation(shift, incident.Occurence, out Seconds startTimeToIncidentDrive, out Coordinate ambulanceLocation);

            Seconds toIncidentTravelDuration = distanceCalculator.GetTravelDuration(ambulanceLocation, incident.Location, startTimeToIncidentDrive);

            return Interval.GetByStartAndDuration(startTimeToIncidentDrive, toIncidentTravelDuration);
        }

        private void CalculateStartTimeAndAmbulanceStartingLocation(Shift shift, Seconds incidentOccurenceTime, out Seconds startTime, out Coordinate ambulanceLocation)
        {
            Seconds firstPossibleStartTime = Math.Max(incidentOccurenceTime.Value, shift.Work.Start.Value).ToSeconds();
            if (shift.IsInDepot(firstPossibleStartTime))
            {
                startTime = firstPossibleStartTime;
                ambulanceLocation = shift.Depot.Location;
                return;
            }

            PlannableIncident currentlyHandlingIncident = shift.PlannedIncidents.Last();
            Coordinate hospitalLocation = currentlyHandlingIncident.NearestHospital.Location;

            if (currentlyHandlingIncident.ToDepotDrive.IsInInterval(firstPossibleStartTime))
            {
                startTime = firstPossibleStartTime + shift.Ambulance.ReroutePenalty;

                Seconds durationDriving = incidentOccurenceTime - currentlyHandlingIncident.ToDepotDrive.Start;
                ambulanceLocation = distanceCalculator.GetNewLocation(hospitalLocation, shift.Depot.Location, durationDriving, firstPossibleStartTime);

                return;
            }

            startTime = currentlyHandlingIncident.InHospitalDelivery.End + shift.Ambulance.ReroutePenalty;
            ambulanceLocation = hospitalLocation;
        }
    }
}