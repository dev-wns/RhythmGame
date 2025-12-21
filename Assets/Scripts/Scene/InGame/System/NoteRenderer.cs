using UnityEngine;


public class NoteRenderer : MonoBehaviour, IObjectPool<NoteRenderer>
{
    public ObjectPool<NoteRenderer> pool { get; set; }

    public SpriteRenderer head, body, tail;
    private Note note;
    public float TailPos => transform.position.y + bodyLength;
    public double EndTime => note.endTime;
    public bool IsSlider => note.isSlider;
    public bool IsKeyDown     { get; set; }

    private static readonly Color NoteFailColor = new Color( .5f, .5f, .5f, 1f );
    private float  column;
    private float  bodyLength;
    private double newDistance;

    //private static bool IsOnlyBody = false;

    private void Awake()
    {
        head.transform.localScale    = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );
        tail.transform.localScale    = new Vector2( GameSetting.NoteWidth, GameSetting.NoteHeight );
        body.transform.localPosition = new Vector2( 0,                     GameSetting.NoteHeight * .5f );
        //body.transform.localPosition = IsOnlyBody ? Vector2.zero : new Vector2( 0, GameSetting.NoteHeight * .5f );
    }

    public void SetInfo( int _lane, in Note _note )
    {
        note        = _note;
        IsKeyDown   = false;
        newDistance = note.distance;

        column = GameSetting.NoteStartPos + ( _lane * GameSetting.NoteWidth ) + ( ( _lane + 1 ) * GameSetting.NoteBlank );

        //head.enabled = IsOnlyBody && IsSlider  ? false :
        //               IsOnlyBody && !IsSlider ? true  : true;
        //body.enabled = IsOnlyBody ? true  : IsSlider;
        //tail.enabled = IsOnlyBody ? false : IsSlider;
        body.enabled = tail.enabled = IsSlider;
        head.color = body.color = tail.color = Color.white;
    }

    public void SetSliderFail()
    {
        if ( IsKeyDown )
        {
            IsKeyDown = false;
            //newDistance = NowPlaying.Distance;
            Snap();
        }

        head.color = body.color = tail.color = NoteFailColor;
    }

    public void Snap()
    {
        if ( !IsSlider )
              return;

        newDistance = NowPlaying.Distance;
        bodyLength  = ( float ) ( ( note.endDistance - newDistance ) * GameSetting.Weight );

        float length = Global.Math.Clamp( bodyLength - GameSetting.NoteHeight, 0f, float.MaxValue );
        body.transform.localScale    = new Vector2( GameSetting.NoteWidth, length );
        tail.transform.localPosition = new Vector2( 0f, length );

        transform.localPosition = new Vector2( column, GameSetting.JudgePos + ( float ) ( newDistance - NowPlaying.Distance ) * GameSetting.Weight );
    }

    public void Despawn()
    {
        IsKeyDown = false;
        pool.Despawn( this );
    }

    private void LateUpdate()
    {
        // ∆«¡§º±ø° ∑’≥Î∆Æ Ω∫≥¿
        if ( IsSlider )
        {
            if ( IsKeyDown && note.distance < NowPlaying.Distance )
                 newDistance = NowPlaying.Distance;

            bodyLength = ( float )( ( note.endDistance - newDistance ) * GameSetting.Weight );

            //float length = Global.Math.Clamp( IsOnlyBody ? bodyLength : bodyLength - GameSetting.NoteHeight, 0f, float.MaxValue );
            float length = Global.Math.Clamp( bodyLength - GameSetting.NoteHeight, 0f, float.MaxValue );
            body.transform.localScale    = new Vector2( GameSetting.NoteWidth, length );
            tail.transform.localPosition = new Vector2( 0f, length );
        }

        transform.localPosition = new Vector2( column, GameSetting.JudgePos + ( float )( newDistance - NowPlaying.Distance ) * GameSetting.Weight );
    }
}
