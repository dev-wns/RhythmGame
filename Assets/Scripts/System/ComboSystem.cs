using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private const float ElapsedPower = 100;
    private float curCombo = 0f, prevCombo = -1f;
    private float targetCombo;
    private float highestCombo;
    private float pointOfMiss;
    private int   prevNum, curNum;
    private bool  isMissing;
    private Color color = Color.white;


    [Header("Effect")]
    private Sequence effectSeq;
    private Vector3 startPos;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();

        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += OnReLoad;
        scene.OnResult += OnResult;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += ComboUpdate;

        var rt = transform as RectTransform;
        rt.anchoredPosition = new Vector2( rt.anchoredPosition.x + GameSetting.GearOffsetX, rt.anchoredPosition.y );

        startPos = transform.localPosition;
        for ( int i = 0; i < images.Count; i++ )
              images[i].sortingOrder = sortingOrder;
    }

    private void Start()
    {
        effectSeq = DOTween.Sequence().Pause().SetAutoKill( false );
        effectSeq.Append( transform.DOMoveY( startPos.y + 10f, .115f ) );

        StartCoroutine( BreakCombo() );
    }

    private void OnDestroy()
    {
        effectSeq?.Kill();
    }

    private void OnResult()
    {
        NowPlaying.Inst.SetResult( HitResult.Combo, Mathf.RoundToInt( highestCombo ) );
    }

    private void OnReLoad()
    {
        StopAllCoroutines();
        curNum       = 0;
        prevNum      = 0;
        highestCombo = 0f;
        curCombo     = 0f;
        targetCombo  = 0f;
        pointOfMiss  = 0f;
        prevCombo    = -1f;
        isMissing    = false;
        color = Color.white;

        transform.position = startPos;
        images[0].gameObject.SetActive( true );
        images[0].sprite = sprites[0];
        for ( int i = 1; i < images.Count; i++ )
        {
            images[i].color = Color.white;
            images[i].gameObject.SetActive( false );
        }
        layoutGroup.SetLayoutHorizontal();
        StartCoroutine( BreakCombo() );
    }

    private IEnumerator BreakCombo()
    {
        WaitUntil waitNextValue = new WaitUntil( () => Global.Math.Abs( targetCombo - curCombo ) > float.Epsilon );
        while ( true )
        {
            yield return waitNextValue;

            if ( isMissing )
            {
                curCombo -= ( ElapsedPower + Global.Math.Abs( curCombo - targetCombo ) * 5f ) * Time.deltaTime;
                if ( curCombo <= targetCombo || curCombo <= 0f )
                {
                    curCombo = targetCombo;
                    isMissing = false;
                    color = Color.white;
                }
            }
            else
            {
                curCombo += ( ElapsedPower + Global.Math.Abs( curCombo - targetCombo ) * 5f ) * Time.deltaTime;
                //curCombo += ElapsedPower * Time.deltaTime;
                if ( curCombo > targetCombo )
                     curCombo = targetCombo;
            }

            if ( Global.Math.Abs( prevCombo - curCombo ) > float.Epsilon )
                 UpdateImages();

            prevCombo = curCombo;
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
            ++targetCombo;
            break;

            case HitResult.Miss:
            isMissing = true;
            color = Color.gray;
            pointOfMiss = targetCombo;
            targetCombo = 0;
            break;
        }

        highestCombo = highestCombo < targetCombo ? targetCombo : highestCombo;
        transform.position = startPos;
        effectSeq.Restart();
    }

    private void UpdateImages()
    {
        curNum = curCombo == 0 ? 1 : Global.Math.Log10( curCombo ) + 1;
        double calcCurCombo  = curCombo;
        for ( int i = 0; i < images.Count; i++ )
        {
            if ( i < curNum )
            {
                images[i].gameObject.SetActive( true );
                images[i].sprite = sprites[( int )calcCurCombo % 10];
                images[i].color  = color;
                calcCurCombo *= .1d;
            }
            else
            {
                images[i].gameObject.SetActive( false );
            }
        }

        if ( prevNum != curNum )
             layoutGroup.SetLayoutHorizontal();

        prevNum   = curNum;
    }
}
