using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeDistribution : MonoBehaviour
{
    private LineRenderer rdr;
    private List<Vector3> newPositions = new List<Vector3>();
    private const int TotalJudge = 100;
    private readonly float PosOffset = 700f / ( TotalJudge + 2);

    private void Awake()
    {
        Result scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Result>();
        Judgement judge = scene.Judge;
        if ( judge == null || !TryGetComponent( out rdr ) ) return;

        double offset  = ( ( NowPlaying.Inst.CurrentSong.totalTime / GameSetting.CurrentPitch ) * .001d ) / TotalJudge;
        double curTime = offset;
        double average = 0d;
        int judgeCount = 0, posCount = 1;
        var hitDatas   = judge.hitDatas;
        hitDatas.Sort(delegate( Judgement.HitData A, Judgement.HitData B)
        {
            if      ( A.time > B.time  ) return 1;
            else if ( A.time < B.time  ) return -1;
            else                         return 0;
        } );

        newPositions.Add( new Vector3( -875f, 0f, 0f ) );
        for ( int i = 0; i < hitDatas.Count; ++i )
        {
            ++judgeCount;
            average += Globals.Abs( hitDatas[i].diff ) <= Judgement.Perfect ? 0d : ( Judgement.Bad * 1000d ) / ( hitDatas[i].diff * 1000d );

            if ( hitDatas[i].time > curTime )
            {
                newPositions.Add( new Vector3( -875f + ( PosOffset * posCount++ ),
                                               Globals.Clamp( ( float )( average / judgeCount ) * 100f, -100f, 100f ), 0 ) );
                curTime += offset;
                judgeCount = 0;
                average = 0d;
            }
        }
        newPositions.Add( new Vector3( -875f + ( PosOffset * ( posCount + 1 ) ), 0f, 0f ) );

        rdr.positionCount = newPositions.Count;
        rdr.SetPositions( newPositions.ToArray() );
    }
}
