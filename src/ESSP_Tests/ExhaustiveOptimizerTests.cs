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
                AllowedShiftDurations = new List<Seconds>
                {
                    6.ToHours().ToSeconds(),
                    8.ToHours().ToSeconds(),
                    12.ToHours().ToSeconds(),
                    24.ToHours().ToSeconds(),
                },
                AllowedShiftStartingTimes = new List<Seconds>
                {
                    0.ToHours().ToSeconds(),
                    6.ToHours().ToSeconds(),
                    12.ToHours().ToSeconds(),
                    18.ToHours().ToSeconds()
                }
            });

            ShiftPlan shiftPlan = testDataProvider.GetShiftPlan();
            shiftPlan.Shifts = shiftPlan.Shifts.GetRange(0, 1);

            List<IncidentsSet> incidentsSet = new List<IncidentsSet> { testDataProvider.GetIncidents(1, 24.ToHours()) };
            incidentsSet[0].Value[0].Occurence = 10_000.ToSeconds();

            IEnumerable<ShiftPlan> optimalShiftPlans = optimizer.FindOptimal(shiftPlan, incidentsSet);

            Assert.That(optimalShiftPlans.Count(), Is.EqualTo(1));
            Assert.That(optimalShiftPlans.First().Shifts.First().Work, Is.EqualTo(Interval.GetByStartAndDuration(0.ToSeconds(), 21_600.ToSeconds())));
        }

        [Test]
        public void MultipleShiftsOneIncidentTest()
        {

        }
    }
}
