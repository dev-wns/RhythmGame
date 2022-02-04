using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitCountSystem : MonoBehaviour
{
    public HitResult type;
    public List<Sprite> sprites = new List<Sprite>();
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private CustomHorizontalLayoutGroup layoutGroup;
    private InGame scene;
    private Judgement judge;

    private int prevCount, curCount;
    private int prevNum, curNum;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();

        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += ReLoad;

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += AddCount;

        NowPlaying.Inst.OnResult += Result;
    }

    private void OnDestroy()
    {
        NowPlaying.Inst.OnResult -= Result;
    }

    private void Result() => judge.SetResult( type, curCount );

    private void ReLoad()
    {
        prevNum = curNum = 0;
        prevCount = curCount = 0;

        images[0].gameObject.SetActive( true );
        images[0].sprite = sprites[0];
        for ( int i = 1; i < images.Count; i++ )
        {
            images[i].gameObject.SetActive( false );
        }
        layoutGroup.SetLayoutHorizontal();
    }

    private void AddCount( HitResult _type )
    {
        if ( type != _type ) return;
        curCount++;

        curNum = curCount == 0 ? 1 : Globals.Log10( curCount ) + 1;
        float calcPrevCount = prevCount;
        float calcCurCount = curCount;
        for ( int i = 0; i < images.Count; i++ )
        {
            if ( ( int )calcPrevCount % 10 == ( int )calcCurCount % 10 )
                 break;

            if ( !images[i].gameObject.activeSelf )
                 images[i].gameObject.SetActive( true );

            images[i].sprite = sprites[( int )calcCurCount % 10];
            calcCurCount *= .1f;
            calcPrevCount *= .1f;
        }

        if ( prevNum != curNum )
             layoutGroup.SetLayoutHorizontal();

        prevCount = curCount;
        prevNum = curNum;
    }
}
