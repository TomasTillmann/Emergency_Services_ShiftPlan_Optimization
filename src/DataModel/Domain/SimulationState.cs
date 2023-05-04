namespace ESSP.DataModel;

public class SimulationState
{
    public Seconds CurrentTime { get; set; } = 0.ToSeconds();

    public Seconds StepDuration { get; set; }
}
