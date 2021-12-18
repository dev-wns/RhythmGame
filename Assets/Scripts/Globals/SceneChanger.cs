using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public enum SceneType { Lobby, FreeStyle, InGame };

public class SceneChanger : SingletonUnity<SceneChanger>
{
    public Scene CurrentScene { get; private set; }
    private SpriteRenderer blackSprite;

    private void Awake()
    {
        DontDestroyOnLoad( this );
        blackSprite = GetComponent<SpriteRenderer>();
        transform.position   = new Vector3( 0f, 0f, 80f );
        transform.localScale = new Vector3( Screen.width, Screen.height, 1f );

        StartCoroutine( InitSceneChange( SceneType.Lobby ) );
    }
    public void LoadScene( SceneType _type ) => StartCoroutine( FadeBackground( _type ) );

    private IEnumerator InitSceneChange( SceneType _type )
    {
        blackSprite.enabled = true;
        blackSprite.color = Color.black;
        yield return YieldCache.WaitForSeconds( 1f );

        blackSprite.DOFade( 0f, 1f );

        yield return YieldCache.WaitForSeconds( 1f );
        StartCoroutine( FadeBackground( _type ) );
    }

    private IEnumerator FadeBackground( SceneType _type )
    {
        DOTween.KillAll();
        CurrentScene?.InputLock();

        blackSprite.enabled = true;
        blackSprite.color = Color.clear;
        blackSprite.DOFade( 1f, 1f );
        yield return YieldCache.WaitForSeconds( 1f );

        SoundManager.Inst.AllStop();

        AsyncOperation oper = SceneManager.LoadSceneAsync( _type.ToString() );
        if ( !oper.isDone ) yield return null;

        blackSprite.DOFade( 0f, 1f );
        yield return YieldCache.WaitForSeconds( 1f );
        
        blackSprite.enabled = false;
        CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
    }
}
