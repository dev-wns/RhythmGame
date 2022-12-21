using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectSystem : MonoBehaviour
{
    private List<BaseEffect> effects = new List<BaseEffect>();
    [SerializeField] private bool isStart;
    [SerializeField] private int currentIndex;

    private void Update()
    {
        if ( !isStart || effects.Count == 0 )
             return;

        // true : 해당 작업이 끝남.
        if ( effects[currentIndex].Process() )
        {
            if ( currentIndex + 1 < effects.Count )
            {
                currentIndex++;
            }
            else
                isStart = false;
        }
    }

    public void Restart()
    {
        isStart = true;
        for ( int i = 0; i <= currentIndex; i++ )
        {
            effects[i].Restart();
        }
        currentIndex = 0;
    }

    public EffectSystem Append( BaseEffect _info )
    {
        effects.Add( _info );
        return this;
    }

    public EffectSystem AppendInterval( float _t )
    {
        effects.Add( new EffectInterval( _t ) );
        return this;
    }

    public EffectSystem OnCompleted( Action _action )
    {
        if ( effects.Count > 0 )
             effects[effects.Count - 1].OnCompleted = _action;

        return this;
    }
}