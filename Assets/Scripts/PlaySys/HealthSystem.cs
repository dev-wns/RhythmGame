using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public Transform left, right;
    private Judgement judge;
    private float currentHealth;
    public static readonly float MaxHealth = 100f;
    public  event Action<float/* Health */> OnChangedHealth;
    private event Action OnFailed;

    private void Awake()
    {
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += HealthUpdate;

        currentHealth = MaxHealth;
    }

    private void HealthUpdate( JudgeType _type )
    {
        float offset = 0f;
        switch ( _type )
        {
            case JudgeType.Perfect:     offset = 10f;  break;
            case JudgeType.LazyPerfect: offset = 7f;   break;
            case JudgeType.Great:       offset = 5f;   break;
            case JudgeType.Good:        offset = 3f;   break;
            case JudgeType.Bad:         offset = -5f;  break;
            case JudgeType.Miss:        offset = -10f; break;
        }

        currentHealth = Globals.Clamp( currentHealth + offset, 0f, MaxHealth );
        OnChangedHealth?.Invoke( currentHealth );

        if ( currentHealth < 0f ) 
             OnFailed?.Invoke();
    }
}
