using System;
using System.Collections.Generic;
using System.Linq;

namespace ESSP.DataModel
{
    public readonly struct IncidentType
    {
        public string Name { get; }
        public Seconds MaximumResponseTime { get; }
        public HashSet<AmbulanceType> AllowedAmbulanceTypes { get; }

        public IncidentType(string name, Seconds maximumResponseTime, HashSet<AmbulanceType> allowedAmbulanceTypes)
        {
            Name=name;
            MaximumResponseTime=maximumResponseTime;
            AllowedAmbulanceTypes = allowedAmbulanceTypes;
        }

        public override string ToString()
        {
            string allowedAmbTypes = "";
            foreach(var type in AllowedAmbulanceTypes)
            {
                allowedAmbTypes+= type.ToString() + " | ";
            }

            return $"INCIDENT TYPE: {{ Name: {Name}, AllowedAmbTypes: {allowedAmbTypes} }}"; 
        }
    }
}