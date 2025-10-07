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
    private float health;       // 바로바로 변동되는 health 값
    private float healthAmount; // UI에서 서서히 증감하며 보여지는 health 값
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
        healthAmount = 0f;
        healthOffset = 0f;
        health       = MaxHealth;
        healthRenderer.size = new Vector2( healthTileCached.x, 0f );
    }

    private void GameStart()
    {
        StartCoroutine( InitHealthEffect() );
    }

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
                StartCoroutine( ( NowPlaying.CurrentScene as InGame ).GameOver() );
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
