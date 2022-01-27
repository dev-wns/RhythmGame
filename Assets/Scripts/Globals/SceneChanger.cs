using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;


// Build Index
public enum SceneType : int { Lobby = 1, FreeStyle, Game, Result };

[RequireComponent( typeof( SpriteRenderer ) )]
public class SceneChanger : SingletonUnity<SceneChanger>
{
    public static Scene CurrentScene;
    private SpriteRenderer blackSprite;
    private Coroutine loadCoroutine;

    private void Awake()
    {
        gameObject.layer = 5; // UI

        Texture2D tex = Texture2D.whiteTexture;
        blackSprite = GetComponent<SpriteRenderer>();
        blackSprite.sprite = Sprite.Create( tex, new Rect( 0f, 0f, tex.width, tex.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );

        blackSprite.drawMode = SpriteDrawMode.Sliced;
        blackSprite.size = new Vector2( 1920, 1080 );
        blackSprite.sortingOrder = 100;

        transform.localScale = Vector3.one;
    }

    public void InitSceneChange() => StartCoroutine( InitSceneChange( SceneType.Lobby ) );

    public void LoadScene( SceneType _type )
    {
        if ( loadCoroutine != null ) return;

        loadCoroutine = StartCoroutine( FadeBackground( _type ) );
    }

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
        CurrentScene?.InputLock( true );
        CurrentScene = null;

        blackSprite.enabled = true;
        blackSprite.color = Color.clear;
        blackSprite.DOFade( 1f, .7f );
        yield return YieldCache.WaitForSeconds( 1f );

        SoundManager.Inst.AllStop();

        //AsyncOperation oper = SceneManager.LoadSceneAsync( ( int )_type );
        //if ( !oper.isDone ) yield return null;

        SceneManager.LoadScene( ( int )_type );
        //Globals.Timer.Start();
        System.GC.Collect();
        //Debug.Log( $"GC Collect : {Globals.Timer.End} ms" );

        blackSprite.DOFade( 0f, .7f );
        yield return YieldCache.WaitForSeconds( 1f );
        
        //CurrentScene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        //CurrentScene?.InputLock( false );
        blackSprite.enabled = false;
        loadCoroutine = null;
    }
}
