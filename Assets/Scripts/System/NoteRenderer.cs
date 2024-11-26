using UnityEngine;


public class NoteRenderer : MonoBehaviour, IObjectPool<NoteRenderer>
{
    public ObjectPool<NoteRenderer> pool { get; set; }

    private Note note;
    public SpriteRenderer head, body, tail;
    public float HeadPos => transform.position.y;
    public float TailPos => transform.position.y + BodyLength;
    public double Time => note.time;
    public double Distance => note.noteDistance;
    public double SliderTime => note.sliderTime;
    public double SliderDistance => note.sliderDistance;
    public bool IsSlider => note.isSlider;
    public bool IsKeyDown { get; set; }
    public float BodyLength { get; private set; }
    public KeySound Sound => note.keySound;

    private float column;
    private static readonly Color NoteFailColor = new Color( .5f, .5f, .5f, 1f );
    private double newDistance;

    private static bool IsOnlyBody = false;

    private void Awake()
    {
        head.transform.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );
        body.transform.localPosition = IsOnlyBody ? Vector2.zero : new Vector2( 0,GameSetting.NoteHeight * .5f );
        tail.transform.localScale = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );
    }

    public void SetInfo( int _lane, in Note _note )
    {
        IsKeyDown  = false;
        note       = _note;

        column = GameSetting.NoteStartPos + ( _lane * GameSetting.NoteWidth ) + ( ( _lane + 1 ) * GameSetting.NoteBlank );
        newDistance = note.noteDistance;

        head.enabled = IsOnlyBody && IsSlider  ? false :
                       IsOnlyBody && !IsSlider ? true : true;
        body.enabled = IsOnlyBody ? true  : IsSlider;
        tail.enabled = IsOnlyBody ? false : IsSlider;
        //body.enabled = tail.enabled = IsSlider;
        head.color   = body.color = tail.color = Color.white;
    }

    public void SetSliderFail()
    {
        if ( IsKeyDown )
        { 
            IsKeyDown = false;
            newDistance = NowPlaying.Distance;
        }

        head.color = body.color = tail.color = NoteFailColor;
    }

    public void Despawn()
    {
        IsKeyDown = false;
        pool.Despawn( this );
    }

    //private void LateUpdate()
    //{
    //    // 롱노트 판정선에 붙기
    //    if ( IsSlider )
    //    {
    //        if ( IsKeyDown && Distance < NowPlaying.Distance )
    //             newDistance = NowPlaying.Distance;

    //        BodyLength = ( float )( ( SliderDistance - newDistance ) * GameSetting.Weight );

    //        float length = Global.Math.Clamp( BodyLength - GameSetting.NoteHeight, 0f, float.MaxValue );
    //        body.transform.localScale = new Vector2( GameSetting.NoteWidth, length );
    //        tail.transform.localPosition = new Vector2( 0f, length );
    //    }

    //    transform.localPosition = new Vector2( column, GameSetting.JudgePos + ( float )( newDistance - NowPlaying.Distance ) * GameSetting.Weight );
    //}

    private void LateUpdate()
    {
        // 판정선에 롱노트 스냅
        if ( IsSlider )
        {
            if ( IsKeyDown && Distance < NowPlaying.Distance )
                 newDistance = NowPlaying.Distance;

            BodyLength = ( float )( ( SliderDistance - newDistance ) * GameSetting.Weight );

            float length = Global.Math.Clamp( IsOnlyBody ? BodyLength : BodyLength - GameSetting.NoteHeight, 0f, float.MaxValue );
            body.transform.localScale    = new Vector2( GameSetting.NoteWidth, length );
            tail.transform.localPosition = new Vector2( 0f, length );
        }

        transform.localPosition = new Vector2( column, GameSetting.JudgePos + ( float )( newDistance - NowPlaying.Distance ) * GameSetting.Weight );
    }
}
