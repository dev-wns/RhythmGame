
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;


public class Gear : MonoBehaviour
{
    [Header( "Gear" )]
    public Transform judge;
    public Transform panel;
    public Transform sideLeft, sideRight;

    public RectTransform hitCount;

    public Transform helpTransform;
    public Transform healthBGTransform;
    public Transform healthRendererTransform;

    [Header( "Lane" )]
    public  Lane prefab;
    public  Transform laneParent;
    private List<Lane> lanes = new();

    public static ReadOnlyCollection<Note>[] Notes { get; private set; } // 레인별로 분할된 노트 데이터

    private void Awake()
    {
        NowPlaying.OnInitialize += Initialize;
        NowPlaying.OnLoadAsync  += DivideNotes;
        Judgement.OnHitNote     += Transfer;
    }

    private void OnDestroy()
    {
        NowPlaying.OnInitialize -= Initialize;
        NowPlaying.OnLoadAsync  -= DivideNotes;
        Judgement.OnHitNote     -= Transfer;

        Notes = null;
    }

    private void Start()
    {
        UpdatePosition();

        if ( GameSetting.BGAOpacity == 0 )
        {
            panel.gameObject.SetActive( false );
            sideLeft.GetComponent<SpriteRenderer>().color  = Color.black;
            sideRight.GetComponent<SpriteRenderer>().color = Color.black;
        }
        else
        {
            if ( GameSetting.PanelOpacity == 0 )
                panel.gameObject.SetActive( false );
            else
            {
                panel.GetComponent<SpriteRenderer>().color = new Color( 0f, 0f, 0f, GameSetting.PanelOpacity * .01f );
                panel.localScale = new Vector3( GameSetting.GearWidth, Global.Screen.Height );
                panel.position   = new Vector2( GameSetting.GearOffsetX, 0f );
            }
        }

        sideLeft.position                = new Vector3( GameSetting.GearStartPos, 0f );
        sideRight.position               = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth, 0f );
        helpTransform.position           = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 5f,  ( -Global.Screen.Height * .5f ) + 50f, 0f );
        healthBGTransform.position       = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 17f, ( -Global.Screen.Height * .5f ) + ( helpTransform.localScale.y * .5f ), 0f );
        healthRendererTransform.position = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 33f, ( -Global.Screen.Height * .5f ) + helpTransform.localScale.y, 0f );
    }

    private void Initialize()
    {
        Notes = new ReadOnlyCollection<Note>[NowPlaying.KeyCount];
        for ( int i = 0; i < NowPlaying.KeyCount; i++ )
        {
            lanes.Add( Instantiate( prefab, laneParent ) );
            lanes[i].Initialize( i );
        }
        Debug.Log( $"Create {lanes.Count} lanes." );
    }

    private void Transfer( HitData _data )
    {
        lanes[_data.lane].AddData( _data );
    }

    private void UpdatePosition()
    {
        judge.position   = new Vector2( GameSetting.GearOffsetX, GameSetting.JudgePos );
        judge.localScale = new Vector3( GameSetting.GearWidth, judge.localScale.y );
    }

    /// <summary> 게임모드가 적용된 상태로 노트를 분할합니다. </summary>
    private void DivideNotes()
    {
        int keyCount = NowPlaying.KeyCount;
        bool isNoSlider = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        bool isConvert  = GameSetting.CurrentGameMode.HasFlag( GameMode.ConvertKey ) && NowPlaying.CurrentSong.keyCount == 7;

        List<int/* lane */> emptyLanes = new List<int>( keyCount );
        List<Note>[] notes             = Array.ConvertAll( new int[keyCount], _ => new List<Note>() );
        System.Random random           = new System.Random( ( int )DateTime.Now.Ticks );
        double[] prevTimes             = Enumerable.Repeat( double.MinValue, keyCount ).ToArray();
        double   secondPerBeat         = ( ( ( 60d / NowPlaying.MainBPM ) * 4d ) / 32d );
        for ( int i = 0; i < DataStorage.Notes.Count; i++ )
        {
            Note newNote = DataStorage.Notes[i];
            newNote.isSlider = isNoSlider ? false : newNote.isSlider;
            if ( isConvert )
            {
                if ( newNote.lane == 3 )
                {
                    // 잘린 노트의 키음은 자동재생되도록 한다.
                    DataStorage.Inst.LoadSound( newNote.keySound.name );
                    DataStorage.Inst.AddSample( newNote.keySound );
                    continue;
                }
                else if ( newNote.lane > 3 )
                {
                    // 제외된 중앙노트보다 우측의 노트는 한칸 이동시킨다.
                    newNote.lane -= 1;
                }
            }

            switch ( GameSetting.CurrentRandom )
            {
                // 레인 인덱스와 동일한 번호에 노트 분배
                case GameRandom.None:
                case GameRandom.Mirror:
                case GameRandom.Basic_Random:
                case GameRandom.Half_Random:
                {
                    newNote.distance    = NowPlaying.Inst.GetDistance( newNote.time );
                    newNote.endDistance = NowPlaying.Inst.GetDistance( newNote.endTime );

                    DataStorage.Inst.LoadSound( newNote.keySound.name );
                    notes[newNote.lane].Add( newNote );
                }
                break;

                // 맥스랜덤은 무작위 레인에 노트를 배치한다.
                case GameRandom.Max_Random:
                {
                    emptyLanes.Clear();
                    // 빠른계단, 즈레 등 고밀도로 배치될 때 보정
                    for ( int j = 0; j < keyCount; j++ )
                    {
                        if ( secondPerBeat < ( newNote.time - prevTimes[j] ) )
                            emptyLanes.Add( j );
                    }

                    // 자리가 없을 때 보정되지않은 상태로 배치
                    if ( emptyLanes.Count == 0 )
                    {
                        for ( int j = 0; j < keyCount; j++ )
                        {
                            if ( prevTimes[j] < newNote.time )
                                emptyLanes.Add( j );
                        }
                    }

                    int selectLane        = emptyLanes[random.Next( 0, int.MaxValue ) % emptyLanes.Count];
                    prevTimes[selectLane] = newNote.isSlider ? newNote.endTime : newNote.time;

                    newNote.lane = selectLane;
                    newNote.distance    = NowPlaying.Inst.GetDistance( newNote.time );
                    newNote.endDistance = NowPlaying.Inst.GetDistance( newNote.endTime );

                    DataStorage.Inst.LoadSound( newNote.keySound.name );
                    notes[selectLane].Add( newNote );
                }
                break;
            }
        }

        // 기본방식의 랜덤은 노트분배가 끝난 후, 완성된 데이터를 스왑한다.
        switch ( GameSetting.CurrentRandom )
        {
            case GameRandom.Mirror:       notes.Reverse();                           break;
            case GameRandom.Basic_Random: Global.Math.Shuffle( notes, 0, keyCount ); break;
            case GameRandom.Half_Random:
            {
                int keyCountHalf = Mathf.FloorToInt( keyCount * .5f );
                Global.Math.Shuffle( notes, 0,                keyCountHalf );
                Global.Math.Shuffle( notes, keyCountHalf + 1, keyCount );
            }
            break;
        }

        for ( int i = 0; i < keyCount; i++ )
        {
            Notes[i] = new ReadOnlyCollection<Note>( notes[i] );
        }
    }
}
