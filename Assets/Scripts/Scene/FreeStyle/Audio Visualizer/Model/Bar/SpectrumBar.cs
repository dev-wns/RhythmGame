using UnityEngine;
using Global.UI;

public class SpectrumBar : MonoBehaviour
{
    [SerializeField] private MeshRenderer mesh;
    private MaterialPropertyBlock mpb;

    [SerializeField] private int   sortingOrder;
    [SerializeField] private Pivot pivot;

        
    public Vector3 LocalScale 
    { 
        set 
        { 
            transform.localScale = value; 
            mesh.SetPropertyBlock( mpb ); 
        }
    }

    public void Awake()
    {
        mpb = new MaterialPropertyBlock();
    }

    public void Initailze( Vector3 _pos, Color _color )
    {
        transform.localPosition = _pos;
        transform.localScale    = Vector3.one;

        mesh.sortingOrder = sortingOrder;
        mpb.SetVector( "_Pivot", Alignment.GetPivotVector( pivot ) );
        mpb.SetColor(  "_Color", _color );
    }
}
