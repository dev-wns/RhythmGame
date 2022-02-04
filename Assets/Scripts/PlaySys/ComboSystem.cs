using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ComboSystem : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();
    private InGame scene;
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private CustomHorizontalLayoutGroup layoutGroup;
    private Judgement judge;
    private int prevCombo = -1, curCombo;
    private int prevNum, curNum;
    private Sequence sequence;

    private Transform tf;
    private Vector2 posCache;

    private void Awake()
    {
        tf = transform;
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();

        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += ReLoad;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += ComboUpdate;

        posCache = tf.position;
    }
    
    private void ReLoad()
    {
        prevNum = curNum = 0;
        prevCombo = -1; 
        curCombo = 0;

        images[0].gameObject.SetActive( true );
        images[0].sprite = sprites[0];
        for ( int i = 1; i < images.Count; i++ )
        {
            images[i].gameObject.SetActive( false );
        }
        layoutGroup.SetLayoutHorizontal();
    }

    private void Start()
    {
        sequence = DOTween.Sequence();

        sequence.Pause().SetAutoKill( false );
        sequence.Append( tf.DOMoveY( posCache.y + 25f, .15f ) );
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }

    private void ComboUpdate( HitResult _type )
    {
        switch ( _type )
        {
            case HitResult.Fast:
            case HitResult.Slow:
            return;

            case HitResult.None:
            case HitResult.Perfect:
            case HitResult.Great:
            case HitResult.Good:
            case HitResult.Bad:
            curCombo++;
            break;

            case HitResult.Miss:
            curCombo = 0;
            break;
        }

        if ( curCombo == 0 )
        {
            curNum = 1;
            if ( !images[0].gameObject.activeSelf )
                 images[0].gameObject.SetActive( true );
            images[0].sprite = sprites[0];

            for ( int i = 1; i < images.Count; i++ )
            {
                if ( images[i].gameObject.activeSelf )
                     images[i].gameObject.SetActive( false );
            }
        }
        else
        {
            curNum = curCombo == 0 ? 1 : Globals.Log10( curCombo ) + 1;
            float calcPrevCombo = prevCombo;
            float calcCurCombo = curCombo;
            for ( int i = 0; i < images.Count; i++ )
            {
                if ( ( int )calcPrevCombo % 10 == ( int )calcCurCombo % 10 )
                    break;

                if ( !images[i].gameObject.activeSelf )
                     images[i].gameObject.SetActive( true );

                images[i].sprite = sprites[( int )calcCurCombo % 10];
                calcCurCombo  *= .1f;
                calcPrevCombo *= .1f;
            }

            tf.position = posCache;
            sequence.Restart();
        }

        if ( prevNum != curNum )
             layoutGroup.SetLayoutHorizontal();

        prevCombo = curCombo;
        prevNum = curNum;
    }
}
