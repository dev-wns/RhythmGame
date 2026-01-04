using UnityEngine;
using Global.Shader.InstancedFade;

public class NoteCircle : NoteMesh
{
    public override void Initialize( Vector3 _size, Color _color, bool _isRaise )
    {
        LocalPosition = new Vector3( 0.016f, 0f, 0f );
        LocalScale    = new Vector3( _size.x * .97f, _size.y, 1f );

        mpb.SetFloat( ShaderID.Offset, 0f ); // ∆‰¿ÃµÂ æ»µ 
        mpb.SetColor( ShaderID.Color, _color );
        ApplyMpb();
    }
}
