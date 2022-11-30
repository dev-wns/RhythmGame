using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JudgeGraph : MonoBehaviour
{
    public TextMeshProUGUI minText, maxText;
    private LineRenderer rdr;
    private List<Vector3> positions = new List<Vector3>();
    private const float StartPosX = -875f;
    private const float EndPosX   = -175f;
    private const int TotalJudge  = 100;

    private void Awake()
    {
        //List<HitData> hitDatas = new List<HitData>();
        //double time = 0d;
        //for ( int i = 0; i < 10000; i++ )
        //{
        //    double diff = UnityEngine.Random.Range( (float)-Judgement.Bad, (float)Judgement.Bad );
        //    double diffAbs = Global.Math.Abs( diff );
        //    //double diff = UnityEngine.Random.Range( -.005f, .005f );

        //    time += 1d;

        //    //hitDatas.Add( new HitData( HitResult.None, time, ( double )diff ) );
        //    hitDatas.Add( new HitData( HitResult.None, time, 0d ) );
        //}
        //rdr = GetComponent<LineRenderer>();

        Result scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Result>();
        Judgement judge = scene.Judge;
        if ( judge == null || !TryGetComponent( out rdr ) )
            return;
        var hitDatas    = judge.hitDatas;

        var posY        = ( transform as RectTransform ).anchoredPosition.y;
        float posOffset = Global.Math.Abs( StartPosX - EndPosX ) / ( float )( TotalJudge + 1 );
        int divideCount = ( int )( hitDatas.Count / TotalJudge );
        List<double> deviations = new List<double>();

        positions.Add( new Vector3( StartPosX, posY , 0f ) );
        for ( int i = 0; i < TotalJudge; i++ )
        {
            var diffRange = hitDatas.GetRange( i * divideCount, divideCount );
            deviations.Add( diffRange.Sum( d => d.diff ) / diffRange.Count );
        }

        bool canDivide = false;
        double minDeviationAbs = Global.Math.Abs( deviations.Min() );
        double maxDeviationAbs = Global.Math.Abs( deviations.Max() );
        double deviationAverage = minDeviationAbs < maxDeviationAbs ? maxDeviationAbs : minDeviationAbs;
        for ( int i = 0; i < deviations.Count; i++ )
        {
            canDivide = deviationAverage > double.Epsilon && Global.Math.Abs( deviations[i] ) > double.Epsilon;
            if ( canDivide )
            {
                float devideAverage = ( float )( deviations[i] / deviationAverage );
                float averageMilliseconds = ( int )( devideAverage * 1000f );
                float result = averageMilliseconds < 5 ? 0f : devideAverage;
                Vector3 newPos = new Vector3( StartPosX + ( posOffset * positions.Count ), posY + ( ( float )result * 100f ), 0 );
                positions.Add( newPos );
            }
            else
            {
                Vector3 newPos = new Vector3( StartPosX + ( posOffset * positions.Count ), posY, 0 );
                positions.Add( newPos );
            }
        }
        positions.Add( new Vector3( EndPosX, posY , 0f ) );

        int deviationMilliseconds = ( int )( deviationAverage * 1000d );
        int maxDeviationAverage   = canDivide && deviationMilliseconds > 5 ? deviationMilliseconds : 1;
        minText.text = $"{-maxDeviationAverage} ms";
        maxText.text = $"{maxDeviationAverage} ms";
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
            float time = 0f;
            while ( Vector3.Distance( newVector, positions[i] ) > .00001f )
            {
                newVector = Vector3.Lerp( newVector, positions[i], time );
                rdr.SetPosition( i, newVector );

                time += Time.deltaTime * 75;
                yield return null;
            }

            rdr.SetPosition( i, positions[i] );
        }
    }
}
