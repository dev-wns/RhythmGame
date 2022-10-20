using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeDistribution : MonoBehaviour
{
    private LineRenderer rdr;
    private List<Vector3> positions = new List<Vector3>();
    private const float StartPosX = -875f;
    private const float EndPosX   = -175f;
    private const int TotalJudge  = 100;
    //private readonly float PosOffset = 700f / ( TotalJudge + 2 );

    private void Awake()
    {
        Result scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Result>();
        Judgement judge = scene.Judge;
        if ( judge == null || !TryGetComponent( out rdr ) ) return;

        var hitDatas   = judge.hitDatas;
        hitDatas.Sort( delegate ( HitData A, HitData B )
         {
             if ( A.time > B.time )      return 1;
             else if ( A.time < B.time ) return -1;
             else                        return 0;
         } );

        float posOffset = Global.Math.Abs( StartPosX - EndPosX ) / ( float )( TotalJudge + 1 );
        int divideCount = ( int )( hitDatas.Count / ( TotalJudge + 2 ) );
        int curCount = 0;
        List<double> diffs = new List<double>();
        positions.Add( new Vector3( StartPosX, 100f, 0f ) );
        for ( int i = 0; i < hitDatas.Count; i++ )
        {
            var diffAbs = hitDatas[i].result == HitResult.Perfect ? 0d :
                          hitDatas[i].result == HitResult.Miss    ? Judgement.Miss :
                                                                    Global.Math.Abs( hitDatas[i].diff );

            float t = ( float )diffAbs * ( float )( 1d / Judgement.Miss );
            var weight  = Global.Math.Lerp( 1f, 5000f, 1f - Mathf.Cos( t * Mathf.PI * 0.5f ) );
            diffs.Add( diffAbs * weight );

            if ( divideCount == ++curCount )
            {
                if ( TotalJudge <= positions.Count - 1 )
                    break;

                float average  = ( float )diffs.Average();
                Vector3 newPos = new Vector3( StartPosX + ( posOffset * positions.Count ), Global.Math.Clamp( 100f - average, -100f, 100f ), 0 );
                positions.Add( newPos );
                curCount = 0;
                diffs.Clear();
            }
        }
        positions.Add( new Vector3( EndPosX, 100f, 0f ) );
    }

    private void Start()
    {
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
