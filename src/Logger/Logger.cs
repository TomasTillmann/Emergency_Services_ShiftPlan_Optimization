namespace Logging;

public class Logger : IDisposable
{
    public static readonly Logger Instance = new();

    private TextWriter writer = new StreamWriter("Log.txt"); 

    private Logger() { }

    public void Write(object? message)
    {
#if DEBUG
        writer.Write(message);
        writer.Flush();
#endif
    }

    public void WriteLine(object? message)
    {
#if DEBUG
        writer.WriteLine(message);
        writer.Flush();
#endif
    }

    public void WriteLine()
    {
#if DEBUG
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
        writer.Flush();
        writer.Dispose();
    }
}