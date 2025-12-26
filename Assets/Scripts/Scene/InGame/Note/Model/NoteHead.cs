using UnityEngine;
using Global.Shader.InstancedFade;

public class NoteHead : NoteMesh
{
    public override void Initialize( Vector3 _size, Color _color, bool _isRaise )
    {
        mesh.transform.localPosition = Vector3.zero;
        mesh.transform.localScale    = _size;

        mpb.SetFloat( ShaderID.Offset, 0f ); // ∆‰¿ÃµÂ æ»µ 
        mpb.SetColor( ShaderID.Color, _color );
        mesh.SetPropertyBlock( mpb );
    }
}
