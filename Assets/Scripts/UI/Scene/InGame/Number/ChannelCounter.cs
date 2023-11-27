using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChannelCounter : MonoBehaviour
{
    [Header("Sprite")]
    public int sortingOrder;

    [Header("Channel Counter")]
    public List<Sprite> sprites = new List<Sprite>();
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private CustomHorizontalLayoutGroup layoutGroup;
    private int prevNum, curNum;
    private int curChannel, prevChannel;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();
        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();
        
        for ( int i = 0; i < images.Count; i++ )
              images[i].sortingOrder = sortingOrder;

        StartCoroutine( UpdateChannel() );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private IEnumerator UpdateChannel()
    {
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .075f );

            curChannel = SoundManager.Inst.ChannelsInUse;
            if ( prevChannel != curChannel )
                 UpdateImage();
            
            prevChannel = curChannel;
        }
    }

    private void UpdateImage()
    {
        float calcChannel = curChannel;
        curNum = Global.Math.Log10( calcChannel ) + 1;
        for ( int i = 0; i < 5; i++ )
        {
            if ( i < curNum )
            {
                images[i].gameObject.SetActive( true );
                images[i].sprite = sprites[( int )calcChannel % 10];
                calcChannel *= .1f;
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
