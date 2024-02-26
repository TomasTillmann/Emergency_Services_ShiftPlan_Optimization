using System.Collections.Generic;

namespace ESSP.DataModel;

public class SuccessRatedIncidents
{
    /// <summary>
    /// Minimum success rate we would like to accomplish for the Incidents.
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// Incidents, we run simulation on.
    /// </summary>
    public List<Incident> Value { get; set; }

    public SuccessRatedIncidents(List<Incident> incidents, double successRate)
    {
        SuccessRate = successRate;
        Value = incidents;
    }
}
