using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private T prefab;
    private Transform parent;
    private Stack<T> pool = new Stack<T>();
    private int allocateCount;

    public ObjectPool( T _prefab, int _allocate = 100 )
    {
        allocateCount = _allocate;

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
    }
    private void Allocate()
    {
        for( int i = 0; i < allocateCount; i++ )
        {
            T obj = UnityEngine.GameObject.Instantiate( prefab, parent );
            obj.gameObject.SetActive( false );
            pool.Push( obj );
        }
    }
    public T Spawn()
    {
        if ( pool.Count == 0 )
             Allocate();

        T obj = pool.Pop();
        obj.gameObject.SetActive( true );

        return obj;
    }
    public void Despawn( T _obj )
    {
        _obj.gameObject.SetActive( false );
        pool.Push( _obj );
    }
}