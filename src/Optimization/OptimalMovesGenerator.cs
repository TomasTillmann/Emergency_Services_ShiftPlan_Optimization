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
  public ImmutableArray<Incident> Incidents { get; set; }
  private readonly Simulation _simulation = new(world, constraints);
  private readonly MoveMaker _moveMaker = new();
  private readonly Random _random = random ?? new Random();

  public override IEnumerable<MoveSequenceDuo> GetMoves(EmergencyServicePlan plan)
  {
    int r = _simulation.Run(plan, Incidents.AsSpan().Slice(0, K - 1));
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
    
    int new_r = _simulation.Run(plan, Incidents.AsSpan().Slice(0, K));
    if (r + 1 == new_r)
    {
      // incident is handled already. No shift needs to be prolonged or new team / ambulance need to be allocated.
      Identity();
      yield return Moves;
    }
    else
    {
      foreach (var move in OptimalMovesTeamAllocations(plan, Incidents, r, startTimeIndex))
      {
        yield return move;
      }

      foreach (var move in OptimalMovesShiftProlonging(plan, Incidents, r))
      {
        yield return move;
      }
    }
  }

  private IEnumerable<MoveSequenceDuo> OptimalMovesTeamAllocations(EmergencyServicePlan plan, ImmutableArray<Incident> incidents, int r, int startTimeIndex)
  {
    int new_r;
    int[] permutated = new int[plan.Assignments.Length];
    for (int i = 0; i < permutated.Length; ++i) permutated[i] = i;
    permutated.Shuffle(_random);
    
    for (int depotIndex = 0; depotIndex < plan.Assignments.Length; ++depotIndex)
    {
      for (; startTimeIndex >= 0; --startTimeIndex)
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

            _moveMaker.ModifyMakeMove(plan, Moves.Normal);
            new_r = _simulation.Run(plan, incidents.AsSpan()[..K]);
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
              
              _moveMaker.ModifyMakeMove(plan, Moves.Normal);
              new_r = _simulation.Run(plan, incidents.AsSpan()[..K]);
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

            _moveMaker.ModifyMakeMove(plan, Moves.Normal);
            int new_r = _simulation.Run(plan, incidents.AsSpan()[..K]);
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
