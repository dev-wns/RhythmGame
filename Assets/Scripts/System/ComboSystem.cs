using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    private InGame scene;

    [Header("ComboSystem")]
    //private Judgement judge;

    private const float ElapsedPower = 100;
    private float curCombo = 0f;
    private float targetCombo;


    public TextMeshProUGUI text;

    [Header("Effect")]
    private Sequence effectSeq;
    private Vector3  startPos;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += OnReLoad;
        //scene.OnResult += OnResult;

        //judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        //judge.OnJudge += ComboUpdate;

        InputManager.OnHitNote += UpdateCombo;

        var rt = transform as RectTransform;
        rt.anchoredPosition = new Vector2( rt.anchoredPosition.x + GameSetting.GearOffsetX, rt.anchoredPosition.y );

        startPos = transform.localPosition;
    }

    private void Start()
    {
        effectSeq = DOTween.Sequence().Pause().SetAutoKill( false );
        effectSeq.Append( transform.DOMoveY( startPos.y + 10f, .115f ) );

        //StartCoroutine( BreakCombo() );
    }

    private void OnDestroy()
    {
        InputManager.OnHitNote -= UpdateCombo;
        effectSeq?.Kill();
    }

    //private void OnResult()
    //{
    //    DataStorage.Inst.UpdateResult( HitResult.Combo, Mathf.RoundToInt( highestCombo ) );
    //}

    private void OnReLoad()
    {
        StopAllCoroutines();
        curCombo = 0f;
        targetCombo = 0f;
        transform.position = startPos;

        text.text = $"{( int )curCombo}";
        text.color = Color.white;

        //StartCoroutine( BreakCombo() );
    }

    //private IEnumerator BreakCombo()
    //{
    //    WaitUntil waitNextValue = new WaitUntil( () => Global.Math.Abs( targetCombo - curCombo ) > float.Epsilon );
    //    while ( true )
    //    {
    //        yield return waitNextValue;

    //        if ( isMissing )
    //        {
    //            curCombo -= ( ElapsedPower + Global.Math.Abs( curCombo - targetCombo ) * 5f ) * Time.deltaTime;
    //            if ( curCombo <= targetCombo || curCombo <= 0f )
    //            {
    //                curCombo = targetCombo;
    //                text.color = Color.white;
    //            }
    //        }
    //        else
    //        {
    //            curCombo += ( ( ElapsedPower * 2.5f ) + Global.Math.Abs( curCombo - targetCombo ) * 5f ) * Time.deltaTime;

    //            if ( curCombo > targetCombo )
    //                curCombo = targetCombo;
    //        }

    //        text.text = $"{( int )curCombo}";
    //    }
    //}

    private void Update()
    {
        if ( targetCombo <= curCombo )
        {
            text.color = Color.gray;
            curCombo -= ( ElapsedPower + Global.Math.Abs( curCombo - targetCombo ) * 5f ) * Time.deltaTime;
            if ( curCombo <= targetCombo || curCombo <= 0f )
            {
                curCombo   = targetCombo;
                text.color = Color.white;
            }
        }
    }

    private void UpdateCombo( HitData _hitData )
    {
        targetCombo = Judgement.CurrentResult.Combo;

        HitResult hitResult = _hitData.hitResult;
        switch ( hitResult )
        {
            case HitResult.None:
            case HitResult.Maximum:
            case HitResult.Perfect:
            case HitResult.Great:
            case HitResult.Good:
            case HitResult.Bad: ++targetCombo; break;

            case HitResult.Miss:
            {
                text.color  = curCombo > double.Epsilon ? Color.gray : Color.white;
                targetCombo = 0;
            } break;

            default: return;
        }

        //highestCombo = highestCombo < targetCombo ? targetCombo : highestCombo;
        transform.position = startPos;
        effectSeq.Restart();
    }
}
