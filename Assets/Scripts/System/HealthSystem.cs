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
    private InGame scene;
    private Judgement judge;

    [Header("Health")]
    public SpriteRenderer healthRenderer;
    public static readonly float MaxHealth = 1f;
    public float smoothHealthControlSpeed = 1;
    private float curHealth, healthCached;
    private float healthOffset;

    [Header("Health Scaler")]
    public float healthScalerSpeed = 1f;
    private Vector2 healthTileCached;
    private float healthTileOffset;
    private float healthScaleTimer;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += Clear;
        scene.OnGameStart += () => StartCoroutine( InitHealthEffect() );

        //judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        //judge.OnJudge += HealthUpdate;

        InputManager.OnHitNote += UpdateHealth;

        healthTileCached = healthRenderer.size;
        healthTileOffset = healthRenderer.size.y;

        Clear();
    }

    private void OnDestroy()
    {
        InputManager.OnHitNote -= UpdateHealth;
    }

    private IEnumerator InitHealthEffect()
    {
        StartCoroutine( SmoothHealthScaler() );
        while ( curHealth < MaxHealth )
        {
            curHealth += Time.deltaTime;
            yield return null;
        }

        curHealth = MaxHealth;
        StartCoroutine( SmoothHealthControl() );
    }

    private IEnumerator SmoothHealthScaler()
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
            curHealth += healthOffset * smoothHealthControlSpeed * Time.deltaTime;
            if ( healthOffset > 0f ? healthCached < curHealth : healthCached > curHealth )
                curHealth = healthCached;

            if ( !isNoFailed && curHealth < 0f )
            {
                StartCoroutine( scene.GameOver() );
                break;
            }
            yield return null;
        }
    }

    private void Clear()
    {
        StopAllCoroutines();
        healthOffset = 0f;
        curHealth = 0f;
        healthCached = MaxHealth;
        healthRenderer.size = new Vector2( healthTileCached.x, 0f );
    }

    private void UpdateHealth( HitData _hitData )
    {
        HitResult hitResult = _hitData.hitResult;
        switch ( hitResult )
        {
            case HitResult.Maximum: healthCached += .017f; break;
            case HitResult.Perfect: healthCached += .011f; break;
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
