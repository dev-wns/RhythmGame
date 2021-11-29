using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public float calcTiming { get; private set; }
    public float timing     { get; private set; }

    private float column;
    private int type; // LN( 128 ) or Default
    private SpriteRenderer rdr;

    public void Initialized( NoteData _data )
    {
        //rdr.sortingOrder = _key;
        calcTiming = _data.calcTime;
        timing = _data.time;
        type = _data.type;

        column = GlobalSetting.NoteStartPos + ( _data.line * GlobalSetting.NoteWidth ) + ( ( _data.line + 1 ) * GlobalSetting.NoteBlank );

        //if ( type == 128 )
        //    GetComponent<SpriteRenderer>().color = new Color( 0, 0, 255, 255 );
        //else if ( type == 2566 || type == 3333 )
        //    GetComponent<SpriteRenderer>().color = new Color( 0, 255, 0, 255 );
        //else
        //    GetComponent<SpriteRenderer>().color = new Color( 255, 255, 255, 255 );

        //GetComponent<UnityEngine.UI.Image>().color = new Color( 255, 0, 255, 255 );
    }

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3( GlobalSetting.NoteWidth, GlobalSetting.NoteHeight, 1f );
    }

    //private void OnDisable()
    //{
    //    transform.position = InitPosition;
    //}

    //private void Update()
    //{
    //    if ( type == 128 )
    //    {
    //        //float height = tf.rect.height;
    //        //float gap = Mathf.Abs( lnEnd.tf.anchoredPosition.y - tf.anchoredPosition.y );
    //        //transform.localScale = new Vector3( 1f, gap / height, 1f );

    //        // if ( lnEnd.tf.anchoredPosition.y <= GlobalSetting.JudgeLine - 300 )
    //        //if ( originTiming + 178.4f < InGame.__time )
    //            ////InGame.nPool.Despawn( this;
    //    }
    //    else 
    //    {
    //        //if ( tf.anchoredPosition.y <= GlobalSetting.JudgeLine )
    //        //    InGame.nPool.Despawn( this );
    //        //if ( originTiming + 178.4f < InGame.__time )
    //    }
    //}

    private void LateUpdate()
    {
        transform.position = new Vector3( column, GlobalSetting.JudgeLine + ( ( calcTiming - NowPlaying.PlaybackChanged ) * NowPlaying.Weight ), 0f );
    }
}
