using System.Collections.Generic;
using ESSP.DataModel;

namespace DataModel.Interfaces;

public interface IModel<T, Result>
{
    Result ShiftPlan { get; }

    /// <summary>
    /// how well model performed on last incidents with trained weights (ShiftPlan)
    /// </summary>
    double SuccessRate { get; }

    void Fit(T incidents);
    void Predict(T incidents);
}

public interface IModel : IModel<IEnumerable<Incident>, ShiftPlan> { }