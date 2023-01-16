using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Net.Http.Headers;

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
    private int highestCombo;
    private int prevCombo = -1;
    private int prevNum, curNum;


    [Header("Effect")]
    private Sequence sequence;
    private Vector3 startPos;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();

        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += ReLoad;
        scene.OnResult += Result;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += ComboUpdate;

        startPos = transform.localPosition;
        for ( int i = 0; i < images.Count; i++ )
              images[i].sortingOrder = sortingOrder;
    }

    private void Start()
    {
        sequence = DOTween.Sequence().Pause().SetAutoKill( false );
        sequence.Append( transform.DOMoveY( startPos.y + 10f, .1f ) );
        StartCoroutine( Count() );
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }

    private void Result()
    {
        NowPlaying.Inst.SetResultData( HitResult.Combo, highestCombo );
    }

    private void ReLoad()
    {
        highestCombo = 0;
        curNum       = 0;
        prevNum      = 0;
        targetCombo  = 0;
        elapsedCombo = 0f;
        curCombo     = 0;
        prevCombo    = -1;

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

    public int curCombo, targetCombo;
    public float elapsedCombo = 0f;
    private IEnumerator Count()
    {
        WaitUntil waitNextValue = new WaitUntil( () => ( targetCombo - curCombo ) != 0 );
        while ( true )
        {
            yield return waitNextValue;

            elapsedCombo += ( targetCombo - curCombo ) / ( targetCombo < curCombo ? .15f : .05f ) * Time.deltaTime;
            curCombo = ( int )elapsedCombo;

            if ( prevCombo != curCombo )
            {
                curNum = curCombo == 0 ? 1 : Global.Math.Log10( curCombo ) + 1;
                float calcCurCombo  = curCombo;
                Color color = targetCombo < curCombo ? Color.grey : Color.white;
                for ( int i = 0; i < images.Count; i++ )
                {
                    if ( i < curNum )
                    {
                        images[i].gameObject.SetActive( true );
                        images[i].color  = color;
                        images[i].sprite = sprites[( int )calcCurCombo % 10];
                        calcCurCombo *= .1f;
                    }
                    else
                    {
                        images[i].gameObject.SetActive( false );
                    }
                }

                if ( prevNum != curNum )
                     layoutGroup.SetLayoutHorizontal();

                prevNum   = curNum;
                prevCombo = curCombo;
            }
        }
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
            targetCombo++;
            break;

            case HitResult.Miss:
            targetCombo = 0;
            break;
        }

        highestCombo = highestCombo < targetCombo ? targetCombo : highestCombo;

        transform.position = startPos;
        sequence.Restart();
    }
}
