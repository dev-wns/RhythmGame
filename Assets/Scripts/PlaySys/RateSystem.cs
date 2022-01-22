using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RateSystem : NumberAtlasBase
{
    [Header( "System" )]
    private Judgement judge;

    private int maxCount;
    private double currentRate;

    protected override void Awake()
    {
        base.Awake();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += RateImageUpdate;
    }

    private void RateImageUpdate( JudgeType _type )
    {
        if ( _type == JudgeType.None ) return;

        double addRate = 0d;
        switch ( _type )
        {
            case JudgeType.Perfect: 
            case JudgeType.LazyPerfect: addRate = 10000d; break; 
            case JudgeType.Great:       addRate = 7500d;  break; 
            case JudgeType.Good:        addRate = 5000d;  break; 
            case JudgeType.Bad:         addRate = 2500d;  break; 
            case JudgeType.Miss:        addRate = 0d;     break; 
        }
        ++maxCount;
        currentRate += addRate;

        double calcRate = Mathf.RoundToInt( ( float )( currentRate / maxCount ) );
        int num = Globals.Log10( calcRate ) + 1;

        for ( int i = 0; i < images.Count; i++ )
        {
            if ( i > 2 && i >= num )
            {
                if ( images[i].gameObject.activeInHierarchy )
                     images[i].gameObject.SetActive( false );
            }
            else
            {
                if ( !images[i].gameObject.activeInHierarchy )
                     images[i].gameObject.SetActive( true );

                images[i].sprite = sprites[( int )calcRate % 10];
                calcRate *= .1d;
            }
        }
    }
}
