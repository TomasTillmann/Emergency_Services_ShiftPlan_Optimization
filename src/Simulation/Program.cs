using System;
using System.Collections.Generic;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace Simulation;

class Statistics
{

}

class State
{
    public Seconds Time { get; set; } = 0.ToSeconds();
    public Seconds StepDuration { get; set; }
}

class Simulation
{

    public IList<Depot> Depots { get; }
    public Seconds Time => state.Time;
    public IDistanceCalculator DistanceCalculator { get; }

    private State state;
    private Statistics statistics;

    public Simulation(IList<Depot> depots, IDistanceCalculator distanceCalculator)
    {
        Depots = depots;
        DistanceCalculator = distanceCalculator;
    }

    public Statistics Run(IEnumerable<Incident> incidents)
    {
        Initialize();

        foreach (Incident incident in incidents)
        {
            UpdateSystem(incident);
            Step(incident);
        }

        return statistics;
    }

    private void Step(Incident incident)
    {

    }

    private void Initialize()
    {
        statistics = new Statistics();
        state = new State();
    }

    private void UpdateSystem(Incident incident)
    {
        UpdateState(incident);
        UpdateAmbulances();
    }

    private void UpdateState(Incident incident)
    {
        Seconds lastTime = state.Time;

        state.Time = incident.Occurence;
        state.StepDuration = state.Time - lastTime;
    }

    private void UpdateAmbulances()
    {
        foreach (Depot depot in Depots)
        {
            foreach (Ambulance ambulance in depot.Ambulances)
            {
                DistanceCalculator.GetNewLocation(ambulance, state.StepDuration);
            }
        }
    }
}
