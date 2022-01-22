using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    private float startTime;
    public  float elapsedSeconds => ( float )( System.DateTime.Now.TimeOfDay.TotalMilliseconds - startTime ) * .001f;
    public  float elapsedMilliSeconds => ( float )System.DateTime.Now.TimeOfDay.TotalMilliseconds - startTime;

    public void Start() => startTime = ( float )System.DateTime.Now.TimeOfDay.TotalMilliseconds;
    public float End => elapsedMilliSeconds; 
}
