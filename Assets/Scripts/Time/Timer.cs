using System.Runtime.InteropServices;

public class Timer
{
    public readonly long Frequency;
    protected long startTick;

    [DllImport( "Assistant" )]
    protected static extern bool GetPerformanceCounter( out long qpc );
    [DllImport( "Assistant" )]
    protected static extern bool GetPerformanceFrequency( out long qpf );
    public Timer() // 생성 후 시작
    {
        if ( !GetPerformanceFrequency( out long frequency ) ) Frequency = 10000000;
        else                                                  Frequency = frequency;

        startTick = NowTick;
    }

    public long NowTick { get { GetPerformanceCounter( out long now ); return now; } }

    // 외부 틱 사용
    public virtual long   GetTicks(        long _now ) => ( _now - startTick );
    public virtual double GetSeconds(      long _now ) => ( ( double )GetTicks( _now ) / Frequency );
    public virtual double GetMilliseconds( long _now ) => ( ( double )GetTicks( _now ) / Frequency ) * 1000d;
    public virtual double GetMicroseconds( long _now ) => ( ( double )GetTicks( _now ) / Frequency ) * 1000000d;

    // 내부 틱 사용
    public virtual long   Ticks        => GetTicks(        NowTick );
    public virtual double Seconds      => GetSeconds(      NowTick );
    public virtual double Milliseconds => GetMilliseconds( NowTick );
    public virtual double Microseconds => GetMicroseconds( NowTick );
}
