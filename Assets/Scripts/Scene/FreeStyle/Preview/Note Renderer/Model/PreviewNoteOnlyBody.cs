using UnityEngine;
using Global.Shader.InstancedFade;

public class PreviewNoteOnlyBody : PreviewNoteRenderer
{
    [SerializeField] private NoteMesh body;

    protected override void Initialize()
    {
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
        body.ApplyMpb();
    }
}