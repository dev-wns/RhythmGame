using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BpmChanger : MonoBehaviour
{
    private int timingIdx;

    //private IEnumerator BpmChange()
    //{
    //    BPM = Data.timings[0].bpm;
    //    BPMChangeEvent();

    //    while ( timingIdx < Data.timings.Count )
    //    {
    //        float changeTime = Data.timings[timingIdx].changeTime;
    //        yield return new WaitUntil( () => Playback >= changeTime );

    //        BPM = Data.timings[timingIdx].bpm;
    //        BPMChangeEvent();
    //        timingIdx++;
    //    }
    //    yield return null;
    //}
}
