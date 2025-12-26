using UnityEngine;
using Global.Shader.InstancedFade;
using static Global.UI.Alignment;

public abstract class NoteMesh : MonoBehaviour
{
    [SerializeField] protected MeshRenderer mesh;
    protected MaterialPropertyBlock mpb;
    [SerializeField] private int sortingOrder;
    [SerializeField] private Pivot pivot = Pivot.Bottom;

    public Vector3 LocalScale    { set { transform.localScale = value; } }
    public Vector3 LocalPosition { set { transform.localPosition = value; } }

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
        mpb.SetVector( ShaderID.Pivot, GetPivotVector( pivot ) );
        mesh.sortingOrder = sortingOrder;
    }

    public abstract void Initialize( Vector3 _size, Color _color, bool _isRaise = false );
    public void SetFloatInMpb( int _nameId, float _value ) => mpb.SetFloat( _nameId, _value );
    public void UpdatePropertyBlock() => mesh.SetPropertyBlock( mpb );
}