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
    private float deltaTime;
    private int curFPS = 0, prevFPS = 0;
    private int prevNum, curNum;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();
        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();

        for ( int i = 0; i < images.Count; i++ )
             images[i].sortingOrder = sortingOrder;

        StartCoroutine( UpdateFrame() );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        deltaTime += ( Time.unscaledDeltaTime - deltaTime ) * .1f;
    }

    private IEnumerator UpdateFrame()
    {
        while( true )
        {
            yield return YieldCache.WaitForSeconds( .075f );
            
            curFPS = ( int )( 1f / deltaTime );
            if ( prevFPS != curFPS )
                UpdateImage();

            prevFPS = curFPS;
        }
    }

    private void UpdateImage()
    {
        float calcFPS = curFPS;
        curNum = Global.Math.Log10( curFPS ) + 1;
        for ( int i = 0; i < 5; i++ )
        {
            if ( i < curNum )
            {
                images[i].gameObject.SetActive( true );
                images[i].sprite = sprites[( int )calcFPS % 10];
                calcFPS *= .1f;
            }
            else
            {
                images[i].gameObject.SetActive( false );
            }
        }

        if ( prevNum != curNum )
             layoutGroup.SetLayoutHorizontal();

        prevNum = curNum;
    }
}
