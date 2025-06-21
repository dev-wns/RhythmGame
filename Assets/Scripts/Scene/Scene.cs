using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Build Index
public enum SceneType : int
{
    FreeStyle = 1, Game, Result, // SiglePlay 
    Lobby, Stage,                // MultiPlay
};

[RequireComponent( typeof( SpriteRenderer ) )]
public abstract class Scene : SceneKeyAction
{
    #region Unity Callback
    protected virtual void Awake()
    {
        NowPlaying np = NowPlaying.Inst;

        Connect();
        CreateFadeSprite();

        NowPlaying.CurrentScene = this;
        KeyBind();
        ChangeAction( ActionType.Main );

        //AudioManager.Inst.SetVolume( AudioManager.Volume, ChannelType.BGM );
        AudioManager.Inst.Pause = false;
    }

    protected virtual void Start()
    {
        StartCoroutine( FadeIn() );
    }

    protected virtual void OnDestroy()
    {
        Disconnect();
    }
    #endregion

    #region Scene Load
    public void LoadScene( SceneType _type )
    {
        StopAllCoroutines();
        StartCoroutine( SceneChange( _type ) );
    }

    private IEnumerator SceneChange( SceneType _type )
    {
        DOTween.KillAll();
        DOTween.Clear();
        DOTween.ClearCachedTweens();

        IsInputLock = true;
        yield return StartCoroutine( FadeOut() );

        AudioManager.Inst.AllStop();
        AudioManager.Inst.PitchReset();
        SceneManager.LoadScene( ( int )_type );
    }
    #endregion

    #region Option Control
    protected void MoveToPrevOption( OptionController _controller )
    {
        _controller.PrevMove();
        AudioManager.Inst.Play( SFX.MenuSelect );
    }

    protected void MoveToNextOption( OptionController _controller )
    {
        _controller.NextMove();
        AudioManager.Inst.Play( SFX.MenuSelect );
    }

    protected void EnableCanvas( ActionType _changeType, OptionController _controller, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    {
        GameObject root = _controller.transform.root.gameObject;
        root.SetActive( true );
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 0f;
            DOTween.To( () => 0f, x => group.alpha = x, 1f, Global.Const.CanvasFadeDuration );
        }
        ChangeAction( _changeType );

        if ( _isSfxPlay )
            AudioManager.Inst.Play( SFX.MenuClick );

        if ( _hasFadeVolume )
             AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, AudioManager.Inst.Volume * .5f, .5f );
    }

    protected void EnableCanvas( ActionType _changeType, GameObject _obj, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    {
        GameObject root = _obj.transform.root.gameObject;
        root.SetActive( true );
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 0f;
            DOTween.To( () => 0f, x => group.alpha = x, 1f, Global.Const.CanvasFadeDuration );
        }

        ChangeAction( _changeType );

        if ( _isSfxPlay )
             AudioManager.Inst.Play( SFX.MenuClick );

        if ( _hasFadeVolume )
             AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, AudioManager.Inst.Volume * .5f, .5f );
    }

    protected void ImmediateDisableCanvas( ActionType _changeType, OptionController _controller )
    {
        _controller.transform.root.gameObject.SetActive( false );

        ChangeAction( _changeType );

        AudioManager.Inst.Play( SFX.MenuExit );
        AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, 1f, .5f );
    }

    protected void DisableCanvas( ActionType _changeType, OptionController _controller, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    {
        GameObject root = _controller.transform.root.gameObject;
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 1f;
            DOTween.To( () => 1f, x => group.alpha = x, 0f, Global.Const.CanvasFadeDuration ).OnComplete( () => root.SetActive( false ) );
        }
        else
        {
            root.SetActive( false );
        }
        ChangeAction( _changeType );

        if ( _isSfxPlay )
            AudioManager.Inst.Play( SFX.MenuExit );

        if ( _hasFadeVolume )
             AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, 1f, .5f );
    }

    protected void DisableCanvas( ActionType _changeType, GameObject _obj, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    {
        GameObject root = _obj.transform.root.gameObject;
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 1f;
            DOTween.To( () => 1f, x => group.alpha = x, 0f, Global.Const.CanvasFadeDuration ).OnComplete( () => root.SetActive( false ) );
        }
        else
        {
            root.SetActive( false );
        }

        ChangeAction( _changeType );

        if ( _isSfxPlay )
             AudioManager.Inst.Play( SFX.MenuExit );

        if ( _hasFadeVolume )
             AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, 1f, .5f );
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
        Config.Inst.Write( ConfigType.ScrollSpeed, GameSetting.ScrollSpeed );
    }

    protected void SpeedControlProcess( bool _isPlus )
    {
        if ( _isPlus )
        {
            AudioManager.Inst.Play( SFX.Slider );
            GameSetting.ScrollSpeed += 1;
        }
        else
        {
            if ( GameSetting.ScrollSpeed > 1 )
                 AudioManager.Inst.Play( SFX.Slider );

            GameSetting.ScrollSpeed -= 1;
        }

        OnScrollChange?.Invoke();
    }
    #endregion

    #region Fade
    private SpriteRenderer blackSprite;
    private static readonly float FadeTime     = .65f;
    private static readonly float FadeWaitTime = .025f;
    private static readonly float FadeDuration = FadeTime + ( FadeWaitTime * 2f );

    private void CreateFadeSprite()
    {
        Texture2D tex = Texture2D.whiteTexture;
        blackSprite = GetComponent<SpriteRenderer>();
        blackSprite.sprite = Sprite.Create( tex, new Rect( 0f, 0f, tex.width, tex.height ), new Vector2( .5f, .5f ), 100, 0, SpriteMeshType.FullRect );

        blackSprite.drawMode = SpriteDrawMode.Sliced;
        blackSprite.size = new Vector2( 10000, 10000 );
        blackSprite.sortingOrder = 100;
        blackSprite.color = Color.black;

        transform.localScale = Vector3.one;
    }

    protected IEnumerator FadeIn()
    {
        blackSprite.color = Color.black;
        blackSprite.enabled = true;

        yield return YieldCache.WaitForSeconds( FadeWaitTime );

        blackSprite.DOFade( 0f, FadeTime );
        yield return YieldCache.WaitForSeconds( FadeDuration );
        blackSprite.color = Color.clear;
        blackSprite.enabled = false;
    }

    protected IEnumerator FadeOut()
    {
        blackSprite.color = Color.clear;
        blackSprite.enabled = true;

        yield return YieldCache.WaitForSeconds( FadeWaitTime );

        blackSprite.DOFade( 1f, FadeTime );
        yield return YieldCache.WaitForSeconds( FadeDuration );
        blackSprite.color = Color.black;
    }
    #endregion

    #region Etc.
    public abstract void Connect();

    public abstract void Disconnect();
    #endregion
}