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
    private Coroutine currentCoroutine;

    private void Awake()
    {
        gameObject.layer = 5; // UI

        Texture2D tex = Texture2D.whiteTexture;
        blackSprite = GetComponent<SpriteRenderer>();
        blackSprite.sprite = Sprite.Create( tex, new Rect( 0f, 0f, tex.width, tex.height ), new Vector2( .5f, .5f ), 100, 0, SpriteMeshType.FullRect );

        blackSprite.drawMode = SpriteDrawMode.Sliced;
        blackSprite.size = new Vector2( 1920, 1080 );
        blackSprite.sortingOrder = 100;

        transform.position   = new Vector3( 0f, 0f, -9f );
        transform.localScale = Vector3.one;
    }

    public void InitSceneChange() => StartCoroutine( InitSceneChange( SCENE_TYPE.LOBBY ) );

    public void LoadScene( SCENE_TYPE _type )
    {
        if ( currentCoroutine != null ) return;

        currentCoroutine = StartCoroutine( FadeBackground( _type ) );
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
        currentCoroutine = null;
    }
}
