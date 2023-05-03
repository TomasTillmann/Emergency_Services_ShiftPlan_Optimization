namespace Logging;

public class Logger : IDisposable
{
    public static readonly Logger Instance = new();

    private TextWriter writer = Console.Out;

    private Logger() { }

    public void Write(object message)
    {
        writer.Write(message);
    }

    public void WriteLine(object message)
    {
        writer.WriteLine(message);
    }

    public void WriteLine()
    {
        writer.WriteLine();
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