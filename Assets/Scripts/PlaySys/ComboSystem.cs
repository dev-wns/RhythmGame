using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ComboSystem : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private CustomHorizontalLayoutGroup layoutGroup;
    private Judgement judge;
    private int prevCombo = -1, curCombo;

    private Sequence sequence;
    private int prevNum, curNum;
    private Transform tf;
    private Vector2 posCache;

    private void Awake()
    {
        tf = transform;
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += ComboUpdate;

        posCache = tf.position;
    }

    private void Start()
    {
        sequence = DOTween.Sequence();

        sequence.Pause().SetAutoKill( false );
        sequence.Append( tf.DOMoveY( posCache.y + 50f, .15f ) );

        StartCoroutine( UpdateImage() );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        sequence?.Kill();
    }

    private IEnumerator UpdateImage()
    {
        WaitUntil waitAddCombo = new WaitUntil( () => prevCombo != curCombo );
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .05f );

            prevNum = curNum;
            if ( curCombo == 0 )
            {
                curNum = 1;

                if ( !images[0].gameObject.activeInHierarchy )
                     images[0].gameObject.SetActive( true );
                images[0].sprite = sprites[0];

                for ( int i = 1; i < images.Count; i++ )
                {
                    if ( images[i].gameObject.activeInHierarchy )
                         images[i].gameObject.SetActive( false );
                }
            }
            else
            {
                float calcPrevCombo = prevCombo;
                float calcCurCombo  = curCombo;

                //curNum = curCombo > 0 ? Globals.Log10( calcCurCombo ) + 1 : 1;
                for ( int i = 0; i < images.Count; i++ )
                {
                    // if ( i == curNum ) break;
                    if ( ( int )calcPrevCombo % 10 == ( int )calcCurCombo % 10 ) 
                         break;

                    if ( !images[i].gameObject.activeInHierarchy )
                         images[i].gameObject.SetActive( true );

                    images[i].sprite = sprites[( int )calcCurCombo % 10];

                    calcCurCombo *= .1f;
                    calcPrevCombo *= .1f;
                }

                tf.position = posCache;
                sequence.Restart();
            }

            prevCombo = curCombo;

            if ( prevNum != curNum )
                layoutGroup.SetLayoutHorizontal();
        }
    }

    private void ComboUpdate( JudgeType _type )
    {
        switch ( _type )
        {
            case JudgeType.None:
            case JudgeType.Perfect:
            case JudgeType.LatePerfect:
            case JudgeType.Great:
            case JudgeType.Good:
            case JudgeType.Bad:
            curCombo++;
            break;

            case JudgeType.Miss:
            curCombo = 0;
            break;
        }
    }
}
