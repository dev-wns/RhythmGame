using DG.Tweening;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;

public class ComboSystem : MonoBehaviour
{
    private InGame scene;

    [Header("ComboSystem")]
    private float curCombo;
    private float targetCombo;
    private float ComboPos;
    private float diffCached;

    public TextMeshProUGUI text;

    [Header("Effect")]
    private Sequence effectSeq;
    private Vector3  startPos;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += OnReLoad;
        //scene.OnResult += OnResult;

        InputManager.OnHitNote += UpdateCombo;

        var rt = transform as RectTransform;
        rt.anchoredPosition = new Vector2( rt.anchoredPosition.x + GameSetting.GearOffsetX, rt.anchoredPosition.y );
        startPos = transform.localPosition;
    }

    private void Start()
    {
        effectSeq = DOTween.Sequence().Pause().SetAutoKill( false );
        effectSeq.Append( transform.DOMoveY( startPos.y + 10f, .115f ) );

        StartCoroutine( BreakCombo() );
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
        curCombo    = 0f;
        ComboPos    = 0f;
        targetCombo = 0f;
        transform.position = startPos;

        text.text  = $"{( int )curCombo}";
        text.color = Color.white;

        //StartCoroutine( BreakCombo() );
    }

    private IEnumerator BreakCombo()
    {
        WaitUntil waitNextValue = new WaitUntil( () => Global.Math.Abs( targetCombo - curCombo ) > float.Epsilon );
        while ( true )
        {
            yield return waitNextValue;

            curCombo -= diffCached  * Time.deltaTime;
            if ( curCombo <= targetCombo || curCombo <= 0f )
            {
                curCombo = targetCombo;
                text.color = Color.white;
            }

            if ( ( int )ComboPos != ( int )curCombo )
            {
                ComboPos = curCombo;
                text.text = $"{( int )curCombo}";
            }
        }
    }

    private void UpdateCombo( HitData _hitData )
    {
        targetCombo = Judgement.CurrentResult.Combo;
        if ( _hitData.hitResult == HitResult.Miss )
        {
            diffCached = Global.Math.Clamp( Global.Math.Abs( curCombo - targetCombo ), 100f, 1000f );
            text.color = curCombo > double.Epsilon ? Color.gray : Color.white;
        }
        
        transform.position = startPos;
        effectSeq.Restart();
    }
}
