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

    private int curMaxCount;
    private double curRate;
    private int prevNum, curNum;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.Reverse();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += RateUpdate;
    }

    private void RateUpdate( HitResult _type )
    {
        if ( _type == HitResult.None ) return;

        ++curMaxCount;
        switch ( _type )
        {
            case HitResult.Perfect: curRate += 10000d; break; 
            case HitResult.Great:   curRate += 9000d;  break; 
            case HitResult.Good:    curRate += 8000d;  break; 
            case HitResult.Bad:     curRate += 7000d;  break; 
            case HitResult.Miss:    curRate += .0001d; break; 
        }

        double calcCurRate  = curRate / curMaxCount;
        curNum = calcCurRate == 0 ? 1 : Globals.Log10( calcCurRate ) + 1;

        for ( int i = 3; i < images.Count; i++ )
        {
            if ( i >= curNum )
            {
                if ( images[i].gameObject.activeSelf )
                     images[i].gameObject.SetActive( false );
            }
            else
            {
                if ( !images[i].gameObject.activeSelf )
                     images[i].gameObject.SetActive( true );
            }
        }

        for ( int i = 0; i < images.Count; i++ )
        {
            if ( i == curNum ) break;

            images[i].sprite = sprites[( int )calcCurRate % 10];
            calcCurRate  *= .1d;
        }

        if ( prevNum != curNum )
             layoutGroup.SetLayoutHorizontal();

        prevNum = curNum;
    }
}
