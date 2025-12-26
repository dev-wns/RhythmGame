using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent( typeof( SpriteRenderer ) )]
public abstract class Scene : UnityInputAction
{
    protected abstract void Connect();

    protected abstract void Disconnect();

    protected virtual UniTask OnRelease() => UniTask.CompletedTask;

    protected virtual void Awake()
    {
        Connect();
        CreateFadeSprite();

        NowPlaying.CurrentScene = this;
        KeyBind();
        ChangeAction( SceneAction.Main );

        float maxVol = AudioManager.Inst.GetVolume( ChannelType.BGM, true );
        AudioManager.Inst.SetVolume( maxVol, ChannelType.BGM );

        AudioManager.Inst.Pause = false;
    }

    protected async virtual void Start()
    {
        await FadeIn();
    }

    protected virtual void OnDestroy()
    {
        Disconnect();
    }

    public async UniTask LoadScene( SceneType _type )
    {
        DOTween.KillAll();
        DOTween.Clear();
        DOTween.ClearCachedTweens();

        IsInputLock = true;

        float volume = AudioManager.Inst.GetVolume( ChannelType.BGM );
        await UniTask.WhenAll( FadeOut(), 
                               AudioManager.Inst.AsyncFadeGroup( ChannelType.BGM, volume, 0f, .75f ) );

        AudioManager.Inst.AllStop();
        AudioManager.Inst.PitchReset();
        await OnRelease();
        SceneManager.LoadScene( ( int ) _type );
    }

    #region Option Control
    protected static readonly float CanvasFadeDuration = .15f;

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

    protected void EnableCanvas( SceneAction _changeType, OptionController _controller, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    {
        GameObject root = _controller.transform.root.gameObject;
        root.SetActive( true );
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 0f;
            DOTween.To( () => 0f, x => group.alpha = x, 1f, CanvasFadeDuration );
        }
        ChangeAction( _changeType );

        if ( _isSfxPlay )
            AudioManager.Inst.Play( SFX.MenuClick );

        if ( _hasFadeVolume )
             AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, AudioManager.Inst.Volume * .5f, .5f );
    }

    protected void EnableCanvas( SceneAction _changeType, GameObject _obj, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    {
        GameObject root = _obj.transform.root.gameObject;
        root.SetActive( true );
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 0f;
            DOTween.To( () => 0f, x => group.alpha = x, 1f, CanvasFadeDuration );
        }

        ChangeAction( _changeType );

        if ( _isSfxPlay )
             AudioManager.Inst.Play( SFX.MenuClick );

        if ( _hasFadeVolume )
             AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, AudioManager.Inst.Volume * .5f, .5f );
    }

    protected void ImmediateDisableCanvas( SceneAction _changeType, OptionController _controller )
    {
        _controller.transform.root.gameObject.SetActive( false );

        ChangeAction( _changeType );

        AudioManager.Inst.Play( SFX.MenuExit );
        AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, 1f, .5f );
    }

    protected void DisableCanvas( SceneAction _changeType, OptionController _controller, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    {
        GameObject root = _controller.transform.root.gameObject;
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 1f;
            DOTween.To( () => 1f, x => group.alpha = x, 0f, CanvasFadeDuration ).OnComplete( () => root.SetActive( false ) );
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

    protected void DisableCanvas( SceneAction _changeType, GameObject _obj, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    {
        GameObject root = _obj.transform.root.gameObject;
        if ( root.TryGetComponent( out CanvasGroup group ) )
        {
            group.alpha = 1f;
            DOTween.To( () => 1f, x => group.alpha = x, 0f, CanvasFadeDuration ).OnComplete( () => root.SetActive( false ) );
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
    public event Action OnChangedSpeed;

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
            if ( GameSetting.ScrollSpeed >= 50 )
                 return;

            AudioManager.Inst.Play( SFX.Slider );
            GameSetting.ScrollSpeed += 1;
        }
        else
        {
            if ( GameSetting.ScrollSpeed <= 1 )
                 return;

            AudioManager.Inst.Play( SFX.Slider );
            GameSetting.ScrollSpeed -= 1;
        }

        OnChangedSpeed?.Invoke();
    }
    #endregion

    #region Fade
    private SpriteRenderer blackSprite;
    private static readonly float FadeTime     = .65f;
    private static readonly float FadeWaitTime = .025f;

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

    protected async UniTask FadeIn()
    {
        blackSprite.color = Color.black;
        blackSprite.enabled = true;

        await UniTask.WaitForSeconds( FadeWaitTime );
        await blackSprite.DOFade( 0f, FadeTime + ( FadeWaitTime * 2 ) );

        blackSprite.color = Color.clear;
        blackSprite.enabled = false;
    }

    protected async UniTask FadeOut()
    {
        blackSprite.color = Color.clear;
        blackSprite.enabled = true;

        await UniTask.WaitForSeconds( FadeWaitTime );
        await blackSprite.DOFade( 1f, FadeTime + ( FadeWaitTime * 2 ) );

        blackSprite.color = Color.black;
    }
    #endregion
}