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

    private int prevMaxCount = 1, curMaxCount;
    private double curRate, prevRate;
    private int prevNum, curNum;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.Reverse();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += RateUpdate;
    }

    private void Start()
    {
        StartCoroutine( UpdateImage() );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private IEnumerator UpdateImage()
    {
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .05f );
            if ( prevRate == curRate )
                 continue;

            prevNum = curNum;

            double calcCurRate  = Mathf.RoundToInt( ( float )( curRate / curMaxCount ) );
            double calcPrevRate = Mathf.RoundToInt( ( float )( prevRate / prevMaxCount ) );

            curNum = Globals.Log10( calcCurRate ) + 1;

            for ( int i = 3; i < images.Count; i++ )
            {
                if ( i >= curNum )
                {
                    if ( images[i].gameObject.activeInHierarchy )
                         images[i].gameObject.SetActive( false );
                }
                else
                {
                    if ( !images[i].gameObject.activeInHierarchy )
                         images[i].gameObject.SetActive( true );
                }
            }

            for ( int i = 0; i < images.Count; i++ )
            {
                if ( ( int )calcPrevRate % 10 == ( int )calcCurRate % 10 )
                     break;

                images[i].sprite = sprites[( int )calcCurRate % 10];
                calcCurRate *= .1d;
                calcPrevRate *= .1d;
            }

            prevRate     = curRate;
            prevMaxCount = curMaxCount;

            if ( prevNum != curNum )
                 layoutGroup.SetLayoutHorizontal();
        }
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
        ++curMaxCount;
        curRate += addRate;
    }
}
