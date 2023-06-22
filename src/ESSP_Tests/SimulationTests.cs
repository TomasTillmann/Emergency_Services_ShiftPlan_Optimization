using DataHandling;
using DataModel.Interfaces;
using ESSP.DataModel;
using Newtonsoft.Json;
using Simulating;
using System;

namespace ESSP_Tests;

public static partial class Helpers
{
    public static string ToJson(this object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static string ToJsonPretty(this object obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }
}
