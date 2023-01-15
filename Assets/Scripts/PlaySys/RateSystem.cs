using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RateSystem : MonoBehaviour
{
    [Header("Sprite")]
    public int sortingOrder;

    [Header("RateSystem")]
    private InGame scene;
    private Judgement judge;
    private CustomHorizontalLayoutGroup layoutGroup;
    public List<Sprite> sprites = new List<Sprite>();
    public List<SpriteRenderer> images = new List<SpriteRenderer>();

    private int curMaxCount;
    private double curRate;
    private int prevNum, curNum;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += ReLoad;
        scene.OnResult += Result;
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.Reverse();
        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += RateUpdate;

        for ( int i = 0; i < images.Count; i++ )
              images[i].sortingOrder = sortingOrder;
    }

    private void Result() => judge.SetResult( HitResult.Rate, ( int )( curRate / curMaxCount ) );

    private void ReLoad()
    {
        curMaxCount = 0;
        curRate = 0d;
        curNum = prevNum = 0;

        images[images.Count - 1].gameObject.SetActive( true );
        images[images.Count - 1].sprite = sprites[1];
        for ( int i = 0; i < images.Count - 1; i++ )
        {
            images[i].gameObject.SetActive( true );
            images[i].sprite = sprites[0];
        }
        layoutGroup.SetLayoutHorizontal();
    }

    private void RateUpdate( HitResult _result, NoteType _type )
    {
        if ( _result == HitResult.None ) return;

        switch ( _result )
        {
            case HitResult.None:
            case HitResult.Fast:
            case HitResult.Slow: 
            return;

            case HitResult.Maximum:
            case HitResult.Perfect: curRate += 10000d; break; 
            case HitResult.Great:   curRate += 9000d;  break; 
            case HitResult.Good:    curRate += 8000d;  break; 
            case HitResult.Bad:     curRate += 7000d;  break; 
            case HitResult.Miss:    curRate += .0001d; break; 
        }
        ++curMaxCount;

        double calcCurRate  = curRate / curMaxCount;
        curNum = Global.Math.Log10( calcCurRate ) + 1;
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
