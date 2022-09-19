using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeDistribution : MonoBehaviour
{
    private LineRenderer rdr;
    private List<Vector3> positions = new List<Vector3>();
    private const int TotalJudge = 100;
    private readonly float PosOffset = 700f / ( TotalJudge + 2 );

    private void Awake()
    {
        Result scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Result>();
        Judgement judge = scene.Judge;
        if ( judge == null || !TryGetComponent( out rdr ) ) return;

        var hitDatas   = judge.hitDatas;
        hitDatas.Sort(delegate( HitData A, HitData B )
        {
            if      ( A.time > B.time  ) return 1;
            else if ( A.time < B.time  ) return -1;
            else                         return 0;
        } );

        double offset      = ( ( NowPlaying.Inst.CurrentSong.totalTime / GameSetting.CurrentPitch ) * .001d ) / TotalJudge;
        double divideTime  = offset;
        List<double> diffs = new List<double>();
        positions.Add( new Vector3( -875f, 100f, 0f ) );
        for ( int i = 0; i < hitDatas.Count; i++ )
        {
            var diffAbs = Globals.Abs( hitDatas[i].diff );
            diffs.Add( diffAbs <= Judgement.Perfect ?  0d : diffAbs * 2000d );

            if ( hitDatas[i].time > divideTime )
            {
                positions.Add( new Vector3( -875f + ( PosOffset * ( positions.Count + 1 ) ),
                                             100f - Globals.Clamp( ( ( float )diffs.Average() * 2 ), 0f, 200f ), 0 ) );
                diffs.Clear();
                divideTime += offset;
            }
        }
        positions.Add( new Vector3( -875f + PosOffset * ( positions.Count + 1 ), 100f, 0f ) );
        StartCoroutine( UpdatePosition() );
    }

    private IEnumerator UpdatePosition()
    {
        rdr.positionCount = 1;
        rdr.SetPosition( 0, positions[0] );
        for ( int i = 1; i < positions.Count; i++ )
        {
            Vector3 newVector = positions[i - 1];
            rdr.positionCount = i + 1;
            while ( Vector3.Distance( newVector, positions[i] ) > .00001f )
            {
                newVector = Vector3.MoveTowards( newVector, positions[i], Time.deltaTime * TotalJudge * 10 );
                rdr.SetPosition( i, newVector );
                yield return null;
            }

            rdr.SetPosition( i, positions[i] );
        }
    }
}
