using System;
using System.Collections.Generic;
using System.Linq;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace Simulation;

internal class Statistics
{
    public IList<Incident> UnhandledIncidents { get; set; } = new List<Incident>();
    public IList<Incident> HandledIncidents { get; set; } = new List<Incident>();

    public void SetUnhandled(Incident incident)
    {
        UnhandledIncidents.Add(incident);
    }

    public void SetHandled(Incident incident)
    {
        HandledIncidents.Add(incident);
    }
}

class State
{
    public Seconds CurrentTime { get; set; } = 0.ToSeconds();
    public Seconds StepDuration { get; set; }
}

class Simulation
{

    public IList<Depot> Depots { get; }
    public Seconds Time => state.CurrentTime;
    public IDistanceCalculator DistanceCalculator { get; }

    private State state;
    private Statistics statistics;
    private ShiftPlan shiftPlan;
    private ShiftEvaluator shiftEvaluator;
    private PlannableIncident.Factory plannableIncidentFactory;

    public Simulation(IList<Depot> depots, IList<Hospital> hospitals, IDistanceCalculator distanceCalculator)
    {
        Depots = depots;
        DistanceCalculator = distanceCalculator;
        plannableIncidentFactory = new PlannableIncident.Factory(distanceCalculator, hospitals);
        shiftEvaluator = new ShiftEvaluator(plannableIncidentFactory);
    }

    public Statistics Run(IEnumerable<Incident> incidents, ShiftPlan shiftPlan)
    {
        InitializeStatsStateAnd(shiftPlan);

        foreach (Incident currentIncident in incidents)
        {
            UpdateSystem(currentIncident);
            Step(currentIncident);
        }

        return statistics;
    }

    private void InitializeStatsStateAnd(ShiftPlan shiftPlan)
    {
        statistics = new Statistics();
        state = new State();
        this.shiftPlan = shiftPlan;
    }

    private void UpdateSystem(Incident incident)
    {
        UpdateState(incident);
        UpdateShiftPlan();
    }

    private void UpdateState(Incident incident)
    {
        Seconds lastTime = state.CurrentTime;

        state.CurrentTime = incident.Occurence;
        state.StepDuration = state.CurrentTime - lastTime;
    }

    private void UpdateShiftPlan()
    {



        foreach (Depot depot in Depots)
        {
            foreach (Ambulance ambulance in depot.Ambulances)
            {
                Coordinate location = DistanceCalculator.GetNewLocation(ambulance, state.StepDuration, state.CurrentTime);
                ambulance.Location = location;
            }
        }
    }

    private void Step(Incident currentIncident)
    {
        List<Shift> handlingShifts = GetHandlingShifts(currentIncident);
        if (handlingShifts.Count == 0)
        {
            statistics.SetUnhandled(currentIncident);
            return;
        }

        Shift bestShift = GetBestShift(handlingShifts, currentIncident);

        bestShift.Plan(plannableIncidentFactory.Get(currentIncident, bestShift, state.CurrentTime));

        statistics.SetHandled(currentIncident);
    }

    private List<Shift> GetHandlingShifts(Incident currentIncident)
    {
        List<Shift> handlingShifts = new();
        foreach (Shift shift in shiftPlan.Shifts)
        {
            if (shiftEvaluator.IsHandling(shift, currentIncident, state.CurrentTime))
            {
                handlingShifts.Add(shift);
            }
        }

        return handlingShifts;
    }

    private Shift GetBestShift(List<Shift> handlingShifts, Incident currentIncident)
    {
        Shift bestShift = handlingShifts.First();
        foreach (Shift shift in handlingShifts)
        {
            bestShift = shiftEvaluator.GetBetter(shift, bestShift, currentIncident, state.CurrentTime);
        }

        return bestShift;
    }
}
