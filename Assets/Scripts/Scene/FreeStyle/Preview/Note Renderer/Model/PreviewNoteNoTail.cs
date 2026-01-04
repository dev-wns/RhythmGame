using UnityEngine;
using Global.Shader.InstancedFade;

public class PreviewNoteNoTail : PreviewNoteRenderer
{
    [SerializeField] private NoteMesh head;
    [SerializeField] private NoteMesh body;
    protected override void Initialize()
    {
        head.Initialize( NoteSize, NoteColor );

        body.gameObject.SetActive( IsSlider );
        if ( IsSlider )
             body.Initialize( NoteSize, NoteColor, isRaise );
    }

    protected override void OnUpdateSlider( float _length )
    {
        body.LocalScale = new Vector3( NoteSize.x, _length, 1f );

        float offset = isFade ? Global.Math.Clamp( FadeOffset / _length, 0f, float.MaxValue ) : 0f;
        body.SetFloatInMpb( ShaderID.Offset, offset );
    }

    protected override void ApplyMpb()
    {
        head.ApplyMpb();
        if ( IsSlider )
             body.ApplyMpb();
    }
}