using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeCountSystem : MonoBehaviour
{
    public JudgeType type;
    public List<Sprite> sprites = new List<Sprite>();
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private CustomHorizontalLayoutGroup layoutGroup;
    private Judgement judge;

    private int prevCount, curCount;
    private int prevNum, curNum;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();

        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();

        judge = GameObject.FindGameObjectWithTag( "Judgement" ).GetComponent<Judgement>();
        judge.OnJudge += AddCount;
    }

    private void Start()
    {
        StartCoroutine( UpdateImage() );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private IEnumerator UpdateImage()
    {
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .05f );
            if ( prevCount == curCount )
                 continue;

            prevNum = curNum;

            float calcPrevCount = prevCount;
            float calcCurCount  = curCount;
            curNum = curCount > 0 ? Globals.Log10( calcCurCount ) + 1 : 1;
            for ( int i = 0; i < images.Count; i++ )
            {
                if ( ( int )calcPrevCount % 10 == ( int )calcCurCount % 10 )
                     break;

                if ( !images[i].gameObject.activeInHierarchy )
                     images[i].gameObject.SetActive( true );

                images[i].sprite = sprites[( int )calcCurCount % 10];
                calcCurCount  *= .1f;
                calcPrevCount *= .1f;
            }

            prevCount = curCount;

            if ( prevNum != curNum )
                 layoutGroup.SetLayoutHorizontal();
        }
    }

    private void AddCount( JudgeType _type )
    {
        if ( type != _type ) return;
        curCount++;
    }
}
