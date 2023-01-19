using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [Header("Sprite")]
    public int sortingOrder;

    [Header("FPS Counter")]
    public List<Sprite> sprites = new List<Sprite>();
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private CustomHorizontalLayoutGroup layoutGroup;
    private float deltaTime = 0f;
    private int prevNum, curNum;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();
        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();

        for ( int i = 0; i < images.Count; i++ )
             images[i].sortingOrder = sortingOrder;

        StartCoroutine( CalcFrameRate() );
    }

    private void Update()
    {
        deltaTime += ( Time.unscaledDeltaTime - deltaTime ) * .1f;
    }

    private IEnumerator CalcFrameRate()
    {
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .075f );

            for ( int i = 0; i < 5; i++ )
            {
                images[i].gameObject.SetActive( false );
            }

            float fps = 1f / deltaTime;
            float calcFPS = fps;
            curNum = Global.Math.Log10( fps ) + 1;

            for ( int i = 0; i < curNum; i++ )
            {
                if ( !images[i].gameObject.activeSelf )
                     images[i].gameObject.SetActive( true );

                images[i].sprite = sprites[( int )calcFPS % 10];
                calcFPS *= .1f;
            }

            if ( prevNum != curNum )
                 layoutGroup.SetLayoutHorizontal();

            prevNum = curNum;
        }
    }
}
