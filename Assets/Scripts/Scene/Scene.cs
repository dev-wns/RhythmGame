using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public abstract class Scene : MonoBehaviour
{
    [SerializeField] private SceneFader sceneFader;
    [SerializeField] private UnityInputAction inputAction;
    private UnityInputHandler inputHandler;
    public SceneAction CurrentAction { get; private set; }
    public bool IsInputLock { get; set; }
    public event Action<UnityInputAction> OnKeyBind;

    protected virtual void Awake()
    {
        sceneFader.Initialize();

        Connect();

        float maxVol = AudioManager.Inst.GetVolume( ChannelType.BGM, true );
        AudioManager.Inst.SetVolume( maxVol, ChannelType.BGM );
        AudioManager.Inst.Pause = false;

        NowPlaying.CurrentScene = this;
    }

    protected async virtual void Start()
    {
        OnKeyBind?.Invoke( inputAction );
        ChangeAction( SceneAction.Main );

        await FadeIn();
    }

    protected virtual void OnDestroy()
    {
        Disconnect();
    }

    protected virtual void LateUpdate()
    {
        if ( !IsInputLock ) 
             inputHandler?.Process();
    }

    public void ChangeAction( SceneAction _action )
    {
        inputHandler  = inputAction.GetHandler( _action );
        CurrentAction = _action;
    }

    public async UniTask LoadScene( SceneType _type )
    {
        IsInputLock = true;

        DOTween.KillAll();
        DOTween.Clear();
        DOTween.ClearCachedTweens();

        float volume = AudioManager.Inst.GetVolume( ChannelType.BGM );
        await UniTask.WhenAll( FadeOut(), 
                               AudioManager.Inst.AsyncFadeGroup( ChannelType.BGM, volume, 0f, .75f ) );

        AudioManager.Inst.AllStop();
        AudioManager.Inst.PitchReset();
        await OnRelease();

        SceneManager.LoadScene( ( int ) _type );
        NowPlaying.CurrentScene = null;
    }

    protected async UniTask FadeIn() => await sceneFader.FadeIn();
    protected async UniTask FadeOut() => await sceneFader.FadeOut();

    protected abstract void Connect();
    protected abstract void Disconnect();
    protected virtual UniTask OnRelease() => UniTask.CompletedTask;


    //#region Option Control
    //protected static readonly float CanvasFadeDuration = .15f;

    //protected void MoveToPrevOption( OptionController _controller )
    //{
    //    _controller.PrevMove();
    //    AudioManager.Inst.Play( SFX.MenuSelect );
    //}

    //protected void MoveToNextOption( OptionController _controller )
    //{
    //    _controller.NextMove();
    //    AudioManager.Inst.Play( SFX.MenuSelect );
    //}

    //protected void EnableCanvas( SceneAction _changeType, OptionController _controller, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    //{
    //    GameObject root = _controller.transform.root.gameObject;
    //    root.SetActive( true );
    //    if ( root.TryGetComponent( out CanvasGroup group ) )
    //    {
    //        group.alpha = 0f;
    //        DOTween.To( () => 0f, x => group.alpha = x, 1f, CanvasFadeDuration );
    //    }
    //    ChangeAction( _changeType );

    //    if ( _isSfxPlay )
    //        AudioManager.Inst.Play( SFX.MenuClick );

    //    if ( _hasFadeVolume )
    //         AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, AudioManager.Inst.Volume * .5f, .5f );
    //}

    //protected void EnableCanvas( SceneAction _changeType, GameObject _obj, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    //{
    //    GameObject root = _obj.transform.root.gameObject;
    //    root.SetActive( true );
    //    if ( root.TryGetComponent( out CanvasGroup group ) )
    //    {
    //        group.alpha = 0f;
    //        DOTween.To( () => 0f, x => group.alpha = x, 1f, CanvasFadeDuration );
    //    }

    //    ChangeAction( _changeType );

    //    if ( _isSfxPlay )
    //         AudioManager.Inst.Play( SFX.MenuClick );

    //    if ( _hasFadeVolume )
    //         AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, AudioManager.Inst.Volume * .5f, .5f );
    //}

    //protected void ImmediateDisableCanvas( SceneAction _changeType, OptionController _controller )
    //{
    //    _controller.transform.root.gameObject.SetActive( false );

    //    ChangeAction( _changeType );

    //    AudioManager.Inst.Play( SFX.MenuExit );
    //    AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, 1f, .5f );
    //}

    //protected void DisableCanvas( SceneAction _changeType, OptionController _controller, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    //{
    //    GameObject root = _controller.transform.root.gameObject;
    //    if ( root.TryGetComponent( out CanvasGroup group ) )
    //    {
    //        group.alpha = 1f;
    //        DOTween.To( () => 1f, x => group.alpha = x, 0f, CanvasFadeDuration ).OnComplete( () => root.SetActive( false ) );
    //    }
    //    else
    //    {
    //        root.SetActive( false );
    //    }
    //    ChangeAction( _changeType );

    //    if ( _isSfxPlay )
    //        AudioManager.Inst.Play( SFX.MenuExit );

    //    if ( _hasFadeVolume )
    //         AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, 1f, .5f );
    //}

    //protected void DisableCanvas( SceneAction _changeType, GameObject _obj, bool _isSfxPlay = true, bool _hasFadeVolume = true )
    //{
    //    GameObject root = _obj.transform.root.gameObject;
    //    if ( root.TryGetComponent( out CanvasGroup group ) )
    //    {
    //        group.alpha = 1f;
    //        DOTween.To( () => 1f, x => group.alpha = x, 0f, CanvasFadeDuration ).OnComplete( () => root.SetActive( false ) );
    //    }
    //    else
    //    {
    //        root.SetActive( false );
    //    }

    //    ChangeAction( _changeType );

    //    if ( _isSfxPlay )
    //         AudioManager.Inst.Play( SFX.MenuExit );

    //    if ( _hasFadeVolume )
    //         AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, 1f, .5f );
    //}
    //#endregion
}