using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    private InGame scene;
    private Judgement judge;
    private float curHealth;
    public static readonly float MaxHealth = 100f;
    public  event Action<float/* Health */> OnChangedHealth;
    private event Action OnFailed;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += ReLoad;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += HealthUpdate;

        curHealth = MaxHealth;
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
