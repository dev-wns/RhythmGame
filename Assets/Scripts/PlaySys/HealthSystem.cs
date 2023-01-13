using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    private InGame scene;
    private Judgement judge;
    public Transform helpTransform;

    public  event Action<float/* Health */> OnChangedHealth;
    private event Action OnFailed;

    [Header("Health")]
    public Transform healthBGTransform;
    public SpriteRenderer healthRenderer;
    private float curHealth;
    public static readonly float MaxHealth = 100f;

    [Header("Health Scaler")]
    public float healthScalerSpeed = 1f;
    private Vector2 healthTileCached;
    private float healthTileOffset;
    private float healthScaleTimer;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += ReLoad;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += HealthUpdate;

        curHealth = MaxHealth;
    }

    private void Start()
    {
        helpTransform.position            = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 5f,  ( -Screen.height * .5f ) + 50f, 0f );
        healthBGTransform.position        = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 15f, ( -Screen.height * .5f ) + ( helpTransform.localScale.y * .5f ), 0f );
        healthRenderer.transform.position = new Vector3( GameSetting.GearStartPos + GameSetting.GearWidth + 31f, ( -Screen.height * .5f ) + helpTransform.localScale.y, 0f );
        healthTileCached = healthRenderer.size;
        healthTileOffset = healthRenderer.size.y * .01f;
    }

    private void Update()
    {
        healthScaleTimer += Time.deltaTime * healthScalerSpeed;
        float scaleOffset   = ( Mathf.Cos( healthScaleTimer ) + 1f ) *.5f; // 0 ~ 1
        float curTileHeight =  curHealth * healthTileOffset;
        float height        = curTileHeight - Global.Math.Lerp( curTileHeight * .035f, 0f, scaleOffset );
        healthRenderer.size = new Vector2( healthTileCached.x, Global.Math.Clamp( height, 0f, healthTileCached.y ) );
    }

    private void ReLoad()
    {
        curHealth = MaxHealth;
    }

    private void HealthUpdate( HitResult _result, NoteType _type )
    {
        float offset = 0f;
        switch ( _result )
        {
            case HitResult.Perfect:     offset = 5f;  break;
            case HitResult.Great:       offset = 3f;  break;
            case HitResult.Good:        offset = 1f;  break;
            case HitResult.Bad:         offset = -3f; break;
            case HitResult.Miss:        offset = -5f; break;
        }

        curHealth = Global.Math.Clamp( curHealth + offset, 0f, MaxHealth );
        OnChangedHealth?.Invoke( curHealth );

        if ( curHealth < 0f ) 
             OnFailed?.Invoke();
    }
}
