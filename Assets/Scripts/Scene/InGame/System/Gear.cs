
using System;
using System.Collections.Generic;
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

    private void Initialize()
    {
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

    private void Awake()
    {
        NowPlaying.OnInitialize += Initialize;
        InputManager.OnHitNote  += Transfer;
    }

    private void OnDestroy()
    {
        NowPlaying.OnInitialize -= Initialize;
        InputManager.OnHitNote  -= Transfer;
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

    private void UpdatePosition()
    {
        judge.position   = new Vector2( GameSetting.GearOffsetX, GameSetting.JudgePos );
        judge.localScale = new Vector3( GameSetting.GearWidth, judge.localScale.y );
    }
}
