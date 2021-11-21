using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    private Note lnEnd;
    private enum NType { DEFAULT = 0, LN = 128 }
    private RectTransform tf;
    private float hitTiming;
    private int idx;
    private float posX;
    private int type; // default or long note, 0 or 128
    private bool isLnDone = false;

    private void Awake()
    {
        tf = GetComponent<RectTransform>();
    }

    public void SetNote( float _PosX, int _type, float _timing, Note _lnEnd )
    {
        idx = Mathf.FloorToInt( _PosX * 6f / 512f );
        float startPos = ( 1f * idx ) + ( 100f * idx );
        posX = -( ( 1f * 6 ) + ( 100f * 6 ) / 2f ) + startPos;
        tf.anchoredPosition = new Vector2( posX, 540 );
        hitTiming = _timing;
        type = _type;

        //if ( type == NType.LN )
        //{
        //    Note note = NotePool.Inst.Dequeue();
        //    note.idx = Mathf.FloorToInt( _data.x * 6f / 512f );
        //    note.posX = -( ( 1f * 6 ) + ( 100f * 6 ) / 2f ) + startPos;
        //    note.tf.anchoredPosition = new Vector2( posX, 540 );
        //    note.hitTiming = _timing;
        //    note.tf.localScale = new Vector2( 1, LNLength - hitTiming );
        //}
        isLnDone = false;
        lnEnd = _lnEnd;

        
        if ( type == 128 )
            GetComponent<SpriteRenderer>().color = new Color( 0, 0, 255, 255 );
        else if ( type == 2566 || type == 3333 )
            GetComponent<SpriteRenderer>().color = new Color( 0, 255, 0, 255 );
        else
            GetComponent<SpriteRenderer>().color = new Color( 255, 255, 255, 255 ); 
    }

    private void Update()
    {
        if ( type == 128 )
        {
            //if ( isLnDone == false )
            //{
                float height = tf.rect.height;
                float gap = Mathf.Abs( lnEnd.tf.anchoredPosition.y - tf.anchoredPosition.y );
                transform.localScale = new Vector3( 1f, gap / height, 1f );
                isLnDone = true;
            //}
        }
    }

    private void LateUpdate()
    {
        //FMOD.Channel channel;
        //SoundManager.Inst.channelGroup.getChannel( 0, out channel );

        //uint soundPos;
        //channel.getPosition( out soundPos, FMOD.TIMEUNIT.MS );
        // *( 3f / 410 * GameManager.Inst.GlobalScroll ) * 150
        //( ( 60f / GameManager.Inst.globalBpm ) * 4 * 1000 )
        //float posY = ( hitTiming - InGame.time ) * ( 60f / GameManager.Inst.globalBpm );
        // double posY = ( hitTiming - InGame.changeTime ) / 60000f / ( 1f / ( GameManager.Inst.globalBpm * 1000f * 60f ) ) * 0.01f;
        //float posY = (float)( ( hitTiming - InGame.originTime ) / 1000f * 3f / 410f * InGame.GlobalSpeed );
        tf.anchoredPosition = new Vector2( posX, -540f + ( ( hitTiming - InGame.PlayBackChanged ) * 410f * 7f ) );

        //if ( tf.anchoredPosition.y <= -540 )
        //    NotePool.Inst.Enqueue( this );
    }
}
