
public class GameTimer : Timer
{
    private enum TimeState : byte { Idle, Running, Paused }
    private TimeState state;
    protected long saveTick;

    public GameTimer() : base() { Restart(); }
    public override long GetTicks( long _now ) => state == TimeState.Running ? saveTick + ( _now - startTick ) : saveTick;

    public void Restart()
    {
        saveTick  = 0;
        startTick = NowTick;
        state     = TimeState.Running;
    }

    public void Pause()
    {
        if ( state != TimeState.Running ) return;

        saveTick += ( NowTick - startTick );
        state = TimeState.Paused;
    }

    public void Resume()
    {
        if ( state != TimeState.Paused ) return;

        startTick = NowTick;
        state = TimeState.Running;
    }
}
