using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance = null;
    public static T Inst
    {
        get
        {
            if ( ReferenceEquals( null, instance ) )
            {
                T[] objs = FindObjectsByType<T>( FindObjectsSortMode.None );
                if ( objs.Length > 1 ) Debug.LogError( string.Format( "create multiple singleton objects. #Name : {0}", typeof( T ) ) );
                if ( objs.Length > 0 ) instance = objs[0];
                if ( ReferenceEquals( null, instance ) )
                {
                    GameObject obj = new GameObject( typeof( T ).Name );
                    instance = obj.AddComponent<T>();
                }
                DontDestroyOnLoad( instance );
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        T[] objs = FindObjectsByType<T>( FindObjectsSortMode.None );
        if ( objs.Length == 1 )
        {
            instance = objs[0];
            DontDestroyOnLoad( this );
        }
        else if ( objs.Length > 1 )
            Destroy( this );
    }
}

//public class Singleton<T> where T : class, new()
//{
//    private static readonly Lazy<T> instance = new Lazy<T>( () => new T() );
//    public static T Inst { get { return instance.Value; } }
//}
