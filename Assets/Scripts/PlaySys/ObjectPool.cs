using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour
{
    private T poolableObject;
    private Transform parent;
    private Stack<T> pool = new Stack<T>();

    private int allocateCount = 100;

    public ObjectPool( T _poolableObject )
    {
        if ( ReferenceEquals( _poolableObject, null ) )
        {
            Debug.LogError( "objectpool Constructor failed" );
        }
        poolableObject = _poolableObject;

        GameObject canvas = GameObject.FindGameObjectWithTag( "InGameCanvas" );
        if ( ReferenceEquals( canvas, null ) )
        {
            Debug.LogError( "not find ingame canvas" );
        }

        GameObject parentObj = Instantiate( new GameObject(), canvas.transform );
        parentObj.transform.position = Vector3.zero;
        parentObj.transform.rotation = Quaternion.identity;
        parentObj.name = string.Format( "{0} Pool", typeof( T ).Name );

        parent = parentObj.transform;
    }

    private void Allocate()
    {
        for( int i = 0; i < allocateCount; i++ )
        {
            T obj = Instantiate( poolableObject, parent );
            obj.gameObject.SetActive( false );
            pool.Push( obj );
        }
    }

    public T Spawn()
    {
        if ( pool.Count <= 0 )
        {
            Allocate();
        }

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
