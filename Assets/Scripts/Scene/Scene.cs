using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Build Index
public enum SceneType : int { FreeStyle = 1, Game, Result };

[RequireComponent( typeof( SpriteRenderer ) )]
public abstract class Scene : SceneKeyAction
{
    #region Variables
    public static bool OnceTweenInit;
    public bool IsGameInputLock { get; set; }
    public Action<float/* pitch */> OnUpdatePitch;
    #endregion

    #region Unity Callback
    protected virtual void Awake()
    {
        if ( !OnceTweenInit )
        {
            DOTween.Init( true, false, LogBehaviour.Default ).SetCapacity( 50, 20 );
            OnceTweenInit = true;
        }

        QualitySettings.maxQueuedFrames = 0;
        Cursor.visible = false;

        CreateFadeSprite();

        Camera.main.orthographicSize = ( Screen.height / ( GameSetting.PPU * 2f ) ) * GameSetting.PPU;
        
        NowPlaying.CurrentScene = this;
        KeyBind();
        ChangeAction( ActionType.Main );

        SoundManager.Inst.SetVolume( SoundManager.Inst.Volume, ChannelType.BGM );
        SoundManager.Inst.SetPaused( false, ChannelType.BGM );
    }

    protected virtual void Start()
    {
        Connect();
        StartCoroutine( FadeIn() );
    }

    protected virtual void OnDestroy()
    {
        Disconnect();
    }

    public void UpdatePitch( float _pitch )
    {
        SoundManager.Inst.SetPitch( _pitch, ChannelType.BGM );
        OnUpdatePitch?.Invoke( _pitch );
    }
    #endregion

    #region Scene Load
    public void LoadScene( SceneType _type )
    {
        StopAllCoroutines();
        SoundManager.Inst.StopFadeEffect();
        StartCoroutine( SceneChange( _type ) );
    }

    private IEnumerator SceneChange( SceneType _type )
    {
        DOTween.KillAll();
        DOTween.Clear();
        DOTween.ClearCachedTweens();
        
        IsInputLock = true;

        yield return StartCoroutine( FadeOut() );

        SoundManager.Inst.AllStop();
        SoundManager.Inst.PitchReset();
        SceneManager.LoadScene( ( int )_type );
    }
    #endregion

    #region Option Control
    protected void MoveToPrevOption( OptionController _controller )
    {
        _controller.PrevMove();
        SoundManager.Inst.Play( SoundSfxType.MenuSelect );
    }

    protected void MoveToNextOption( OptionController _controller )
    {
        _controller.NextMove();
        SoundManager.Inst.Play( SoundSfxType.MenuSelect );
    }

    protected void EnableCanvas( ActionType _changeType, OptionController _controller, IconController _icon = null, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    {
        GameObject root = _controller.transform.root.gameObject;
        root.SetActive( true );
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 0f;
            DOTween.To( () => 0f, x => group.alpha = x, 1f, Global.Const.OptionFadeDuration );
        }
        ChangeAction( _changeType );

        if ( _isSfxPlay )
             SoundManager.Inst.Play( SoundSfxType.MenuClick );

        if ( _hasFadeVolume )
             SoundManager.Inst.FadeVolume( SoundManager.Inst.GetVolume( ChannelType.BGM ), SoundManager.Inst.Volume * .5f, .5f );

        _icon?.Play();
    }

    protected void EnableCanvas( ActionType _changeType, GameObject _obj, IconController _icon = null, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    {
        GameObject root = _obj.transform.root.gameObject;
        root.SetActive( true );
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 0f;
            DOTween.To( () => 0f, x => group.alpha = x, 1f, Global.Const.OptionFadeDuration );
        }
        
        ChangeAction( _changeType );

        if ( _isSfxPlay )
             SoundManager.Inst.Play( SoundSfxType.MenuClick );

        if ( _hasFadeVolume )
             SoundManager.Inst.FadeVolume( SoundManager.Inst.GetVolume( ChannelType.BGM ), SoundManager.Inst.Volume * .5f, .5f );

        _icon?.Play();
    }

    protected void ImmediateDisableCanvas( ActionType _changeType, OptionController _controller )
    {
        _controller.transform.root.gameObject.SetActive( false );

        ChangeAction( _changeType );

        SoundManager.Inst.Play( SoundSfxType.MenuHover );
        SoundManager.Inst.FadeVolume( SoundManager.Inst.GetVolume( ChannelType.BGM ), SoundManager.Inst.Volume, .5f );
    }

    protected void DisableCanvas( ActionType _changeType, OptionController _controller, IconController _icon = null, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    {
        GameObject root = _controller.transform.root.gameObject;
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 1f;
            DOTween.To( () => 1f, x => group.alpha = x, 0f, Global.Const.OptionFadeDuration ).OnComplete( () => root.SetActive( false ) );
        }
        else
        {
            root.SetActive( false );
        }
        ChangeAction( _changeType );

        if ( _isSfxPlay )
             SoundManager.Inst.Play( SoundSfxType.MenuHover );

        if ( _hasFadeVolume )
             SoundManager.Inst.FadeVolume( SoundManager.Inst.GetVolume( ChannelType.BGM ), SoundManager.Inst.Volume, .5f );

        _icon?.Play();
    }

    protected void DisableCanvas( ActionType _changeType, GameObject _obj, IconController _icon = null, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    {
        GameObject root = _obj.transform.root.gameObject;
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 1f;
            DOTween.To( () => 1f, x => group.alpha = x, 0f, Global.Const.OptionFadeDuration ).OnComplete( () => root.SetActive( false ) );
        }
        else
        {
            root.SetActive( false );
        }
        
        ChangeAction( _changeType );

        if ( _isSfxPlay )
             SoundManager.Inst.Play( SoundSfxType.MenuHover );

        if ( _hasFadeVolume )
             SoundManager.Inst.FadeVolume( SoundManager.Inst.GetVolume( ChannelType.BGM ), SoundManager.Inst.Volume, .5f );

        _icon?.Play();
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
            GameSetting.ScrollSpeed += .1f;
        }
        else
        {
            if ( GameSetting.ScrollSpeed > 1.0001d )
                 SoundManager.Inst.Play( SoundSfxType.Slider );

            GameSetting.ScrollSpeed -= .1f;
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