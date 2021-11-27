using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    private Note lnEnd;
    private float column, weight;
    public float originTiming { get; private set; }
    public float timing { get; private set; }
    private int type; // LN( 128 ) or Default
    public bool IsLN { get; private set; }
    private SpriteRenderer rdr;
    // Caches
    private static Vector3 InitPosition = new Vector3( 0f, 4000f, 0f );

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3( GlobalSetting.NoteWidth, GlobalSetting.NoteHeight, 1f );
    }

    private void OnDisable()
    {
        transform.position = InitPosition;
    }

    private void Update()
    {
        if ( type == 128 )
        {
            //float height = tf.rect.height;
            //float gap = Mathf.Abs( lnEnd.tf.anchoredPosition.y - tf.anchoredPosition.y );
            //transform.localScale = new Vector3( 1f, gap / height, 1f );

            // if ( lnEnd.tf.anchoredPosition.y <= GlobalSetting.JudgeLine - 300 )
            //if ( originTiming + 178.4f < InGame.__time )
                ////InGame.nPool.Despawn( this;
        }
        else 
        {
            //if ( tf.anchoredPosition.y <= GlobalSetting.JudgeLine )
            //    InGame.nPool.Despawn( this );
            //if ( originTiming + 178.4f < InGame.__time )
        }
    }

    private void LateUpdate()
    {
        transform.position = new Vector3( column, GlobalSetting.JudgeLine + ( ( timing - InGame.PlaybackChanged ) * weight ) , 0f );
    }

    public void SetNote( int _lineNum, int _key, float _weight, int _type, float _originTiming, float _timing, Note _lnEnd )
    {
        weight = _weight;
        //transform.localScale = Vector3.one;
        rdr.sortingOrder = _key;

        column = GlobalSetting.NoteStartPos + ( _lineNum * GlobalSetting.NoteWidth ) + ( ( _lineNum + 1 ) * GlobalSetting.NoteBlank );

        transform.position = new Vector3( column, 540f, 0f );
        timing = _timing;
        originTiming = _originTiming;
        lnEnd = _lnEnd;

        type = _type;
        if ( type == 128 ) IsLN = true;
        else IsLN = false;


        //if ( type == 128 )
        //    GetComponent<SpriteRenderer>().color = new Color( 0, 0, 255, 255 );
        //else if ( type == 2566 || type == 3333 )
        //    GetComponent<SpriteRenderer>().color = new Color( 0, 255, 0, 255 );
        //else
        //    GetComponent<SpriteRenderer>().color = new Color( 255, 255, 255, 255 );

        //GetComponent<UnityEngine.UI.Image>().color = new Color( 255, 0, 255, 255 );
    }
}
