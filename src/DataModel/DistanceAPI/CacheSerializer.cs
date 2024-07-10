using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using DataModel.Interfaces;
using ESSP.DataModel;
using Newtonsoft.Json;

namespace DistanceAPI;

public class CacheSerializer
{
    private readonly World _world;
    private readonly ImmutableArray<Incident> _incidents;
    private readonly IDistanceCalculator _distanceCalculator;
    private readonly string _workingDir;

    public CacheSerializer(World world, ImmutableArray<Incident> incidents, IDistanceCalculator distanceCalculator, string workingDir = null)
    {
        _workingDir = workingDir ?? "/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/Data/";
        _world = world;
        _incidents = incidents;
        _distanceCalculator = distanceCalculator;
    }

    public void SerializeFromIncidentsToHospitals(string file)
    {
        Dictionary<(int, int), int> travelDurations = new();
        for (int i = 0; i < _incidents.Length; ++i)
        {
            for (int j = 0; j < _world.Hospitals.Length; ++j)
            {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: {i}/{_incidents.Length} incidents, {j}/{_world.Hospitals.Length} hospitals");
                int durationSec = _distanceCalculator.GetTravelDurationSec(_incidents[i].Location, _world.Hospitals[j].Location);
                travelDurations[(i, j)] = durationSec;
            }
        }
        
        WriteToFile(file, travelDurations);
    }
    
    public void SerializeFromDepotsToIncidents(string file)
    {
        Dictionary<(int, int), int> travelDurations = new();
        for (int i = 0; i < _world.Depots.Length; ++i)
        {
            for (int j = 0; j < _incidents.Length; ++j)
            {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: {i}/{_world.Depots.Length} depots, {j}/{_incidents.Length} incidents");
                int durationSec = _distanceCalculator.GetTravelDurationSec(_world.Depots[i].Location, _incidents[j].Location);
                travelDurations[(i, j)] = durationSec;
            }
        }
        
        WriteToFile(file, travelDurations);
    }
    
    public void SerializeFromHospitalsToDepots(string file)
    {
        Dictionary<(int, int), int> travelDurations = new();
        for (int i = 0; i < _world.Depots.Length; ++i)
        {
            for (int j = 0; j < _world.Hospitals.Length; ++j)
            {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: {i}/{_world.Depots.Length} depots, {j}/{_world.Hospitals.Length} hospitals");
                int durationSec = _distanceCalculator.GetTravelDurationSec(_world.Hospitals[j].Location, _world.Depots[i].Location);
                travelDurations[(i, j)] = durationSec;
            }
        }
        
        WriteToFile(file, travelDurations);
    }

    private void WriteToFile(string file, Dictionary<(int, int), int> travelDurations)
    {
        var travelDurationsStringKeys = new Dictionary<string, int>();
        foreach (var kvp in travelDurations)
        {
            string key = $"({kvp.Key.Item1}, {kvp.Key.Item2})";
            travelDurationsStringKeys[key] = kvp.Value;
        }

        string json = JsonConvert.SerializeObject(travelDurationsStringKeys, Formatting.Indented);
        File.WriteAllText(Path.Join(_workingDir, file), json);
    }
}