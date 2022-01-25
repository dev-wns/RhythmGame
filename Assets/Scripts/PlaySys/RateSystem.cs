using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RateSystem : MonoBehaviour
{
    private Judgement judge;
    private CustomHorizontalLayoutGroup layoutGroup;
    public List<Image> images = new List<Image>();
    public List<Sprite> sprites = new List<Sprite>();


    private int maxCount;
    private double currentRate, previousRate;
    private NumberBit previousBit, currentBit;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.Reverse();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += RateUpdate;

        StartCoroutine( RateProcess() );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private IEnumerator RateProcess()
    {
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .1f );

            if ( previousRate == currentRate ) 
                 continue;

            previousRate = currentRate;
            previousBit  = currentBit;
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

                    currentBit = ( NumberBit )( 1 << i );
                }
            }

            if ( previousBit != currentBit )
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
            case JudgeType.LazyPerfect: addRate = 10000d; break; 
            case JudgeType.Great:       addRate = 9000d;  break; 
            case JudgeType.Good:        addRate = 8000d;  break; 
            case JudgeType.Bad:         addRate = 7000d;  break; 
            case JudgeType.Miss:        addRate = .0001d; break; 
        }
        ++maxCount;
        currentRate += addRate;
    }
}
