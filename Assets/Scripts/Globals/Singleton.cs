using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance = null;
    public static T inst
    {
        get
        {
            if ( ReferenceEquals( null, instance ) )
            {
                T[] objs = FindObjectsOfType<T>();
                if ( objs.Length > 0 ) instance = objs[ 0 ];
                if ( objs.Length > 1 ) Debug.Log( "create multiple singleton objects. #Name : " + typeof( T ) );
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
}
