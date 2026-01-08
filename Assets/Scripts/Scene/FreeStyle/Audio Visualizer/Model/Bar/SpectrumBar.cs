using UnityEngine;
using Global.UI;
using Global.Shader.InstancedFade;

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

    public void Initialize( Vector3 _pos, Color _color )
    {
        transform.localPosition = _pos;
        transform.localScale    = Vector3.one;

        mesh.sortingOrder = sortingOrder;
        mpb.SetVector( ShaderID.Pivot, Alignment.GetPivotVector( pivot ) );
        mpb.SetColor(  ShaderID.Color, _color );
    }
}
