using System.Collections.Immutable;
using DataModel.Interfaces;
using ESSP.DataModel;
using MyExtensions;
using Simulating;

namespace Optimizing;

/// <summary>
/// Implementation of optimal moves generator.
/// </summary>
public class OptimalMovesGenerator(
  World world,
  ShiftTimes shiftTimes,
  Constraints constraints,
  IDistanceCalculator distanceCalculator,
  int movesBufferSize,
  Random random = null)
  : MoveGeneratorBase(shiftTimes, constraints, movesBufferSize)
{
  public int K { get; set; }
  public ImmutableArray<Incident> Incidents { get; set; }

  private readonly Simulation _simulation = new(world, constraints, distanceCalculator);
  private readonly MoveMaker _moveMaker = new();
  private readonly Random _random = random ?? new Random();

  /// <inheritdoc />
  public override IEnumerable<MoveSequenceDuo> GetMoves(EmergencyServicePlan plan)
  {
    Incident lastIncident = Incidents[K];
    int startTimeIndex = GetStartTimeIndex(lastIncident);
    if (startTimeIndex == -1)
    {
      // incident happens before any shift could possible be allocated, therefore it cannot be handled. 
      // Identity move is returned, so this incident is ignored.
      // There is no branch, where this incident could possibly be handled.
      Identity();
      yield return Moves;
      yield break;
    }

    ISimulationState simulationState = new SimulationState(world.Depots.Length, constraints);

    int r = _simulation.Run(plan, Incidents.AsSpan().Slice(0, K - 1));
    simulationState.FillFrom(_simulation.State);

    _simulation.State.FillFrom(simulationState);
    int new_r = _simulation.Run(plan, Incidents.AsSpan().Slice(K - 1, 1), resetState: false);

    if (new_r == 1)
    {
      // incident is handled already. No shift needs to be prolonged or new team / ambulance need to be allocated.
      Identity();
      yield return Moves;
    }
    else
    {
      bool cantHandle = true;
      foreach (var move in OptimalMovesShiftProlonging(plan, Incidents, simulationState))
      {
        cantHandle = false;
        yield return move;
      }

      foreach (var move in OptimalMovesTeamAllocations(plan, Incidents, startTimeIndex, simulationState))
      {
        cantHandle = false;
        yield return move;
      }

      if (cantHandle)
      {
        Identity();
        yield return Moves;
      }
    }
  }

  private IEnumerable<MoveSequenceDuo> OptimalMovesTeamAllocations(EmergencyServicePlan plan, ImmutableArray<Incident> incidents, int startTimeIndex, ISimulationState simulationState)
  {
    int new_r;
    for (int depotIndex = 0; depotIndex < plan.Assignments.Length; ++depotIndex)
    {
      for (int currStartTimeIndex = startTimeIndex; currStartTimeIndex >= 0; --currStartTimeIndex)
      {
        for (int durationIndex = 0; durationIndex < ShiftTimes.AllowedDurationsSecSorted.Length; ++durationIndex)
        {
          Interval shift = Interval.GetByStartAndDuration(
            ShiftTimes.AllowedStartingTimesSecSorted[startTimeIndex],
            ShiftTimes.AllowedDurationsSecSorted[durationIndex]
          );

          if (plan.CanAllocateTeam(depotIndex, Constraints))
          {
            AllocateTeam(
              depotIndex,
              shift,
              plan.Assignments[depotIndex].MedicTeams.Count
            );

            _simulation.State.FillFrom(simulationState);
            _moveMaker.ModifyMakeMove(plan, Moves.Normal);
            new_r = _simulation.Run(plan, incidents.AsSpan().Slice(K - 1, 1), resetState: false);
            _moveMaker.ModifyMakeInverseMove(plan, Moves.Inverse);

            if (new_r == 1)
            {
              yield return Moves;
              goto next;
            }

            if (plan.CanAllocateAmbulance(depotIndex, Constraints))
            {
              AllocateTeamAndAmbulance(
                depotIndex,
                shift,
                plan.Assignments[depotIndex].MedicTeams.Count
              );

              _simulation.State.FillFrom(simulationState);
              _moveMaker.ModifyMakeMove(plan, Moves.Normal);
              new_r = _simulation.Run(plan, incidents.AsSpan().Slice(K - 1, 1), resetState: false);
              _moveMaker.ModifyMakeInverseMove(plan, Moves.Inverse);

              if (new_r == 1)
              {
                yield return Moves;
                goto next;
              }
            }
          }
        }
      }
    next: { }
    }
  }

  private IEnumerable<MoveSequenceDuo> OptimalMovesShiftProlonging(EmergencyServicePlan plan, ImmutableArray<Incident> incidents, ISimulationState simulationState)
  {
    for (int depotIndex = 0; depotIndex < plan.Assignments.Length; ++depotIndex)
    {
      for (int teamIndex = 0; teamIndex < plan.Assignments[depotIndex].MedicTeams.Count; ++teamIndex)
      {
        MedicTeamId teamId = new(depotIndex, teamIndex);
        int durationIndex = Array.BinarySearch(ShiftTimes.AllowedDurationsSecSorted, plan.Team(teamId).Shift.DurationSec) + 1;
        for (; durationIndex < ShiftTimes.AllowedDurationsSecSorted.Length; ++durationIndex)
        {
          Interval oldShift = plan.Assignments[depotIndex].MedicTeams[teamIndex].Shift;
          if (plan.CanLonger(teamId, ShiftTimes))
          {
            ChangeShift(teamId, oldShift, Interval.GetByStartAndDuration(oldShift.StartSec, ShiftTimes.AllowedDurationsSecSorted[durationIndex]));

            _simulation.State.FillFrom(simulationState);
            _moveMaker.ModifyMakeMove(plan, Moves.Normal);
            int new_r = _simulation.Run(plan, incidents.AsSpan().Slice(K - 1, 1), resetState: false);
            _moveMaker.ModifyMakeInverseMove(plan, Moves.Inverse);

            if (new_r == 1)
            {
              yield return Moves;
              goto next;
            }
          }
        }

      next: { }
      }
    }
  }

  private int GetStartTimeIndex(Incident lastIncident)
  {
    int index = Array.BinarySearch(ShiftTimes.AllowedStartingTimesSecSorted, lastIncident.OccurenceSec);
    return ~index - 1;
  }
}
