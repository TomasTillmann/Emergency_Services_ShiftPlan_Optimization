using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSP.DataModel;

public readonly struct Incidents
{
    public double Threshold { get; }

    private readonly List<Incident> _value;
    public IReadOnlyList<Incident> Value => _value;


    public Incidents(List<Incident> incidents, double threshold)
    {
        Threshold = threshold;
        _value = incidents;
    }
}
