using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

public abstract class CanvasActivator : MonoBehaviour
{
    protected Scene CurrentScene { get; private set; }

    [SerializeField] private Canvas canvas;
    private CanvasGroup canvasGroup;

    [Header( "[Enable Canvas]" )]
    [SerializeField] private SceneAction enableAction;
    [SerializeField] private SFX enableSFX;

    [Header( "[Disable Canvas]" )]
    [SerializeField] private SceneAction disableAction;
    [SerializeField] private SFX disableSFX;

    [Header( "[Effect]" )]
    [SerializeField] private bool isFadeVolume = true;

    protected static readonly float FadeDuration = .15f;

    protected virtual void Awake()
    {
        CurrentScene = GetComponentInParent<Scene>();
        if ( CurrentScene == null )
        {
            Debug.LogError( "[Canvas Activator] Scene not found in parent chain." );
            return;
        }

        if ( canvas )
        {
            if ( canvas.TryGetComponent( out canvasGroup ) )
                 canvasGroup.alpha = 0f;
        }

        CurrentScene.OnKeyBind += KeyBind;
    }

    protected virtual void OnDestroy()
    {
        if ( CurrentScene )
             CurrentScene.OnKeyBind -= KeyBind;
    }

    protected abstract void KeyBind( UnityInputAction _input );

    public virtual void ImmediateEnableCanvas()
    {
        canvas.gameObject.SetActive( true );
        CurrentScene.ChangeAction( enableAction );
    }

    public virtual void EnableCanvas()
    {
        canvas.gameObject.SetActive( true );
        if ( canvasGroup )
        {
            DOTween.Kill( canvasGroup );
            canvasGroup.DOFade( 1f, FadeDuration );
        }
        CurrentScene.ChangeAction( enableAction );

        if ( enableSFX != SFX.None )
             AudioManager.Inst.Play( enableSFX );

        if ( isFadeVolume )
             AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, AudioManager.Inst.Volume * .5f, .5f );
    }

    public virtual async UniTask EnableCanvasAsync( CancellationToken _token )
    {
        canvas.gameObject.SetActive( true );
        if ( canvasGroup )
        {
            DOTween.Kill( canvasGroup );
            Tween tween = canvasGroup.DOFade( 1f, FadeDuration );
            await tween.ToUniTask( TweenCancelBehaviour.Kill, _token );
        }
        CurrentScene.ChangeAction( enableAction );

        if ( enableSFX != SFX.None )
            AudioManager.Inst.Play( enableSFX );

        if ( isFadeVolume )
            AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, AudioManager.Inst.Volume * .5f, .5f );
    }

    public virtual void ImmediateDisableCanvas()
    {
        canvas.gameObject.SetActive( false );
        CurrentScene.ChangeAction( disableAction );
    }

    public virtual void DisableCanvas()
    {
        if ( canvasGroup )
        {
            DOTween.Kill( canvasGroup );
            canvasGroup.DOFade( 0f, FadeDuration ).OnComplete( () => canvas.gameObject.SetActive( false ) );
        }
        CurrentScene.ChangeAction( disableAction );

        if ( disableSFX != SFX.None )
             AudioManager.Inst.Play( disableSFX );

        if ( isFadeVolume )
             AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, 1f, .5f );
    }

    public virtual async UniTask DisableCanvasAsync( CancellationToken _token = default )
    {
        if ( canvasGroup )
        {
            DOTween.Kill( canvasGroup );
            Tween tween = canvasGroup.DOFade( 0f, FadeDuration ).OnComplete( () => canvas.gameObject.SetActive( false ) );
            await tween.ToUniTask( TweenCancelBehaviour.Kill, _token );
        }
        CurrentScene.ChangeAction( disableAction );

        if ( disableSFX != SFX.None )
             AudioManager.Inst.Play( disableSFX );

        if ( isFadeVolume )
             AudioManager.Inst.Fade( AudioManager.Inst.MainChannel, AudioManager.Inst.Volume, 1f, .5f );
    }
}
