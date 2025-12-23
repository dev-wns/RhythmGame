using UnityEngine;

public class PreviewNoteRenderer : MonoBehaviour, IObjectPool<PreviewNoteRenderer>
{
    public ObjectPool<PreviewNoteRenderer> pool { get; set; }
    protected Note note;
    public SpriteRenderer head, body, tail;

    public MeshRenderer bodyMesh;

    public bool IsSlider => note.isSlider;

    protected float column;
    protected static readonly Color NoteFailColor = new Color( .5f, .5f, .5f, 1f );
    protected double newDistance;

    private bool isStart;

    private MaterialPropertyBlock headMpb;
    private MaterialPropertyBlock bodyMpb;

    private static readonly int ColorID  = Shader.PropertyToID( "_Color" );

    private Color color;
    private void Awake()
    {
        //head.transform.localScale = new Vector3( PreviewNoteSystem.NoteWidth, PreviewNoteSystem.NoteHeight, 1f );
        bodyMesh.transform.position   = new Vector3( 0,                           PreviewNoteSystem.NoteHeight * .5f, 0f );
        //tail.transform.localScale = new Vector3( PreviewNoteSystem.NoteWidth, PreviewNoteSystem.NoteHeight, 1f );

        //headMpb = new MaterialPropertyBlock();
        bodyMpb = new MaterialPropertyBlock();
    }


    public void SetInfo( in Note _note, float _startPos, Color _color )
    {
        note = _note;
        color = _color;
        column = _startPos + ( note.lane * ( PreviewNoteSystem.NoteWidth ) ) + ( ( note.lane + 1 ) * GameSetting.NoteBlank );
        newDistance = note.distance;

        bodyMesh.enabled = IsSlider;
        //tail.enabled = IsSlider;

        //head.transform.localScale = new Vector3( PreviewNoteSystem.NoteWidth, PreviewNoteSystem.NoteHeight, 1f );
        bodyMesh.transform.localPosition = new Vector3( 0, PreviewNoteSystem.NoteHeight * .5f, 0f );
        //tail.transform.localScale = new Vector3( PreviewNoteSystem.NoteWidth, PreviewNoteSystem.NoteHeight, 1f );

        //head.color = body.color = tail.color = _color;

        //head.GetPropertyBlock( headMpb );
        //headMpb.SetColor( ColorID, _color );
        //headMpb.SetFloat( "_IsFade", 0f );
        //head.SetPropertyBlock( headMpb );

        //if ( IsSlider )
        //{
        //    body.GetPropertyBlock( bodyMpb );
        //    bodyMpb.SetColor( ColorID, _color );
        //    bodyMpb.SetFloat( "_IsFade", 1f );
        //    body.SetPropertyBlock( bodyMpb );
        //}

        isStart = true;
    }

    private void LateUpdate()
    {
        if ( !isStart )
             return;

        float length = 1f;
        if ( IsSlider )
        {
            if ( note.distance < PreviewNoteSystem.Distance )
                 newDistance = PreviewNoteSystem.Distance;

            float bodyLength = ( float )( ( note.endDistance - newDistance ) / GameSetting.CurrentPitch * PreviewNoteSystem.Weight );
            length = Global.Math.Clamp( bodyLength - PreviewNoteSystem.NoteHeight, 0f, float.MaxValue );

            // Body
            bodyMesh.transform.localScale = new Vector3( PreviewNoteSystem.NoteWidth, length, 1f );


            // Tail
            //tail.transform.localPosition = new Vector2( 0f, length );
            //mpb.SetFloat( "_IsFade", 0f );
            //tail.SetPropertyBlock( mpb );

            // Parent
            transform.localPosition = new Vector3( column, -390f + ( ( float )( newDistance - PreviewNoteSystem.Distance ) / GameSetting.CurrentPitch * PreviewNoteSystem.Weight ), 0f );
            if ( note.endTime - PreviewNoteSystem.Playback < double.Epsilon )
                 Despawn();
        }
        else
        {
            transform.localPosition = new Vector3( column, -390f + ( ( float )( newDistance - PreviewNoteSystem.Distance ) / GameSetting.CurrentPitch * PreviewNoteSystem.Weight ), 0f );
            if ( note.time - PreviewNoteSystem.Playback < double.Epsilon )
                 Despawn();
        }

        bodyMpb.Clear();
        bodyMpb.SetColor( "_Color", color );
        bodyMpb.SetFloat( "_Offset", Global.Math.Clamp( 150f / length, 0.001f, float.MaxValue ) );
        //bodyMpb.SetFloat( "_IsFade", 1f );
        bodyMesh.SetPropertyBlock( bodyMpb );

        //head.SetPropertyBlock( headMpb );
    }

    private void Despawn()
    {
        isStart = false;
        transform.localPosition = new Vector2( int.MaxValue, int.MaxValue );
        pool.Despawn( this );
    }
}