using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChannelCounter : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();
    private List<SpriteRenderer> images = new List<SpriteRenderer>();
    private CustomHorizontalLayoutGroup layoutGroup;

    private void Awake()
    {
        layoutGroup = GetComponent<CustomHorizontalLayoutGroup>();
        images.AddRange( GetComponentsInChildren<SpriteRenderer>( true ) );
        images.Reverse();

        StartCoroutine( CalcFrameRate() );
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

            float calcChannel = SoundManager.Inst.UseChannelCount;
            int number = Global.Math.Log10( calcChannel ) + 1;
            for ( int i = 0; i < number; i++ )
            {
                if ( !images[i].gameObject.activeSelf )
                     images[i].gameObject.SetActive( true );

                images[i].sprite = sprites[( int )calcChannel % 10];
                calcChannel *= .1f;
            }

            layoutGroup.SetLayoutHorizontal();
        }
    }
}
