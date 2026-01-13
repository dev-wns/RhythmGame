using System.Runtime.InteropServices;
public class FrameCounter
{
    public int  FPS  { get; private set; }
    public long Diff { get; private set; }

    private int  count;
    private long frequency;
    private long next;

    private long lastTick;

    [DllImport( "Assistant" )]
    public static extern bool GetPerformanceCounter( out long performanceCount );
    [DllImport( "Assistant" )]
    public static extern bool GetPerformanceFrequency( out long frequency );
    
    public FrameCounter() 
    {
        GetPerformanceFrequency( out frequency );
        GetPerformanceCounter( out next );
        next += frequency;
        lastTick = next;
    }

    public void Update( long _now )
    {
        Diff = _now - lastTick;
        lastTick = _now;

        count++;
        if ( _now >= next )
        {
            FPS   = count;
            count = 0;
            do { next += frequency; }
            while ( _now >= next );
        }
    }
}
