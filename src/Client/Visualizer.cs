using ESSP.DataModel;
using Model.Extensions;
using Simulating;

namespace Client;

public class Visualizer : IDisposable
{
    private readonly TextWriter _writer;

    public Visualizer(TextWriter writer)
    {
        _writer = writer;
    }

    public void Dispose()
    {
        _writer.Flush();
        _writer.Close();
        _writer.Dispose();
    }

    public void WriteGraph(ShiftPlan shiftPlan, Seconds end)
    {
        int index = 1;
        foreach (var shift in shiftPlan.Shifts)
        {
            _writer.Write($"{index++}: ");

            for (Seconds time = 0.ToSeconds(); time < end; time += (5 * 60).ToSeconds())
            {
                if (time.Value % 1.ToHours().ToSeconds().Value == 0)
                {
                    _writer.Write($"{time.Value / (60 * 60)}");
                }
                else
                {
                    _writer.Write(shift.Work.IsInInterval(time) ? "-" : " ");
                }
            }

            _writer.WriteLine();
            _writer.Write($"{shift.Id}: ");

            for (Seconds time = 0.ToSeconds(); time < end; time += (5 * 60).ToSeconds())
            {
                _writer.Write(shift.PlannedIncident(time) is not null ? "=" : " ");
            }

            _writer.WriteLine();
        }

        _writer.Flush();
    }

    public void WriteStats(Statistics stats)
    {
        _writer.WriteLine("Success rate: " + stats.SuccessRate);
        _writer.WriteLine("Unhandled incidents: \n" + stats.UnhandledIncidents.Select(inc => inc.Id).Visualize());
        _writer.Flush();
    }

    public void WriteSuccessRatedIncidentsSet(List<SuccessRatedIncidents> incidentsSet)
    {
        int rank = 1;
        foreach (var incidents in incidentsSet)
        {
            _writer.WriteLine($"({rank})");
            WriteSuccessRatedIncidents(incidents);
            _writer.WriteLine("--------------------");
        }
        
        _writer.Flush();
    }

    public void WriteSuccessRatedIncidents(SuccessRatedIncidents incidents)
    {
        incidents.Value.OrderBy(inc => inc.Occurence.Value);
        _writer.WriteLine($"Success rate: {incidents.SuccessRate}");
        
        foreach (var incident in incidents.Value)
        {
            _writer.Write($"({incident.Id}): ");
            WriteSuccessRatedIncident(incident);
        }
        
        _writer.Flush();
    }

    public void WriteSuccessRatedIncident(Incident incident)
    {
        _writer.WriteLine(incident.ToString());
        _writer.Flush();
    }
}