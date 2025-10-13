using Cysharp.Threading.Tasks;
using System.Collections;
using System.Threading;
using UnityEngine;

public struct HealthData
{
    public double time;
    public float  health;

    public HealthData( double _time, float _health )
    {
        time = _time;
        health = _health;
    }
}

public class HealthSystem : MonoBehaviour
{
    [Header("Health")]
    public SpriteRenderer healthRenderer;
    public static readonly float MaxHealth = 1f;
    public float smoothHealthSpeed = 1;
    private float health;       // 바로바로 변동되는 health 값
    private float healthAmount; // UI에서 서서히 증감하며 보여지는 health 값
    private float healthOffset;

    [Header("Health Scaler")]
    public float healthScalerSpeed = 1f;
    private Vector2 healthTileCached;
    private float healthTileOffset;
    private float healthScaleTimer;

    //[Header( "Asynchronous" )]
    //private CancellationTokenSource breakPoint;

    private void Awake()
    {
        NowPlaying.OnGameStart += GameStart;
        NowPlaying.OnClear     += Clear;
        InputManager.OnHitNote += UpdateHealth;

        healthTileCached = healthRenderer.size;
        healthTileOffset = healthRenderer.size.y;
    }

    private void OnDestroy()
    {
        NowPlaying.OnGameStart -= GameStart;
        NowPlaying.OnClear     -= Clear;
        InputManager.OnHitNote -= UpdateHealth;

        //breakPoint?.Cancel();
        //breakPoint?.Dispose();
    }

    private void Clear()
    {
        StopAllCoroutines();

        //breakPoint?.Cancel();
        //breakPoint?.Dispose();
        //breakPoint = null;

        healthAmount = 0f;
        healthOffset = 0f;
        health       = MaxHealth;
        healthRenderer.size = new Vector2( healthTileCached.x, 0f );
    }

    private void GameStart()
    {
        //breakPoint = new CancellationTokenSource();
        //await HealthEffect( breakPoint.Token );

        StartCoroutine( InitHealthEffect() );
    }

    //private async UniTask HealthEffect( CancellationToken _token )
    //{
    //    while ( healthAmount < MaxHealth )
    //    {
    //        healthAmount += Time.deltaTime;
    //        await UniTask.Yield( PlayerLoopTiming.Update, _token );
    //    }

    //    healthAmount = MaxHealth;
    //    StartCoroutine( AutoScaling() );

    //    bool isNoFailed = GameSetting.CurrentGameMode.HasFlag( GameMode.NoFail );
    //    while ( !_token.IsCancellationRequested )
    //    {
    //        healthAmount += healthOffset * smoothHealthSpeed * Time.deltaTime;
    //        if ( healthOffset > 0f ? health < healthAmount : health > healthAmount )
    //             healthAmount = health;

    //        if ( !isNoFailed && health < 0f )
    //        {
    //            await NowPlaying.Inst.GameOver();
    //            while ( healthAmount > -MaxHealth )
    //            {
    //                healthAmount -= Time.deltaTime;
    //                await UniTask.Yield( PlayerLoopTiming.Update, _token );
    //            } break;
    //        }

    //        await UniTask.Yield( PlayerLoopTiming.Update, _token );
    //    }
    //}

    private IEnumerator InitHealthEffect()
    {
        StartCoroutine( AutoScaling() );
        while ( healthAmount < MaxHealth )
        {
            healthAmount += Time.deltaTime;
            yield return null;
        }

        healthAmount = MaxHealth;
        StartCoroutine( SmoothHealthControl() );
    }

    private IEnumerator AutoScaling()
    {
        while ( true )
        {
            healthScaleTimer += healthScalerSpeed * Time.deltaTime;
            float scaleOffset   = ( Mathf.Cos( healthScaleTimer ) + 1f ) * .5f; // 0 ~ 1
            float curTileHeight = healthAmount * healthTileOffset;
            float height        = curTileHeight - Global.Math.Lerp( curTileHeight * .04f, 0f, scaleOffset );
            healthRenderer.size = new Vector2( healthTileCached.x, Global.Math.Clamp( height, 0f, healthTileCached.y ) );
            yield return null;
        }
    }

    private IEnumerator SmoothHealthControl()
    {
        bool isNoFailed = GameSetting.CurrentGameMode.HasFlag( GameMode.NoFail );
        while ( true )
        {
            healthAmount += healthOffset * smoothHealthSpeed * Time.deltaTime;
            if ( healthOffset > 0f ? health < healthAmount : health > healthAmount )
                 healthAmount = health;

            if ( !isNoFailed && health < 0f )
            {
                StartCoroutine( NowPlaying.Inst.GameOver().ToCoroutine() );
                while ( healthAmount > -MaxHealth )
                {
                    healthAmount -= Time.deltaTime;
                    yield return null;
                }
                break;
            }
            yield return null;
        }
    }

    private void UpdateHealth( HitData _hitData )
    {
        HitResult hitResult = _hitData.hitResult;
        switch ( hitResult )
        {
            case HitResult.Maximum: health += .015f; break;
            case HitResult.Perfect: health += .009f; break;
            case HitResult.Great:   health += .005f; break;
            case HitResult.Good:    health -= .007f; break;
            case HitResult.Bad:     health -= .017f; break;
            case HitResult.Miss:    health -= .041f; break;
            default: return;
        }

        health       = Global.Math.Clamp( health, -MaxHealth, MaxHealth );
        healthOffset = health - healthAmount;
    }
}
