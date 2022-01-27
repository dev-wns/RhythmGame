using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RateSystem : MonoBehaviour
{
    private Judgement judge;
    private CustomHorizontalLayoutGroup layoutGroup;
    public List<Sprite> sprites = new List<Sprite>();
    public List<SpriteRenderer> images = new List<SpriteRenderer>();

    private int maxCount;
    private double curRate, prevRate;
    private int prevNum, curNum;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.Reverse();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += RateUpdate;
    }

    private void FixedUpdate()
    {
        if ( prevRate == curRate )
             return;

        prevRate = curRate;
        prevNum  = curNum;

        double calcRate = Mathf.RoundToInt( ( float )( curRate / maxCount ) );
        curNum = Globals.Log10( calcRate ) + 1;

        for ( int i = 0; i < images.Count; i++ )
        {
            if ( i > 2 && i >= curNum )
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

        if ( prevNum != curNum )
            layoutGroup.SetLayoutHorizontal();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void RateUpdate( JudgeType _type )
    {
        if ( _type == JudgeType.None ) return;

        double addRate = 0d;
        switch ( _type )
        {
            case JudgeType.Perfect: 
            case JudgeType.LatePerfect: addRate = 10000d; break; 
            case JudgeType.Great:       addRate = 9000d;  break; 
            case JudgeType.Good:        addRate = 8000d;  break; 
            case JudgeType.Bad:         addRate = 7000d;  break; 
            case JudgeType.Miss:        addRate = .0001d; break; 
        }
        ++maxCount;
        curRate += addRate;
    }
}
