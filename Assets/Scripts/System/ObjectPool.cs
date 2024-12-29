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
    private List<T>  totalObjects = new List<T>();
    private Queue<T> waitObjects  = new Queue<T>();
    private int allocateCount;
    public int ActiveCount { get; private set; }

    public ObjectPool( T _prefab, int _initializeCount, int _allocateCount = 1 )
    {
        allocateCount = _allocateCount;

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
        Allocate( _initializeCount );
    }
    public ObjectPool( T _prefab, Transform _parent, int _initializeCount, int _allocateCount = 1 )
    {
        allocateCount = _allocateCount;

        if ( ReferenceEquals( _prefab, null ) )
        {
            Debug.LogError( "objectpool Constructor failed" );
        }
        prefab = _prefab;
        parent = _parent;
        Allocate( _initializeCount );
    }
    private void Allocate( int _allocateCount )
    {
        if ( _allocateCount < 0 )
             return;

        T[] objects = new T[_allocateCount];
        for( int i = 0; i < _allocateCount; i++ )
        {
            T obj = UnityEngine.GameObject.Instantiate( prefab, parent );
            if ( obj.TryGetComponent( out IObjectPool<T> _base ) )
                 _base.pool = this;

            obj.gameObject.SetActive( false );
            objects[i] = obj;
            waitObjects.Enqueue( obj );
        }

        totalObjects.AddRange( objects );
    }

    public T Spawn()
    {
        if ( waitObjects.Count == 0 )
             Allocate( allocateCount );

        T obj = waitObjects.Dequeue();
        obj.gameObject.SetActive( true );
        ActiveCount++;

        return obj;
    }

    public void Despawn( T _obj )
    {
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