using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ComboSystem : MonoBehaviour
{
    private InGame scene;
    [Header("Sprite")]
    public int sortingOrder;

    [Header("ComboSystem")]
    public  List<Sprite>         sprites = new List<Sprite>();
    private List<SpriteRenderer> images  = new List<SpriteRenderer>();
    private CustomHorizontalLayoutGroup layoutGroup;
    private Judgement judge;
    private int maxCombo;
    private int prevCombo = -1, curCombo;
    private int prevNum, curNum;

    [Header("Effect")]
    // private Sequence sequence;
    private Sequence sequence;
    private Vector3 startPos;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();

        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += ReLoad;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += ComboUpdate;

        NowPlaying.Inst.OnResult += Result;

        startPos = transform.localPosition;

        for ( int i = 0; i < images.Count; i++ )
              images[i].sortingOrder = sortingOrder;
    }

    private void Start()
    {
        sequence = DOTween.Sequence().Pause().SetAutoKill( false );
        sequence.Append( transform.DOMoveY( startPos.y + 20f, .15f ) );
    }

    private void OnDestroy()
    {
        sequence?.Kill();
        NowPlaying.Inst.OnResult -= Result;
    }

    private void Result() => judge.SetResult( HitResult.Combo, maxCombo );

    private void ReLoad()
    {
        maxCombo  = 0;
        prevNum   = curNum = 0;
        prevCombo = -1; 
        curCombo  = 0;

        transform.position = startPos;
        images[0].gameObject.SetActive( true );
        images[0].sprite = sprites[0];
        for ( int i = 1; i < images.Count; i++ )
        {
            images[i].color = Color.white;
            images[i].gameObject.SetActive( false );
        }
        layoutGroup.SetLayoutHorizontal();
    }

    private void ComboUpdate( HitResult _result, NoteType _type )
    {
        switch ( _result )
        {
            case HitResult.Fast:
            case HitResult.Slow:
            return;

            case HitResult.None:
            case HitResult.Maximum:
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

        maxCombo = maxCombo < curCombo ? curCombo : maxCombo;

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
            curNum = curCombo == 0 ? 1 : Global.Math.Log10( curCombo ) + 1;
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

            transform.position = startPos;
            sequence.Restart();
        }

        if ( prevNum != curNum )
             layoutGroup.SetLayoutHorizontal();

        prevCombo = curCombo;
        prevNum   = curNum;
    }
}
