using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSP.DataModel;

public class IncidentsSet
{
    public double Threshold { get; set; }

    public List<Incident> Value { get; set; }

    public IncidentsSet(List<Incident> incidents, double threshold)
    {
        Threshold = threshold;
        Value = incidents;
    }
}
