using System.Collections.Generic;
using System.Linq;
using DataModel.Interfaces;

namespace ESSP.DataModel;

public partial class PlannableIncident
{
    public Interval ToIncidentDrive { get; private set; }
    public Interval OnSceneDuration { get; private set; }
    public Interval ToHospitalDrive { get; private set; }
    public Interval InHospitalDelivery { get; private set; }
    public Interval ToDepotDrive { get; private set; }
    public Interval IncidentHandling => Interval.GetByStartAndEnd(ToIncidentDrive.Start, ToDepotDrive.Start);
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

        public PlannableIncident Get(Incident incident, Shift shift, Seconds time)
        {
            PlannableIncident value = new();

            Seconds reroutePenalty = shift.IsInDepot(time) ? shift.Ambulance.ReroutePenalty : 0.ToSeconds();
            Seconds toIncidentTravelDuration = distanceCalculator.GetTravelDuration(shift.Ambulance, incident, time + reroutePenalty);
            value.ToIncidentDrive = Interval.GetByStartAndDuration(time, reroutePenalty + toIncidentTravelDuration);

            value.OnSceneDuration = Interval.GetByStartAndDuration(value.ToHospitalDrive.End, incident.OnSceneDuration);

            Hospital nearestHospital = distanceCalculator.GetNearestLocatable(incident, hospitals).First();
            Seconds toHospitalTravelDuration = distanceCalculator.GetTravelDuration(incident, nearestHospital, value.OnSceneDuration.End);
            value.ToHospitalDrive = Interval.GetByStartAndDuration(value.OnSceneDuration.End, toHospitalTravelDuration);

            value.InHospitalDelivery = Interval.GetByStartAndDuration(value.ToHospitalDrive.End, incident.InHospitalDelivery);

            Seconds toDepotDriveDuration = distanceCalculator.GetTravelDuration(nearestHospital, shift.Depot, value.InHospitalDelivery.End);
            value.ToDepotDrive = Interval.GetByStartAndDuration(value.InHospitalDelivery.End, toDepotDriveDuration);

            return value;
        }
    }
}