using System.Collections.Generic;
using System.Linq;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public partial class PlannableIncident
{
    public Incident Incident { get; private set; }
    public Hospital Hospital { get; private set; }
    public Interval ToIncidentDrive { get; private set; }
    public Interval OnSceneDuration { get; private set; }
    public Interval ToHospitalDrive { get; private set; }
    public Interval InHospitalDelivery { get; private set; }
    public Interval ToDepotDrive { get; private set; }
    public Interval IncidentHandling => Interval.GetByStartAndEnd(ToIncidentDrive.Start, ToDepotDrive.Start);
    public Interval WholeInterval => Interval.GetByStartAndEnd(ToIncidentDrive.Start, ToDepotDrive.End);

    public PlannableIncident(Incident incident)
    {
        Incident = incident;
    }
}

partial class PlannableIncident
{
    public class Factory
    {
        private IDistanceCalculator distanceCalculator;
        private IList<Hospital> hospitals;

        public Factory(IDistanceCalculator distanceCalculator, IList<Hospital> hospitals)
        {
            this.distanceCalculator = distanceCalculator;
            this.hospitals = hospitals;
        }

        public PlannableIncident Get(Incident incident, Shift shift, Seconds currentTime)
        {
            PlannableIncident value = new(incident);

            value.ToIncidentDrive = GetToIncidentDrive(incident, shift, currentTime);

            value.OnSceneDuration = Interval.GetByStartAndDuration(value.ToHospitalDrive.End, incident.OnSceneDuration);

            Hospital nearestHospital = distanceCalculator.GetNearestLocatable(incident, hospitals).First();
            Seconds toHospitalTravelDuration = distanceCalculator.GetTravelDuration(incident, nearestHospital, value.OnSceneDuration.End);
            value.ToHospitalDrive = Interval.GetByStartAndDuration(value.OnSceneDuration.End, toHospitalTravelDuration);

            value.InHospitalDelivery = Interval.GetByStartAndDuration(value.ToHospitalDrive.End, incident.InHospitalDelivery);

            Seconds toDepotDriveDuration = distanceCalculator.GetTravelDuration(nearestHospital, shift.Depot, value.InHospitalDelivery.End);
            value.ToDepotDrive = Interval.GetByStartAndDuration(value.InHospitalDelivery.End, toDepotDriveDuration);

            return value;
        }

        private Interval GetToIncidentDrive(Incident incident, Shift shift, Seconds currentTime)
        {
            PlannableIncident value = new(incident);

            Seconds reroutePenalty = 0.ToSeconds();
            Coordinate ambulanceLocation = shift.Depot.Location;

            // handling incident or driving to depot
            if (!shift.IsInDepot(currentTime))
            {
                PlannableIncident currentlyHandlingIncident = shift.PlannedIncident(currentTime);
                Coordinate hospitalLocation = currentlyHandlingIncident.Hospital.Location;

                ambulanceLocation = hospitalLocation;

                // interrupting midway
                if (currentlyHandlingIncident.ToDepotDrive.Contains(currentTime))
                {
                    reroutePenalty = shift.Ambulance.ReroutePenalty;
                    Seconds durationDriving = currentTime - currentlyHandlingIncident.ToDepotDrive.Start;

                    ambulanceLocation = distanceCalculator.GetNewLocation(hospitalLocation, shift.Depot.Location, durationDriving, currentTime);
                }
            }

            Seconds toIncidentTravelDuration = distanceCalculator.GetTravelDuration(ambulanceLocation, incident.Location, currentTime + reroutePenalty);
            value.ToIncidentDrive = Interval.GetByStartAndDuration(currentTime, reroutePenalty + toIncidentTravelDuration);
        }
    }
}