#define PRINT

namespace Logging;

public class Logger : IDisposable
{
    public static readonly Logger Instance = new();

    private TextWriter writer;

    private Logger()
    {
#if PRINT
        writer = new StreamWriter("Log.txt"); 
#endif
    }

    public void Write(object? message)
    {
#if PRINT
        writer.Write(message);
        writer.Flush();
#endif
    }

    public void WriteLine(object? message)
    {
#if PRINT
        writer.WriteLine(message);
        writer.Flush();
#endif
    }

    public void WriteLine()
    {
#if PRINT
        writer.WriteLine();
        writer.Flush();
#endif
    }

    ~Logger()
    {
        Dispose();
    }

    public void Dispose()
    {
#if PRINT
        writer.Flush();
        writer.Dispose();
#endif
    }
}