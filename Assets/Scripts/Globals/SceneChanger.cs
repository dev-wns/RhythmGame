using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : Singleton<SceneChanger>
{
    public void Change( string _name )
    {
        StartCoroutine( LoadScene( _name ) );
    }

    private IEnumerator LoadScene( string _name )
    {
        AsyncOperation oper = SceneManager.LoadSceneAsync( _name );

        while ( !oper.isDone )
        {
            yield return null;
        }
    }
}
