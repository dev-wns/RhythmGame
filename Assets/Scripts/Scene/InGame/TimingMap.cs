using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class TimingMap
{
    private ReadOnlyCollection<Timing> Timings;
    private ReadOnlyCollection<double> AccDistances; // 구간별 누적된 거리
    private double mainBPM;
    private int    pos;
    
    public void Initialize( double _mainBPM, ReadOnlyCollection<Timing> _timings )
    {
        if ( _mainBPM <= double.Epsilon || _timings == null ) return;

        List<double> accList = new List<double>( _timings.Count + 1 );
        accList.Add( 0d ); // 현재 위치의 이전값을 보기에 첫 요소는 0

        double elapsed  = 0d;
        double prevTime = 0d;
        for ( int i = 0; i < _timings.Count; i++ )
        {
            double time = _timings[i].time;
            double bpm  = _timings[i].bpm / _mainBPM;

            elapsed += bpm * ( time - prevTime );

            accList.Add( elapsed );
            prevTime = time;
        }

        pos          = 0;
        mainBPM      = _mainBPM;
        Timings      = _timings;
        AccDistances = new ReadOnlyCollection<double>( accList );
    }

    public void Release()
    {
        Timings      = null;
        AccDistances = null;
        mainBPM      = 0;
        pos          = 0;
    }

    public double TimeToDistance( double _time )
    {
        if ( Timings == null || Timings.Count == 0 ) return 0d;

        while ( pos > 0 && _time < Timings[pos].time ) pos--;
        for ( int i = pos; i < Timings.Count; i++ )
        {
            if ( Timings[i].time > _time ) // 다음 타이밍일 때 종료
                 break;  

            pos = i; // 현재 타이밍 위치
        }

        Timing timing   = Timings[pos];
        double bpm      = timing.bpm / mainBPM;
        double distance = bpm * ( _time - timing.time );

        return AccDistances[pos] + distance; // 이전 타이밍 누적값 + 현재 타이밍 시작시간부터의 거리
    }
}
