using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;


// Build Index
public enum SceneType : int { Lobby = 1, FreeStyle, Game, Result };
[RequireComponent( typeof( SpriteRenderer ) )]
public abstract class Scene : SceneKeyAction, IKeyBind, IDSPControl
{
    #region Variables
    private SpriteRenderer blackSprite;
    private readonly float FadeTime = .65f;
    #endregion

    #region Unity Callback
    protected virtual void Awake()
    {
        //Cursor.visible = false;

        CreateFadeSprite();
        Camera.main.orthographicSize = ( Screen.height / ( GameSetting.PPU * 2f ) ) * GameSetting.PPU;
        
        NowPlaying.CurrentScene = this;
        KeyBind();
        ChangeAction( ActionType.Main );
    }

    protected virtual void Start()
    {
        StartCoroutine( FadeIn() );
        Connect();
    }

    protected virtual void OnDestroy()
    {
        Disconnect();
    }
    #endregion

    #region Load
    public void LoadScene( SceneType _type )
    {
        StopAllCoroutines();
        StartCoroutine( SceneChange( _type ) );
    }

    private IEnumerator SceneChange( SceneType _type )
    {
        DOTween.KillAll();
        IsInputLock = true;

        yield return StartCoroutine( FadeOut() );

        SoundManager.Inst.AllStop();
        SoundManager.Inst.PitchReset();
        SceneManager.LoadScene( ( int )_type );
    }
    #endregion

    #region Input
    public event Action OnScrollChange;

    private bool isPressed = false;
    private float pressWaitTime = .5f;
    private float pressUpdateTime = .05f;
    private float pressTime;

    protected void PressedSpeedControl( bool _isPlus )
    {
        pressTime += Time.deltaTime;
        if ( pressTime >= pressWaitTime )
            isPressed = true;

        if ( isPressed && pressTime >= pressUpdateTime )
        {
            pressTime = 0f;
            SpeedControlProcess( _isPlus );
        }
    }

    protected void UpedSpeedControl()
    {
        pressTime = 0f;
        isPressed = false;
    }

    protected void SpeedControlProcess( bool _isPlus )
    {
        if ( _isPlus )
        {
            SoundManager.Inst.Play( SoundSfxType.Slider );
            GameSetting.ScrollSpeed += .1d;
        }
        else
        {
            if ( GameSetting.ScrollSpeed > 1.0001d )
                 SoundManager.Inst.Play( SoundSfxType.Slider );

            GameSetting.ScrollSpeed -= .1d;
        }

        OnScrollChange?.Invoke();
    }
    #endregion
    #region Effect
    public abstract void Connect();

    public abstract void Disconnect();

    private void CreateFadeSprite()
    {
        //gameObject.layer = 6; // 3d

        Texture2D tex = Texture2D.whiteTexture;
        blackSprite = GetComponent<SpriteRenderer>();
        blackSprite.sprite = Sprite.Create( tex, new Rect( 0f, 0f, tex.width, tex.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );

        blackSprite.drawMode = SpriteDrawMode.Sliced;
        blackSprite.size = new Vector2( 10000, 10000 );
        blackSprite.sortingOrder = 100;

        transform.localScale = Vector3.one;
    }

    protected IEnumerator FadeIn()
    {
        blackSprite.color = Color.black;
        blackSprite.enabled = true;
        blackSprite.DOFade( 0f, FadeTime );
        yield return YieldCache.WaitForSeconds( FadeTime + .1f );
        blackSprite.enabled = false;
    }

    protected IEnumerator FadeOut()
    {
        blackSprite.color = Color.clear;
        blackSprite.enabled = true;
        blackSprite.DOFade( 1f, FadeTime );
        yield return YieldCache.WaitForSeconds( FadeTime + .1f );
    }
    #endregion
}
