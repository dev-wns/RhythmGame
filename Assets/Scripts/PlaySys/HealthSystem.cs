using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public Transform left, right;
    private Judgement judge;
    private float curHealth;
    public static readonly float MaxHealth = 100f;
    public  event Action<float/* Health */> OnChangedHealth;
    private event Action OnFailed;

    private void Awake()
    {
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += HealthUpdate;

        curHealth = MaxHealth;
    }

    private void HealthUpdate( HitResult _type )
    {
        float offset = 0f;
        switch ( _type )
        {
            case HitResult.Perfect:     offset = 5f;  break;
            case HitResult.Great:       offset = 3f;   break;
            case HitResult.Good:        offset = 1f;   break;
            case HitResult.Bad:         offset = -3f;  break;
            case HitResult.Miss:        offset = -5f; break;
        }

        curHealth = Globals.Clamp( curHealth + offset, 0f, MaxHealth );
        OnChangedHealth?.Invoke( curHealth );

        if ( curHealth < 0f ) 
             OnFailed?.Invoke();
    }
}
