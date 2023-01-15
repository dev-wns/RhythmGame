using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    private InGame scene;
    private Judgement judge;
    public Transform helpTransform;

    private event Action OnFailed;

    [Header("Health")]
    public Transform healthBGTransform;
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
        scene.OnReLoad += Initialize;
        scene.OnGameStart += () => StartCoroutine( InitHealthEffect() );

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += HealthUpdate;

        healthTileCached = healthRenderer.size;
        healthTileOffset = healthRenderer.size.y;

        helpTransform.position            = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 5f,  ( -Screen.height * .5f ) + 50f, 0f );
        healthBGTransform.position        = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 17f, ( -Screen.height * .5f ) + ( helpTransform.localScale.y * .5f ), 0f );
        healthRenderer.transform.position = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 33f, ( -Screen.height * .5f ) + helpTransform.localScale.y, 0f );

        Initialize();
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
            healthScaleTimer   += healthScalerSpeed * Time.deltaTime;
            float scaleOffset   = ( Mathf.Cos( healthScaleTimer ) + 1f ) * .5f; // 0 ~ 1
            float curTileHeight =  curHealth * healthTileOffset;
            float height        = curTileHeight - Global.Math.Lerp( curTileHeight * .04f, 0f, scaleOffset );
            healthRenderer.size = new Vector2( healthTileCached.x, Global.Math.Clamp( height, 0f, healthTileCached.y ) );
            yield return null;
        }
    }

    private IEnumerator SmoothHealthControl()
    {
        while ( curHealth > 0f )
        {
            curHealth += healthOffset * smoothHealthControlSpeed * Time.deltaTime;
            if ( healthOffset > 0f ? healthCached < curHealth :  healthCached > curHealth )
                 curHealth = healthCached;
            yield return null;
        }

        OnFailed?.Invoke();
    }

    private void Initialize()
    {
        StopAllCoroutines();
        curHealth    = 0f;
        healthOffset = 0f;
        healthCached = MaxHealth;
        healthRenderer.size = new Vector2( healthTileCached.x, 0f );
    }

    private void HealthUpdate( HitResult _result, NoteType _type )
    {
        switch ( _result )
        {
            case HitResult.Maximum:     healthCached += .017f; break;
            case HitResult.Perfect:     healthCached += .011f; break;
            case HitResult.Great:       healthCached += .005f; break;
            case HitResult.Good:        healthCached -= .019f; break;
            case HitResult.Bad:         healthCached -= .027f; break;
            case HitResult.Miss:        healthCached -= .055f; break;
        }
        healthCached = Global.Math.Clamp( healthCached, -MaxHealth, MaxHealth );
        healthOffset = healthCached - curHealth;
    }
}
