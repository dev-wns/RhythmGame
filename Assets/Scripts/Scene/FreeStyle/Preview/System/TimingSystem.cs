using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TimingSystem
{
    private List<Timing> timings = new ();
    private Timing current;
    private int pos;
    
    // Cached
    private int startPos;
    private double startDistance;

    private const double DelayTime = ( 1d / 60d ) * 1000d;

    public void Initialize( in Song _song )
    {
        // 다음 곡이 선택됐을때 로딩 전까지 이전곡을 플레이하기위해 복사
        timings.Clear();
        timings.AddRange( DataStorage.Inst.Timings );

        // Seek PreviewTime
        for ( int i = 0; i < timings.Count; i++ )
        {
            double time = timings[i].time;
            double bpm  = timings[i].bpm / _song.mainBPM;

            if ( i + 1 < timings.Count && timings[i + 1].time < _song.previewTime )
            {
                startDistance += bpm * ( timings[i + 1].time - time );
                continue;
            }

            startPos = i;
            break;
        }
    }
}
