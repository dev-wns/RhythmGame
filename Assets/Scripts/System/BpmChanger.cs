using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class BpmChanger : MonoBehaviour
{
    private InGame scene;
    private ReadOnlyCollection<Timing> timings;
    private int curIndex;
    
    [Header("Sprite")]
    public int sortingOrder;

    [Header("BPM Changer")]
    public List<Sprite> sprites = new List<Sprite>();
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private CustomHorizontalLayoutGroup layoutGroup;
    private int prevNum, curNum;

    [Header("Time")]
    private const double DelayTime = 1d / 60d;
    private Timing curTiming;
    private bool isStart;
    private double time;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>(); 
        scene.OnSystemInitialize += Initialize;
        scene.OnReLoad += ReLoad;

        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();

        for ( int i = 0; i < images.Count; i++ )
              images[i].sortingOrder = sortingOrder;
    }

    private void ReLoad()
    {
        StopAllCoroutines();
        prevNum = curNum = 0;
        curIndex = 0;
        time = 0d;

        images[0].gameObject.SetActive( true );
        images[0].sprite = sprites[0];
        for ( int i = 1; i < images.Count; i++ )
        {
            images[i].gameObject.SetActive( false );
        }
        layoutGroup.SetLayoutHorizontal();
        Initialize();
    }

    private void Initialize( Chart _chart )
    {
        timings = _chart.timings;
        Initialize();
    }

    private void Initialize()
    {
        curIndex = 0;
        curTiming = timings[curIndex];
        time = curTiming.time;
        ImageUpdate( curTiming.bpm );
        isStart = true;
    }

    private void LateUpdate()
    {
        if ( isStart && curIndex < timings.Count &&
             time < NowPlaying.Playback )
        {
            ImageUpdate( curTiming.bpm );

            if ( ++curIndex < timings.Count )
            {
                curTiming = timings[curIndex];
                time = curIndex + 1 < timings.Count && Global.Math.Abs( timings[curIndex + 1].time - curTiming.time ) > DelayTime ?
                       curTiming.time + DelayTime : curTiming.time;
            }
        }
    }

    private void ImageUpdate( double _bpm )
    {
        double calcCurBpm = _bpm;
        curNum = Global.Math.Log10( _bpm ) + 1;
        for ( int i = 0; i < images.Count; i++ )
        {
            if ( i < curNum )
            {
                if ( !images[i].gameObject.activeSelf )
                    images[i].gameObject.SetActive( true );

                images[i].sprite = sprites[( int )calcCurBpm % 10];
                calcCurBpm *= .1d;
            }
            else
            {
                if ( images[i].gameObject.activeSelf )
                    images[i].gameObject.SetActive( false );
            }
        }

        if ( prevNum != curNum )
            layoutGroup.SetLayoutHorizontal();

        prevNum = curNum;
    }
}
