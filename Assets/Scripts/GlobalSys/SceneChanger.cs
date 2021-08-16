using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger sceneChanger;
    private SoundHandler soundHandler;

    private void Start ()
    {
        sceneChanger = this.GetComponent<SceneChanger>();
        soundHandler = this.GetComponent<SoundHandler>();
    }

    private void Update ()
    {
        if ( SceneManager.GetActiveScene().name == "PlayMusic" )
        {
            // if ( FileReader.isLoaded )
            if ( Input.GetKeyDown( KeyCode.Escape ) )
            {
                StartCoroutine( LoadScene( "SelectMusic" ) );
                soundHandler.StopSound();
                soundHandler.ReleaseSound();
            }
        }
    }

    public void goRoom( string _name )
    {
        StartCoroutine( LoadScene( _name ) );
    }

    private IEnumerator LoadScene( string _sceneName )
    {
        AsyncOperation asyncOper = SceneManager.LoadSceneAsync( _sceneName );
        while( !asyncOper.isDone )
        {
            yield return null;
        }
    }
}
