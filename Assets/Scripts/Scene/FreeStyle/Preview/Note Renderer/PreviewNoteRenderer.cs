using UnityEngine;

public abstract class PreviewNoteRenderer : MonoBehaviour, IObjectPool<PreviewNoteRenderer>
{
    public ObjectPool<PreviewNoteRenderer> pool { get; set; }

    private Note   note;
    private float  column;
    private double distance; // 스냅으로 줄어드는 양까지 포함
    private double despawnTime;
    private bool   isStart;

    [SerializeField] protected bool isFade;
    [SerializeField] protected bool isRaise;
    [SerializeField] protected bool isCircle;
    protected Vector2 NoteSize  { get; private set; }
    protected Color   NoteColor { get; private set; }
    protected bool    IsSlider => note.isSlider;

    protected static readonly float FadeOffset = 150f;

    protected abstract void Initialize();
    protected abstract void OnUpdateSlider( float _length );
    protected abstract void ApplyMpb();

    public void SetInfo( in Note _note, float _startPos, Color _color )
    {
        note        = _note;
        NoteSize    = isCircle ? new Vector3( PreviewNoteSystem.NoteWidth, PreviewNoteSystem.NoteWidth,  1f ) :
                                 new Vector3( PreviewNoteSystem.NoteWidth, PreviewNoteSystem.NoteHeight, 1f );
        column      = _startPos + ( note.lane * NoteSize.x ) + ( ( note.lane + 1 ) * GameSetting.NoteBlank );
        distance    = note.distance;
        despawnTime = IsSlider ? note.endTime : note.time;
        NoteColor   = _color;

        Initialize();
        isStart = true;
    }

    private void LateUpdate()
    {
        if ( !isStart )
             return;

        if ( IsSlider )
        {
            if ( note.distance < PreviewNoteSystem.Distance )
                 distance = PreviewNoteSystem.Distance;

            float bodyLength = ( float )( ( note.endDistance - distance ) / GameSetting.Pitch * PreviewNoteSystem.Weight );
            float bodyOffset = isRaise ? NoteSize.y : 0f; // 노트 크기만큼 길이를 줄이고( tail도 같이 내려옴 ), Body 위치만 크기의 반절만큼 올린다.
            float length     = Global.Math.Clamp( bodyLength - bodyOffset, 0f, float.MaxValue );

            OnUpdateSlider( length );
        }

        // 부모 갱신
        transform.localPosition = new Vector3( column, -390f + ( ( float )( distance - PreviewNoteSystem.Distance ) / GameSetting.Pitch * PreviewNoteSystem.Weight ), 0f );
        ApplyMpb();

        if ( despawnTime - PreviewNoteSystem.Playback < double.Epsilon )
             Despawn();
    }

    private void Despawn()
    {
        isStart = false;
        transform.localPosition = new Vector2( int.MaxValue, int.MaxValue );
        pool.Despawn( this );
    }
}