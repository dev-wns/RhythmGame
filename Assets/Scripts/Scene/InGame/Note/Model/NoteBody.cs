using UnityEngine;
using Global.Shader.InstancedFade;

public class NoteBody : NoteMesh
{
    public override void Initialize( Vector3 _size, Color _color, bool _isRaise )
    {
        float sizeY = _isRaise ? _size.y * .5f : 0f; // 헤드 높이의 반절만큼 올린다.
        mesh.transform.localPosition = new Vector3( 0f, sizeY, 0f );
        mesh.transform.localScale    = _size; // new Vector3( _size.x, 0f, 1f );

        mpb.SetColor( ShaderID.Color, _color );
        mesh.SetPropertyBlock( mpb );
    }
}
