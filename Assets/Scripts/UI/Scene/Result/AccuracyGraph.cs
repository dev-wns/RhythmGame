using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.Remoting.Messaging;

public class AccuracyGraph : MonoBehaviour
{
    public TextMeshProUGUI accuracyRangeText;
    private LineRenderer rdr;
    private List<Vector3> positions = new List<Vector3>();
    private const float StartPosX  = 200f;
    private const float EndPosX    = 900f;
    private const int   TotalJudge = 80;
    private const float Power      = 5f;


    private void Awake()
    {
        //List<HitData> hitDatas = new List<HitData>();
        //double times = 0d;
        //for ( int i = 0; i < 2500; i++ )
        //{
        //    double diff = UnityEngine.Random.Range( (float)-Judgement.Bad, (float)Judgement.Bad );
        //    times += 1d;
        //    hitDatas.Add( new HitData( HitResult.Default, diff, times ) );
        //}

        if ( !TryGetComponent( out rdr ) )
             return;

        var    hitDatas   = NowPlaying.Inst.HitDatas.FindAll( ( HitData _data ) => _data.type == NoteType.Default );
        float  posY       = ( transform as RectTransform ).anchoredPosition.y;
        float  posOffset  = Global.Math.Abs( StartPosX - EndPosX ) / ( float )( TotalJudge + 2 );

        List<HitData> datas    = new List<HitData>();
        double sumDivideDiff   = 0d;
        double totalSumMinDiff = 0d, totalSumMaxDiff = 0d;
        int    totalMinCount   = 0,  totalMaxCount   = 0;

        int step = ( hitDatas.Count / TotalJudge );
        float stepInverse = 1f / step;
        positions.Add( new Vector3( StartPosX, posY, 0f ) );
        for ( int i = 0; i < hitDatas.Count; i++ )
        {
            var diff = hitDatas[i].diff;
            if ( diff < 0d )
            {
                totalSumMinDiff += diff * 1000d;
                totalMinCount++;
            }
            else
            {
                totalSumMaxDiff += diff * 1000d;
                totalMaxCount++;
            }

            sumDivideDiff += diff;
            if ( i % step == 0 )
            {
                datas.Add( new HitData( NoteType.Default, sumDivideDiff * stepInverse, hitDatas[i].time ) );
                sumDivideDiff = 0d;
            }
        }
        
        for ( int i = 0; i < datas.Count; i++ )
        {
            if ( positions.Count == TotalJudge + 1 )
                 break;

            var avg = datas[i].diff * 1000f * Power;
            avg = Mathf.Round( ( float )( avg - ( avg % ( 5d * Power ) ) ) );
            positions.Add( new Vector3( StartPosX + ( posOffset * positions.Count ), Global.Math.Clamp( posY + ( float )avg, -170f, 50f ), 0 ) );
        }
        positions.Add( new Vector3( EndPosX - posOffset, posY, 0f ) );
        positions.Add( new Vector3( EndPosX, posY, 0f ) );

        int minAverageMS = totalMinCount == 0 ? 0 : Mathf.RoundToInt( ( float )( totalSumMinDiff / totalMinCount ) );
        int maxAverageMS = totalMaxCount == 0 ? 0 : Mathf.RoundToInt( ( float )( totalSumMaxDiff / totalMaxCount ) );
        accuracyRangeText.text = $"{minAverageMS} ms ~ {maxAverageMS} ms";
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
