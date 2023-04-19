using System;
using ESSP.DataModel;

namespace DataModel.Interfaces;
using Id = UInt32;

public interface IIdentifiable
{
    public Id Id { get; }
}