using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    private Note lnEnd;
    public ColNote col;
    private RectTransform tf;
    private float column, weight;
    public float originTiming { get; private set; }
    public float timing { get; private set; }
    private int type; // LN( 128 ) or Default
    public bool IsLN { get; private set; }
    
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
        tf.anchoredPosition = new Vector3( column, GlobalSetting.JudgeLine + ( ( timing - InGame.PlaybackChanged ) * weight ) );
    }

    public void SetNote( int _lineNum, int _key, float _weight, int _type, float _originTiming, float _timing, Note _lnEnd, ColNote _col )
    {
        weight = _weight;
        transform.localScale = Vector3.one;
        GetComponent<SpriteRenderer>().sortingOrder = _key;

        column = GlobalSetting.NoteStartPos + ( _lineNum * GlobalSetting.NoteWidth ) + ( ( _lineNum + 1 ) * GlobalSetting.NoteBlank );

        tf.anchoredPosition = new Vector2( column, 540 );
        timing = _timing;
        originTiming = _originTiming;
        lnEnd = _lnEnd;
        col = _col;

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
