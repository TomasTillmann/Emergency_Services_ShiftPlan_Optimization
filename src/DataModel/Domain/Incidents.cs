using System.Collections.Generic;

namespace ESSP.DataModel;

public class SuccessRatedIncidents
{
    public double SuccessRate { get; set; }

    public List<Incident> Value { get; set; }

    public SuccessRatedIncidents(List<Incident> incidents, double successRate)
    {
        SuccessRate = successRate;
        Value = incidents;
    }
}
