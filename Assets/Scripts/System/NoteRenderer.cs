using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer : MonoBehaviour, IObjectPool<NoteRenderer>
{
    public ObjectPool<NoteRenderer> pool { get; set; }

    public SpriteRenderer head, body, tail;
    private Transform headTf, bodyTf, tailTf;
    private Note note;
    public int SpawnIndex { get; private set; }
    public float HeadPos => transform.position.y;
    public float TailPos => transform.position.y + BodyLength;
    public double Time => note.time;
    public double CalcTime => note.calcTime;
    public double SliderTime => note.sliderTime;
    public double CalcSliderTime => note.calcSliderTime;
    public bool IsSlider => note.isSlider;
    public bool ShouldResizeSlider { get; set; }
    public float BodyLength { get; private set; }
    public KeySound Sound => note.keySound;

    private float column;
    private static readonly Color NoteFailColor = new Color( .25f, .25f, .25f, 1f );
    private double newTime;

    private void Awake()
    {
        headTf = head.transform;
        headTf.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );

        bodyTf = body.transform;
        bodyTf.transform.position = new Vector2( 0, GameSetting.NoteHeight * .5f );

        tailTf = tail.transform;
        tailTf.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );
    }

    public void SetInfo( int _lane, in Note _note, int _spawnIndex )
    {
        //system  = _system;
        SpawnIndex = _spawnIndex;
        ShouldResizeSlider = false;
        note      = _note;

        column = GameSetting.NoteStartPos + ( _lane * GameSetting.NoteWidth ) + ( ( _lane + 1 ) * GameSetting.NoteBlank );
        newTime = note.calcTime;

        body.enabled = tail.enabled = IsSlider;
        head.color   = tail.color   = Color.white;
        body.color   = GameSetting.IsNoteBodyGray ? Color.gray : Color.white;
    }

    public void SetBodyFail() => head.color = body.color = tail.color = NoteFailColor;

    public void Despawn()
    {
        ShouldResizeSlider = false;
        pool.Despawn( this );
    }

    private void LateUpdate()
    {
       // �ճ�Ʈ �������� �ٱ�
        if ( IsSlider )
        {
            if ( ShouldResizeSlider && Time < NowPlaying.Playback )
                 newTime = NowPlaying.ScaledPlayback;

            BodyLength   = ( float )( ( CalcSliderTime - newTime ) * GameSetting.Weight );
            float length =  Global.Math.Clamp( BodyLength - GameSetting.NoteHeight,  0f, float.MaxValue );
            bodyTf.localScale    = new Vector2( GameSetting.NoteBodyWidth, length );
            tailTf.localPosition = new Vector2( 0f, length );
        }

        transform.position = new Vector2( column, GameSetting.JudgePos + ( float )( ( newTime - NowPlaying.ScaledPlayback ) * GameSetting.Weight ) );
    }
}