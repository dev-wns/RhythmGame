using UnityEngine;

using Global.Shader.InstancedFade;

public class PreviewNoteAllPart : PreviewNoteRenderer
{
    [SerializeField] private NoteMesh head;
    [SerializeField] private NoteMesh body;
    [SerializeField] private NoteMesh tail;

    protected override void Initialize()
    {
        head.Initialize( NoteSize, NoteColor );

        body.gameObject.SetActive( IsSlider );
        tail.gameObject.SetActive( IsSlider );

        if ( IsSlider )
        {
            body.Initialize( NoteSize, NoteColor, isRaise );
            tail.Initialize( NoteSize, NoteColor );
        }
    }

    protected override void OnUpdateSlider( float _length )
    {
        body.LocalScale = new Vector3( NoteSize.x, _length, 1f );

        float fadeOffset = isFade ? Global.Math.Clamp( FadeOffset / _length, 0f, float.MaxValue ) : 0f;
        body.SetFloatInMpb( ShaderID.Offset, fadeOffset );

        float raiseOffset  = isRaise ? 0f : NoteSize.y;
        tail.LocalPosition = new Vector3( 0f, Global.Math.Clamp( _length - raiseOffset, 0f, float.MaxValue ), 0f );
    }

    protected override void ApplyMpb()
    {
        head.ApplyMpb();
        if ( IsSlider )
        {
            tail.ApplyMpb();
            body.ApplyMpb();
        }
    }
}
