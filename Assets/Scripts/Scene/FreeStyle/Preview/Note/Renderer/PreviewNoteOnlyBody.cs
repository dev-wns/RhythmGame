using UnityEngine;

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

        float offset = isFade ? Global.Math.Clamp( FadeOffset / _length, 0.001f, float.MaxValue ) : 0f;
        body.SetFloatInMpb( OffsetID, offset );
    }

    protected override void UpdatePropertyBlock()
    {
        body.UpdatePropertyBlock();
    }
}