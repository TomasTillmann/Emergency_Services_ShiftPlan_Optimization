using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSP.DataModel;

public readonly struct Incidents
{
    public double Threshold { get; }

    public List<Incident> Value { get; }

    public Incidents(List<Incident> incidents, double threshold)
    {
        Threshold = threshold;
        Value = incidents;
    }
}
