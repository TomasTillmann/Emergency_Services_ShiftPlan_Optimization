using DataModel.Interfaces;
using Model.Extensions;

namespace ESSP.DataModel;
public class Incident : ILocatable
{
    public Coordinate Location { get; set; }

    public Seconds Occurence { get; set; }

    public Seconds OnSceneDuration { get; set; }

    public Seconds InHospitalDelivery { get; set; }

    public IncidentType Type { get; set; }

    public Incident(Coordinate location, Seconds occurence, Seconds onSceneDuration, Seconds inHospitalDelivery, IncidentType type)
    {
        Location = location;
        Occurence = occurence;
        OnSceneDuration = onSceneDuration;
        InHospitalDelivery = inHospitalDelivery;
        Type = type;
    }

    public override string ToString()
    {
        return $"Location: {Location}, Occurence: {Occurence}, AllowedAmbTypes: {Type.AllowedAmbulanceTypes.Visualize()}";
    }
}