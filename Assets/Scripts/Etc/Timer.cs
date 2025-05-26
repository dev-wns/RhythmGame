public class Timer
{
    private double startTime;
    public double ElapsedSeconds      => ( System.DateTime.Now.TimeOfDay.TotalMilliseconds - startTime ) * .001d;
    public double ElapsedMilliSeconds => System.DateTime.Now.TimeOfDay.TotalMilliseconds - startTime;

    public void Start() => startTime = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
    public uint End     => ( uint )ElapsedMilliSeconds;

    public double CurrentTime => System.DateTime.Now.TimeOfDay.TotalSeconds;

    public Timer() { Start(); }
    public Timer( bool _shouldStart )
    {
        if ( _shouldStart )
            Start();
    }
}
