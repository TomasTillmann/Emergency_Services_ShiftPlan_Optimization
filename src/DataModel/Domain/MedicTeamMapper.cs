namespace ESSP.DataModel;

public class MedicTeamMapper
{
    public MedicTeamModel Map(MedicTeam team)
    {
        return new MedicTeamModel
        {
            Shift = team.Shift
        };
    }

    public MedicTeam MapBack(MedicTeamModel model)
    {
        return new MedicTeam
        {
            Shift = model.Shift 
        };
    }
}