using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using DataModel.Interfaces;
using ESSP.DataModel;
using Newtonsoft.Json;

namespace DistanceAPI;

public class CacheDeserializer
{
    private readonly string _incidentsToHospitals;
    private readonly string _depotsToIncidents;
    private readonly string _hospitalsToDepots;
    private readonly World _world;
    private readonly ImmutableArray<Incident> _incidents;
    private readonly string _workDir;
    
    public CacheDeserializer(World world,
        ImmutableArray<Incident> incidents,
        string incidentsToHospitals,
        string depotsToIncidents,
        string hospitalsToDepots,
        string workDir = null
    )
    {
        _world = world;
        _incidents = incidents;
        _incidentsToHospitals = incidentsToHospitals;
        _depotsToIncidents = depotsToIncidents;
        _hospitalsToDepots = hospitalsToDepots;
        _workDir = workDir ?? "/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/Data/";;
    }
    
    public void InitNearestHospitalCache(Dictionary<Coordinate, Hospital> nearestHospital)
    {
        var cache = GetCache(_incidentsToHospitals);
        for (int i = 0; i < _incidents.Length; ++i)
        {
            Hospital nearest = default(Hospital);
            int duration = int.MaxValue;
            for (int j = 0; j < _world.Hospitals.Length; ++j)
            {
                var time = cache[(i, j)];
                if (time < duration)
                {
                    nearest = _world.Hospitals[j];
                    duration = time;
                }
            }
            
            nearestHospital[_incidents[i].Location] = nearest;
        }
    }
    
    public void InitTravelDurationsCache(Dictionary<(Coordinate, Coordinate), int> travelDurations)
    {
        var cache1 = GetCache(_depotsToIncidents);
        var cache2 = GetCache(_incidentsToHospitals);
        var cache3 = GetCache(_hospitalsToDepots);

        for (int i = 0; i < _world.Depots.Length; ++i)
        {
            for (int j = 0; j < _incidents.Length; ++j)
            {
                try
                {
                    travelDurations[(_world.Depots[i].Location, _incidents[j].Location)] = cache1[(i,j)];
                }
                catch {
                    Console.WriteLine($"{i} depot and {j} incident not present");
                }
            }
        }

        for (int i = 0; i < _incidents.Length; ++i)
        {
            for (int j = 0; j < _world.Hospitals.Length; ++j)
            {
                try
                {
                    travelDurations[(_incidents[i].Location, _world.Hospitals[j].Location)] = cache2[(i, j)];
                }
                catch
                {
                    Console.WriteLine($"{i} incident and {j} hospital not present");
                }
            }
        }
        
        
        for (int i = 0; i < _world.Hospitals.Length; ++i)
        {
            for (int j = 0; j < _world.Depots.Length; ++j)
            {
                try
                {
                    travelDurations[(_world.Hospitals[i].Location, _world.Depots[j].Location)] = cache3[(i, j)];
                }
                catch
                {
                    Console.WriteLine($"{i} hospital and {j} depot not present");
                }
            }
        }
    }
    
    private Dictionary<(int, int), int> GetCache(string file)
    {
        string json = File.ReadAllText(Path.Join(_workDir, file));
        var travelDurationsStringKeys = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
        var travelDurations = new Dictionary<(int, int), int>();

        foreach (var kvp in travelDurationsStringKeys)
        {
            var keyParts = kvp.Key.Trim('(', ')').Split(',');
            int index1 = int.Parse(keyParts[0]);
            int index2 = int.Parse(keyParts[1]);
            
            travelDurations[(index1, index2)] = kvp.Value;
            travelDurations[(index2, index1)] = kvp.Value;
        }

        return travelDurations;
    }
}