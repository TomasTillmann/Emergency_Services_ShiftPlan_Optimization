//#define PRINT

namespace Logging;

public class Logger : IDisposable
{
    public static readonly Logger Instance = new();
    public static readonly string LogDivider = new string('#', 50);

    private TextWriter writer;

    private Logger()
    {
        writer = new StreamWriter("/home/tom/School/Bakalarka/Emergency_Services_ShiftPlan_Optimization/src/Log.txt", append: true);

        writer.WriteLine();
        writer.WriteLine(LogDivider);
        writer.WriteLine();
        writer.Flush();
    }

    public void Write(object? message)
    {
#if PRINT
        writer.Write(message);
        writer.Flush();
#endif
    }

    public void WriteForce(object? message)
    {
        writer.Write(message);
        writer.Flush();
    }

    public void WriteLine(object? message)
    {
#if PRINT
        writer.WriteLine(message);
        writer.Flush();
#endif
    }

    public void WriteLineForce(object? message)
    {
        writer.WriteLine(message);
        writer.Flush();
    }

    public void WriteLine()
    {
#if PRINT
        writer.WriteLine();
        writer.Flush();
#endif
    }

    public void WriteLineForce()
    {
        writer.WriteLine();
        writer.Flush();
    }

    ~Logger()
    {
        Dispose();
    }

    public void Dispose()
    {
        writer.Flush();
        writer.Dispose();
    }
}