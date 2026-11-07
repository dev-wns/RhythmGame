using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class TimingMap
{
    private ReadOnlyCollection<Timing> Timings;
    private ReadOnlyCollection<double> AccDistances; // 구간별 누적된 거리
    private double mainBPM;
    public  int Pos { get; private set; }
    
    public void Initialize( in Song _song )
    {
        Pos     = 0;
        mainBPM = _song.mainBPM;
        Timings = DataStorage.Inst.Timings;

        if ( mainBPM <= double.Epsilon || Timings == null ) return;

        double   cache = 0d;
        double[] accList = new double[Timings.Count];
        for ( int i = 0; i < Timings.Count; i++ )
        {
            accList[i] = cache;
            if ( i + 1 < Timings.Count )
            {
                double bpm = Timings[i].bpm / mainBPM;
                cache += bpm * ( Timings[i + 1].time - Timings[i].time );
            }
        }

        AccDistances = new ReadOnlyCollection<double>( accList );
    }

    public void Release()
    {
        Timings      = null;
        AccDistances = null;
        mainBPM      = 0;
        Pos          = 0;
    }

    public double TimeToDistance( double _time )
    {
        if ( Timings == null || Timings.Count == 0 ) return 0d;

        while ( Pos + 1 < Timings.Count && Timings[Pos + 1].time <= _time ) Pos++;
        while ( Pos > 0                 && Timings[Pos].time     >  _time ) Pos--;

        Timing current  = Timings[Pos];
        double bpm      = current.bpm / mainBPM;
        double distance = bpm * ( _time - current.time );

        return AccDistances[Pos] + distance; // 누적된 이전 값 + 현재 값( 타이밍 시작시간 ~ 현재시간 )
    }

    public void Seek( double _time )
    {
        int left  = 0;
        int right = Timings.Count - 1;
        while ( left <= right )
        {
            int mid = ( left + right ) >> 1;
            if ( Timings[mid].time <= _time ) left  = mid + 1;
            else                              right = mid - 1;
        }

        Pos = Mathf.Max( 0, right );
    }
}
