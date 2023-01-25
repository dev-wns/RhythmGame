using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitCountSystem : MonoBehaviour
{
    [Header("Sprite")]
    public int sortingOrder;

    [Header("HitCountSystem")]
    public HitResult type;
    public List<Sprite> sprites = new List<Sprite>();
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private CustomHorizontalLayoutGroup layoutGroup;
    private InGame scene;
    private Judgement judge;

    private int prevCount, curCount;
    private int prevNum,   curNum;

    private void Awake()
    {
        if ( !GameSetting.CurrentVisualFlag.HasFlag( GameVisualFlag.ShowHitCount ) )
             return;

        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();

        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += OnReLoad;
        scene.OnResult += OnResult;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += AddCount;

        for ( int i = 0; i < images.Count; i++ )
              images[i].sortingOrder = sortingOrder;
    }

    private void OnResult()
    {
        NowPlaying.Inst.SetResultCount( type, curCount );
    }

    private void OnReLoad()
    {
        prevNum   = curNum   = 0;
        prevCount = curCount = 0;

        images[0].gameObject.SetActive( true );
        images[0].sprite = sprites[0];
        for ( int i = 1; i < images.Count; i++ )
        {
            images[i].gameObject.SetActive( false );
        }
        layoutGroup.SetLayoutHorizontal();
    }

    private void AddCount( HitResult _result, NoteType _type )
    {
        HitResult resultType = _result == HitResult.Maximum ? HitResult.Perfect : _result;
        if ( type != resultType ) return;
        curCount++;

        curNum = curCount == 0 ? 1 : Global.Math.Log10( curCount ) + 1;
        float calcPrevCount = prevCount;
        float calcCurCount  = curCount;
        for ( int i = 0; i < images.Count; i++ )
        {
            if ( ( int )calcPrevCount % 10 == ( int )calcCurCount % 10 )
                 break;

            if ( !images[i].gameObject.activeSelf )
                 images[i].gameObject.SetActive( true );

            images[i].sprite = sprites[( int )calcCurCount % 10];
            calcCurCount  *= .1f;
            calcPrevCount *= .1f;
        }

        if ( prevNum != curNum )
             layoutGroup.SetLayoutHorizontal();

        prevCount = curCount;
        prevNum   = curNum;
    }
}
