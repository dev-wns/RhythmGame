using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColNote : MonoBehaviour
{
    public float timing { get; private set; }
    private float column, weight;
    private int type; // LN( 128 ) or Default
    private Note lnEnd;
    private RectTransform tf;
    public bool IsLN { get; private set; }

    private void Awake()
    {
        tf = GetComponent<RectTransform>();
    }

    private void Update()
    {
        //if ( type == 128 )
        //{
        //    float height = tf.rect.height;
        //    float gap = Mathf.Abs( tfLN.anchoredPosition.y - tf.anchoredPosition.y );
        //    transform.localScale = new Vector3( 1f, gap / height, 1f );

        //    if ( tfLN.anchoredPosition.y <= GlobalSetting.JudgeLine + 10f )
        //        InGame.cPool.Despawn( this ); 
        //}
        //else
        //{
            //if ( originTiming + 178.4f < InGame.__time )
            //    InGame.cPool.Despawn( this );
                //if ( tf.anchoredPosition.y <= GlobalSetting.JudgeLine - 300f )
        //}
    }

    private void LateUpdate()
    {
        tf.anchoredPosition = new Vector3( column, GlobalSetting.JudgeLine + ( ( timing - InGame.__time ) ) );
    }

    public void SetNote( int _lineNum, float _weight, int _type, float _timing, Note _lnEnd )
    {
        weight = _weight;
        transform.localScale = Vector3.one;

        column = GlobalSetting.NoteStartPos + ( _lineNum * GlobalSetting.NoteWidth ) +
                 ( ( _lineNum * GlobalSetting.NoteBlank ) + GlobalSetting.NoteBlank );
        tf.anchoredPosition = new Vector2( column, 540 );
        timing = _timing;
        lnEnd = _lnEnd;

        type = _type;
    }
}
