using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    private double startTime;
    public  double elapsedSeconds => ( System.DateTime.Now.TimeOfDay.TotalMilliseconds - startTime ) * .001f;
    public double elapsedMilliSeconds => System.DateTime.Now.TimeOfDay.TotalMilliseconds - startTime;

    public void Start() => startTime = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
    public uint End => ( uint )elapsedMilliSeconds;

    public double CurrentTime => System.DateTime.Now.TimeOfDay.TotalSeconds;

    public Timer() { Start(); }
    public Timer( bool _shouldStart )
    {
        if ( _shouldStart )
             Start();
    }
}
