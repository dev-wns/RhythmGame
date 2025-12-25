using UnityEngine;

public class NoteHead : NoteMesh
{
    public override void Initialize( Vector3 _size, Color _color, bool _isRaise )
    {
        mesh.transform.localPosition = Vector3.zero;
        mesh.transform.localScale    = _size;

        mpb.SetFloat( OffsetID, 0f ); // ∆‰¿ÃµÂ æ»µ 
        mpb.SetColor( ColorID, _color );
        mesh.SetPropertyBlock( mpb );
    }
}
