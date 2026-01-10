using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using DG.Tweening;


public abstract class Scene : MonoBehaviour
{
    [SerializeField] private SceneFader sceneFader;
    private UnityInputAction  inputAction = new (); // Action Map
    private UnityInputHandler inputHandler;         // Actor
    public SceneAction CurrentAction { get; private set; }
    public bool IsInputLock { get; set; }
    public event Action<UnityInputAction> OnKeyBind;

    public async UniTask LoadScene( SceneType _type )
    {
        IsInputLock = true;

        DOTween.KillAll();
        DOTween.Clear();
        DOTween.ClearCachedTweens();

        float volume = AudioManager.Inst.GetVolume( ChannelType.BGM );
        await UniTask.WhenAll( FadeOut(),
                               AudioManager.Inst.FadeGroupAsync( ChannelType.BGM, volume, 0f, .75f ) );

        AudioManager.Inst.AllStop();
        AudioManager.Inst.PitchReset();
        await Release();

        SceneManager.LoadScene( ( int ) _type );
        NowPlaying.CurrentScene = null;
    }

    protected virtual void Awake()
    {
        NowPlaying nowPlaying = NowPlaying.Inst;
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

    protected async virtual void OnDestroy()
    {
        Disconnect();
        await Release();
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

    protected async UniTask FadeIn()  => await sceneFader.FadeIn();
    protected async UniTask FadeOut() => await sceneFader.FadeOut();

    protected abstract void Connect();
    protected abstract void Disconnect();
    protected virtual UniTask Release() => UniTask.CompletedTask;
}