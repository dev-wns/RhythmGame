using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;


// Build Index
public enum SCENE_TYPE : int { LOBBY = 1, FREESTYLE, GAME, RESULT };

[RequireComponent( typeof( SpriteRenderer ) )]
public class SceneChanger : SingletonUnity<SceneChanger>
{
    private Scene CurrentScene;
    private SpriteRenderer blackSprite;
    private Coroutine curCoroutine;

    private void Awake()
    {
        DontDestroyOnLoad( this );
        blackSprite = GetComponent<SpriteRenderer>();
        blackSprite.sortingOrder = 100;
        transform.position   = new Vector3( 0f, 0f, -9 );
        transform.localScale = new Vector3( Screen.width, Screen.height, 1f );

        StartCoroutine( InitSceneChange( SCENE_TYPE.LOBBY ) );
    }

    public void LoadScene( SCENE_TYPE _type )
    {
        if ( curCoroutine != null ) return;

        curCoroutine = StartCoroutine( FadeBackground( _type ) );
    }

    private IEnumerator InitSceneChange( SCENE_TYPE _type )
    {
        blackSprite.enabled = true;
        blackSprite.color = Color.black;
        yield return YieldCache.WaitForSeconds( 1f );

        blackSprite.DOFade( 0f, 1f );

        yield return YieldCache.WaitForSeconds( 1f );
        StartCoroutine( FadeBackground( _type ) );
    }

    private IEnumerator FadeBackground( SCENE_TYPE _type )
    {
        DOTween.KillAll();
        CurrentScene?.InputLock( true );

        blackSprite.enabled = true;
        blackSprite.color = Color.clear;
        blackSprite.DOFade( 1f, 1f );
        yield return YieldCache.WaitForSeconds( 1f );

        SoundManager.Inst.AllStop();

        AsyncOperation oper = SceneManager.LoadSceneAsync( ( int )_type );
        if ( !oper.isDone ) yield return null;
        

        blackSprite.DOFade( 0f, 1f );
        yield return YieldCache.WaitForSeconds( 1f );
        
        CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        CurrentScene?.InputLock( false );
        blackSprite.enabled = false;
        curCoroutine = null;
    }
}
