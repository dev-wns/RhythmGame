using UnityEngine;

public abstract class NoteMesh : MonoBehaviour
{
    [SerializeField] protected MeshRenderer mesh;
    protected MaterialPropertyBlock mpb;

    [SerializeField] private Global.UI.Pivot pivot = Global.UI.Pivot.Bottom;
    public Vector3 LocalScale    { set { transform.localScale = value; } }
    public Vector3 LocalPosition { set { transform.localPosition = value; } }

    protected static readonly int PivotID  = Shader.PropertyToID( "_Pivot"  );
    protected static readonly int ColorID  = Shader.PropertyToID( "_Color"  );
    protected static readonly int OffsetID = Shader.PropertyToID( "_Offset"  );

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
        mpb.SetVector( PivotID, Global.UI.GetPivotVector( pivot ) );
    }

    public abstract void Initialize( Vector3 _size, Color _color, bool _isRaise = false );
    public void SetFloatInMpb( int _nameId, float _value ) => mpb.SetFloat( _nameId, _value );
    public void UpdatePropertyBlock() => mesh.SetPropertyBlock( mpb );
}