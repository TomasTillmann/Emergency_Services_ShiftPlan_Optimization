using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSP.DataModel;

public class Incidents
{
    public double Threshold { get; set; }

    public List<Incident> Value { get; set; }

    public Incidents(List<Incident> incidents, double threshold)
    {
        Threshold = threshold;
        Value = incidents;
    }
}
