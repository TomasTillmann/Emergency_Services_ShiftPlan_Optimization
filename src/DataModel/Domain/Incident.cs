using DataModel.Interfaces;
using Newtonsoft.Json;

namespace ESSP.DataModel;
public class Incident : ILocatable
{
    public Coordinate Location { get; }

    [JsonIgnore]
    public Seconds Occurence { get; }

    [JsonIgnore]
    public Seconds OnSceneDuration { get; }

    [JsonIgnore]
    public Seconds InHospitalDelivery { get; }

    public IncidentType Type { get; }

    public Incident(Coordinate coordinate, Seconds occurence, Seconds onSceneDuration, Seconds inHospitalDelivery, IncidentType type)
    {
        Location = coordinate;
        Occurence = occurence;
        OnSceneDuration = onSceneDuration;
        InHospitalDelivery = inHospitalDelivery;
        Type = type;
    }

    public override string ToString()
    {
        return $"INCIDENT: {{ {Location}, OCCURENCE: {Occurence}, {Type} }}";
    }
}