using System.Collections;
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
    private float curHealth;
    private float healthCached;
    private float healthOffset;

    [Header("Health Scaler")]
    public float healthScalerSpeed = 1f;
    private Vector2 healthTileCached;
    private float healthTileOffset;
    private float healthScaleTimer;

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
    }

    private void Clear()
    {
        StopAllCoroutines();
        curHealth    = 0f;
        healthOffset = 0f;
        healthCached = MaxHealth;
        healthRenderer.size = new Vector2( healthTileCached.x, 0f );
    }

    private void GameStart()
    {
        StartCoroutine( InitHealthEffect() );
    }

    private IEnumerator InitHealthEffect()
    {
        StartCoroutine( AutoScaling() );
        while ( curHealth < MaxHealth )
        {
            curHealth += Time.deltaTime;
            yield return null;
        }

        curHealth = MaxHealth;
        StartCoroutine( SmoothHealthControl() );
    }

    private IEnumerator AutoScaling()
    {
        while ( true )
        {
            healthScaleTimer += healthScalerSpeed * Time.deltaTime;
            float scaleOffset   = ( Mathf.Cos( healthScaleTimer ) + 1f ) * .5f; // 0 ~ 1
            float curTileHeight = curHealth * healthTileOffset;
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
            curHealth += healthOffset * smoothHealthSpeed * Time.deltaTime;
            if ( healthOffset > 0f ? healthCached < curHealth : healthCached > curHealth )
                 curHealth = healthCached;

            if ( !isNoFailed && curHealth < 0f )
            {
                StartCoroutine( ( NowPlaying.CurrentScene as InGame ).GameOver() );
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
            case HitResult.Maximum: healthCached += .015f; break;
            case HitResult.Perfect: healthCached += .009f; break;
            case HitResult.Great:   healthCached -= .005f; break;
            case HitResult.Good:    healthCached -= .017f; break;
            case HitResult.Bad:     healthCached -= .028f; break;
            case HitResult.Miss:    healthCached -= .041f; break;
            default: return;
        }

        healthCached = Global.Math.Clamp( healthCached, -MaxHealth, MaxHealth );
        healthOffset = healthCached - curHealth;
    }
}
