using DataModel.Interfaces;
using Newtonsoft.Json;

namespace ESSP.DataModel;
public class Shift : IIdentifiable
{
    [JsonIgnore]
    private static uint IdGenerator = 1; 

    public uint Id { get; }

    public Ambulance Ambulance { get; }

    public Interval Duration { get; internal set; }

    public Shift(Ambulance ambulance, Interval duration)
    {
        Ambulance = ambulance;
        Duration = duration;

        Id = IdGenerator++;
    }
}