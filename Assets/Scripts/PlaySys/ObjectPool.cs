using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private T poolableObject;
    private Transform parent;
    private Stack<T> pool = new Stack<T>();
    
    private int allocateCount;
    private bool isAutoActive;

    public ObjectPool( T _poolableObject, int _allocate = 100, bool _isAutoActive = true )
    {
        isAutoActive  = _isAutoActive;
        allocateCount = _allocate;

        if ( ReferenceEquals( _poolableObject, null ) )
        {
            Debug.LogError( "objectpool Constructor failed" );
        }
        poolableObject = _poolableObject;

        GameObject canvas = GameObject.FindGameObjectWithTag( "Pools" );
        if ( ReferenceEquals( canvas, null ) )
        {
            Debug.LogError( "not find ingame canvas" );
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
            T obj = UnityEngine.GameObject.Instantiate( poolableObject, parent );
            if ( isAutoActive )
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
        if ( isAutoActive )
            obj.gameObject.SetActive( true );

        return obj;
    }

    public void Despawn( T _obj )
    {
        if ( isAutoActive )
            _obj.gameObject.SetActive( false );

        pool.Push( _obj );
    }
}
