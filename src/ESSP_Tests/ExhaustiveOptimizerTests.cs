using ESSP.DataModel;
using Optimizing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESSP_Tests
{
    public class ExhaustiveOptimizerTests : Tests
    {
        public ExhaustiveOptimizerTests()
        {
        }

        [Test]
        public void OneShiftOneIncidentTest()
        {
            IOptimizer optimizer = new ExhaustiveOptimizer(world, new Constraints
            {
                AllowedShiftDurations = new HashSet<Seconds>
                {
                    6.ToHours().ToSeconds(),
                    8.ToHours().ToSeconds(),
                    12.ToHours().ToSeconds(),
                    24.ToHours().ToSeconds(),
                },
                AllowedShiftStartingTimes = new HashSet<Seconds>
                {
                    0.ToHours().ToSeconds(),
                    6.ToHours().ToSeconds(),
                    12.ToHours().ToSeconds(),
                    18.ToHours().ToSeconds()
                }
            });

            ShiftPlan shiftPlan = testDataProvider.GetShiftPlan();
            shiftPlan.Shifts = shiftPlan.Shifts.GetRange(0, 1);

            List<SuccessRatedIncidents> incidentsSet = new List<SuccessRatedIncidents> { testDataProvider.GetIncidents(1, 24.ToHours()) };
            incidentsSet[0].Value[0].Occurence = 10_000.ToSeconds();

            IEnumerable<ShiftPlan> optimalShiftPlans = optimizer.FindOptimal(shiftPlan, incidentsSet);

            Assert.That(optimalShiftPlans.Count(), Is.EqualTo(1));
            Assert.That(optimalShiftPlans.First().Shifts.First().Work, Is.EqualTo(Interval.GetByStartAndDuration(0.ToSeconds(), 21_600.ToSeconds())));
        }

        [Test]
        public void MultipleShiftsSomeShiftsNotUsedOneIncidentTest()
        {
            IOptimizer optimizer = new ExhaustiveOptimizer(world, new Constraints
            {
                AllowedShiftDurations = new HashSet<Seconds>
                {
                    6.ToHours().ToSeconds(),
                },
                AllowedShiftStartingTimes = new HashSet<Seconds>
                {
                    0.ToHours().ToSeconds(),
                    6.ToHours().ToSeconds(),
                }
            });

            ShiftPlan shiftPlan = testDataProvider.GetShiftPlan();
            shiftPlan.Shifts = shiftPlan.Shifts.GetRange(0, 3);

            List<SuccessRatedIncidents> incidentsSet = new List<SuccessRatedIncidents> { testDataProvider.GetIncidents(1, 24.ToHours()) };
            incidentsSet[0].Value[0].Occurence = 10_000.ToSeconds();

            List<ShiftPlan> optimalShiftPlans = optimizer.FindOptimal(shiftPlan, incidentsSet).ToList();

            // Not three (0s-0s, 0s-0s, 0s-21600s), because, the third shift has ambulance type of higher cost
            Assert.That(optimalShiftPlans.Count, Is.EqualTo(2));

            Assert.That(optimalShiftPlans[0].Shifts.Select(shift => shift.Work).ToJson(),
                Is.EqualTo(
                    new List<Interval>
                    {
                        Interval.GetByStartAndDurationFromSeconds(0, 21_600),
                        Interval.GetByStartAndDurationFromSeconds(0, 0),
                        Interval.GetByStartAndDurationFromSeconds(0, 0),
                    }
                    .ToJson()
                )
            );

            Assert.That(optimalShiftPlans[1].Shifts.Select(shift => shift.Work).ToJson(),
                Is.EqualTo(
                    new List<Interval>
                    {
                        Interval.GetByStartAndDurationFromSeconds(0, 0),
                        Interval.GetByStartAndDurationFromSeconds(0, 21_600),
                        Interval.GetByStartAndDurationFromSeconds(0, 0),
                    }
                    .ToJson()
                )
            );
        }
    }
}
