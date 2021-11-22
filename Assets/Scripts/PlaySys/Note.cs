using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    private Note lnEnd;
    private RectTransform tf;
    private float hitTiming;
    private float posX;
    private int type; // default or long note, 0 or 128

    private void Awake()
    {
        tf = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if ( type == 128 )
        {
            float height = tf.rect.height;
            float gap = Mathf.Abs( lnEnd.tf.anchoredPosition.y - tf.anchoredPosition.y );
            transform.localScale = new Vector3( 1f, gap / height, 1f );
        }
    }

    private void LateUpdate()
    {
        tf.anchoredPosition = new Vector2( posX, GlobalSetting.JudgeLine + ( ( hitTiming - InGame.PlaybackChanged ) * GlobalSetting.BPMWeight * GlobalSetting.ScrollSpeed ) );
    }

    public void SetNote( float _PosX, int _type, float _timing, Note _lnEnd )
    {
        int posCacIdx = Mathf.FloorToInt( _PosX * 6f / 512f );
        float startPos = ( 1f * posCacIdx ) + ( 100f * posCacIdx );
        posX = -( ( 1f * 6 ) + ( 100f * 6 ) / 2f ) + startPos;
        tf.anchoredPosition = new Vector2( posX, 540 );
        hitTiming = _timing;
        type = _type;
        lnEnd = _lnEnd;

        if ( type == 128 )
            GetComponent<SpriteRenderer>().color = new Color( 0, 0, 255, 255 );
        else if ( type == 2566 || type == 3333 )
            GetComponent<SpriteRenderer>().color = new Color( 0, 255, 0, 255 );
        else
            GetComponent<SpriteRenderer>().color = new Color( 255, 255, 255, 255 );
    }
}
