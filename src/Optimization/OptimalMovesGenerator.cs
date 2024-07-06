using System.Collections.Immutable;
using ESSP.DataModel;
using MyExtensions;
using Simulating;

namespace Optimizing;

public class OptimalMovesGenerator(
  World world,
  ShiftTimes shiftTimes,
  Constraints constraints,
  int movesBufferSize,
  Random random = null)
  : MoveGeneratorBase(shiftTimes, constraints, movesBufferSize)
{
  public int K { get; set; }
  public TextWriter writer;

  public ImmutableArray<Incident> Incidents { get; set; }

  private readonly Simulation _simulation = new(world, constraints);
  private readonly MoveMaker _moveMaker = new();
  private readonly Random _random = random ?? new Random();
  private readonly ISimulationState _simulationState = new SimulationState(world.Depots.Length, constraints);

  public override IEnumerable<MoveSequenceDuo> GetMoves(EmergencyServicePlan plan)
  {
    Incident lastIncident = Incidents[K];
    int startTimeIndex = GetStartTimeIndex(lastIncident);
    if (startTimeIndex == -1)
    {
      // incident happens before any shift could possible be allocated, therefore it cannot be handled. 
      // Identity move is returned, so this incident is ignored.
      // There is no branch, where this incident could possibly be handled.
      //writer.WriteLine("No way");
      Identity();
      yield return Moves;
      yield break;
    }

    // set the simulation state to state after handling all but the last incident
    int r = _simulation.Run(plan, Incidents.AsSpan().Slice(0, K - 1));
    //_simulationState.FillFrom(_simulation.State);

    //_simulation.State = _simulationState;
    int new_r = _simulation.Run(plan, Incidents.AsSpan().Slice(0, K));

    if (new_r == r + 1)
    {
      // incident is handled already. No shift needs to be prolonged or new team / ambulance need to be allocated.
      //writer.WriteLine("Identity -> r = r + 1");
      Identity();
      yield return Moves;
    }
    else
    {
      bool cantHandle = true;
      foreach (var move in OptimalMovesShiftProlonging(plan, Incidents, r))
      {
        cantHandle = false;
        yield return move;
      }

      foreach (var move in OptimalMovesTeamAllocations(plan, Incidents, startTimeIndex, r))
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

  private IEnumerable<MoveSequenceDuo> OptimalMovesTeamAllocations(EmergencyServicePlan plan, ImmutableArray<Incident> incidents, int startTimeIndex, int r)
  {
    int[] permutated = new int[plan.Assignments.Length];
    for (int i = 0; i < permutated.Length; ++i) permutated[i] = i;
    permutated.Shuffle(_random);

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

          if (plan.CanAllocateTeam(permutated[depotIndex], Constraints))
          {
            AllocateTeam(
              permutated[depotIndex],
              shift,
              plan.Assignments[permutated[depotIndex]].MedicTeams.Count
            );

            //_simulation.State = _simulationState;
            _moveMaker.ModifyMakeMove(plan, Moves.Normal);
            new_r = _simulation.Run(plan, incidents.AsSpan().Slice(0, K - 1));
            _moveMaker.ModifyMakeInverseMove(plan, Moves.Inverse);

            if (new_r == r + 1)
            {
              yield return Moves;
              goto next;
            }

            if (plan.CanAllocateAmbulance(permutated[depotIndex], Constraints))
            {
              AllocateTeamAndAmbulance(
                permutated[depotIndex],
                shift,
                plan.Assignments[permutated[depotIndex]].MedicTeams.Count
              );

              //_simulation.State = _simulationState;
              _moveMaker.ModifyMakeMove(plan, Moves.Normal);
              new_r = _simulation.Run(plan, incidents.AsSpan().Slice(0, K - 1));
              _moveMaker.ModifyMakeInverseMove(plan, Moves.Inverse);

              if (new_r == r + 1)
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

  private IEnumerable<MoveSequenceDuo> OptimalMovesShiftProlonging(EmergencyServicePlan plan, ImmutableArray<Incident> incidents, int r)
  {
    int[] permutated = new int[plan.Assignments.Length];
    for (int i = 0; i < permutated.Length; ++i) permutated[i] = i;
    permutated.Shuffle(_random);

    for (int depotIndex = 0; depotIndex < plan.Assignments.Length; ++depotIndex)
    {
      for (int teamIndex = 0; teamIndex < plan.Assignments[permutated[depotIndex]].MedicTeams.Count; ++teamIndex)
      {
        MedicTeamId teamId = new(permutated[depotIndex], teamIndex);
        int durationIndex = Array.BinarySearch(ShiftTimes.AllowedDurationsSecSorted, plan.Team(teamId).Shift.DurationSec) + 1;
        for (; durationIndex < ShiftTimes.AllowedDurationsSecSorted.Length; ++durationIndex)
        {
          Interval oldShift = plan.Assignments[permutated[depotIndex]].MedicTeams[teamIndex].Shift;
          if (plan.CanLonger(teamId, ShiftTimes))
          {
            ChangeShift(teamId, oldShift, Interval.GetByStartAndDuration(oldShift.StartSec, ShiftTimes.AllowedDurationsSecSorted[durationIndex]));

            //_simulation.State = _simulationState;
            _moveMaker.ModifyMakeMove(plan, Moves.Normal);
            int new_r = _simulation.Run(plan, incidents.AsSpan().Slice(0, K - 1));
            _moveMaker.ModifyMakeInverseMove(plan, Moves.Inverse);

            if (new_r == r + 1)
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
