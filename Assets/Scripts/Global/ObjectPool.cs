using System.Collections.Generic;
using UnityEngine;

public interface IObjectPool<T> where T : MonoBehaviour
{
    public ObjectPool<T> pool { get; set; }
}

public class ObjectPool<T> where T : MonoBehaviour
{
    private T prefab;
    private Transform parent;
    private  List<T>  totalObjects = new List<T>();
    private Queue<T> waitObjects  = new Queue<T>();
    
    private bool isActiveControl; // 렌더러의 enbled 등으로 처리하고싶을 때
    public uint ActiveCount     { get; private set; }
    public uint AllocateCount   { get; set; } = 1;

    public ObjectPool( T _prefab, uint _initAlloc, bool _isActiveCtrl = true )
    {
        isActiveControl = _isActiveCtrl;
        if ( ReferenceEquals( _prefab, null ) )
        {
            Debug.LogError( "objectpool Constructor failed" );
        }
        prefab = _prefab;

        GameObject canvas = GameObject.FindGameObjectWithTag( "Pools" );
        if ( ReferenceEquals( canvas, null ) )
        {
            Debug.LogError( "not find pool canvas" );
        }

        GameObject parentObj = new GameObject();
        parentObj.transform.parent = canvas.transform;
        parentObj.transform.localPosition = Vector3.zero;
        parentObj.transform.localRotation = Quaternion.identity;
        parentObj.transform.localScale = Vector3.one;
        parentObj.name = string.Format( "{0} Pool", typeof( T ).Name );

        parent = parentObj.transform;
        Allocate( _initAlloc );
    }

    public ObjectPool( T _prefab, Transform _parent, uint _initAlloc, bool _isActiveCtrl = true )
    {
        isActiveControl = _isActiveCtrl;
        if ( ReferenceEquals( _prefab, null ) )
        {
            Debug.LogError( "objectpool Constructor failed" );
        }
        prefab = _prefab;
        parent = _parent;
        Allocate( _initAlloc );
    }

    private void Allocate( uint _count )
    {
        for ( int i = 0; i < _count; i++ )
        {
            T obj = UnityEngine.GameObject.Instantiate( prefab, parent );
            if ( obj.TryGetComponent( out IObjectPool<T> _base ) )
                 _base.pool = this;

            if ( isActiveControl )
                 obj.gameObject.SetActive( false );

            totalObjects.Add( obj );
            waitObjects.Enqueue( obj );
        }
    }

    public T Spawn()
    {
        if ( waitObjects.Count == 0 )
            Allocate( AllocateCount );

        T obj = waitObjects.Dequeue();
        if ( isActiveControl )
             obj.gameObject.SetActive( true );

        ActiveCount++;

        return obj;
    }

    public void Despawn( T _obj )
    {
        if ( isActiveControl )
             _obj.gameObject.SetActive( false );

        waitObjects.Enqueue( _obj );
        ActiveCount--;
    }

    public void AllDespawn()
    {
        for ( int i = 0; i < totalObjects.Count; i++ )
        {
            if ( totalObjects[i].gameObject.activeInHierarchy )
                 Despawn( totalObjects[i] );
        }
    }
}