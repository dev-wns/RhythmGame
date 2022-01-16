using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    private float startTime;
    public  float elapsedSeconds { get { return ( float )( System.DateTime.Now.TimeOfDay.TotalMilliseconds - startTime ) * .001f; } }
    public  float elapsedMilliSeconds { get { return ( float )System.DateTime.Now.TimeOfDay.TotalMilliseconds - startTime; } }

    public void Start() { startTime = ( float )System.DateTime.Now.TimeOfDay.TotalMilliseconds; }
    public float End { get { return elapsedMilliSeconds; } }
}
