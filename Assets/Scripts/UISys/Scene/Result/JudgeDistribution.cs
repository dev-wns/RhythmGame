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

        positions.Add( new Vector3( -875f, 0f, 0f ) );
        for ( int i = 0; i < hitDatas.Count; ++i )
        {
            ++judgeCount;
            average += Globals.Abs( hitDatas[i].diff ) <= Judgement.Perfect ? 0d : ( Judgement.Bad * 1000d ) / ( hitDatas[i].diff * 1000d );

            if ( hitDatas[i].time > curTime )
            {
                positions.Add( new Vector3( -875f + ( PosOffset * posCount++ ),
                                               Globals.Clamp( ( float )( average / judgeCount ) * 100f, -100f, 100f ), 0 ) );
                curTime += offset;
                judgeCount = 0;
                average = 0d;
            }
        }
        positions.Add( new Vector3( -875f + ( PosOffset * ( posCount + 1 ) ), 0f, 0f ) );

        //rdr.positionCount = positions.Count;
        //rdr.SetPositions( positions.ToArray() );

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
                newVector = Vector3.MoveTowards( newVector, positions[i], Time.deltaTime * ( TotalJudge + 2 ) * 10 );
                rdr.SetPosition( i, newVector );
                yield return null;
            }

            rdr.SetPosition( i, positions[i] );
        }
    }
}
