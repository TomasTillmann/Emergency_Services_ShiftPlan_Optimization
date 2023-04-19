using System;
using System.Collections.Generic;
using System.Linq;
using DataModel.Interfaces;
using ESSP.DataModel;

namespace Simulating;

public class Statistics
{
    public IList<Incident> UnhandledIncidents { get; set; } = new List<Incident>();
    public IList<Incident> HandledIncidents { get; set; } = new List<Incident>();

    public IList<Incident> AllIncidents { get; set; }

    public double Threshold => AllIncidents.Count / HandledIncidents.Count;

    public Statistics(IList<Incident> allIncidents)
    {
        AllIncidents = allIncidents;
    }

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

public class Simulation
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

    public Statistics Run(IList<Incident> incidents, ShiftPlan shiftPlan)
    {
        Initialization(shiftPlan, incidents);

        foreach (Incident currentIncident in incidents)
        {
            UpdateSystem(currentIncident);
            Step(currentIncident);
        }

        return statistics;
    }

    private void Initialization(ShiftPlan shiftPlan, IList<Incident> allIncidents)
    {
        statistics = new Statistics(allIncidents);
        state = new State();
        this.shiftPlan = shiftPlan;
    }

    private void UpdateSystem(Incident incident)
    {
        UpdateState(incident);
    }

    private void UpdateState(Incident incident)
    {
        Seconds lastTime = state.CurrentTime;

        state.CurrentTime = incident.Occurence;
        state.StepDuration = state.CurrentTime - lastTime;
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
