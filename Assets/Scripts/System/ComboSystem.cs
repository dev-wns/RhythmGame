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
        sequence = DOTween.Sequence().Pause().SetAutoKill( false );
        sequence.Append( transform.DOMoveY( startPos.y + 10f, .1f ) );
        StartCoroutine( BreakCombo() );
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }

    private void OnResult()
    {
        NowPlaying.Inst.SetResult( HitResult.Combo, highestCombo );
    }

    private void OnReLoad()
    {
        StopAllCoroutines();
        highestCombo = 0;
        curNum       = 0;
        prevNum      = 0;
        curCombo     = 0;
        targetCombo  = 0;
        prevCombo    = -1;
        breakCombo   = 0;
        breakElapsedCombo = 0f;
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

    public int curCombo;
    public int targetCombo;
    public float breakElapsedCombo = 0f;
    public float breakCombo = 0f;
    public Color color = Color.white;
    private IEnumerator BreakCombo()
    {
        WaitUntil waitNextValue = new WaitUntil( () => targetCombo < curCombo );
        while ( true )
        {
            yield return waitNextValue;

            breakElapsedCombo -= breakCombo * 2f * Time.deltaTime; // .5s
            curCombo = ( int )Global.Math.Clamp( breakElapsedCombo, 0f, 1000000f );
            if ( targetCombo >= curCombo )
            {
                color = Color.white;
                curCombo = targetCombo;
            }

            UpdateImages();
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
            color = Color.gray;
            breakElapsedCombo = breakCombo = curCombo;
            targetCombo = 0;
            break;
        }

        highestCombo = highestCombo < targetCombo ? targetCombo : highestCombo;

        transform.position = startPos;
        sequence.Restart();

        if ( targetCombo > curCombo )
        {
            curCombo = targetCombo;
            UpdateImages();
        }
    }

    private void UpdateImages()
    {
        curNum = curCombo == 0 ? 1 : Global.Math.Log10( curCombo ) + 1;
        float calcCurCombo  = curCombo;
        for ( int i = 0; i < images.Count; i++ )
        {
            if ( i < curNum )
            {
                images[i].gameObject.SetActive( true );
                images[i].sprite = sprites[( int )calcCurCombo % 10];
                images[i].color  = color;
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
