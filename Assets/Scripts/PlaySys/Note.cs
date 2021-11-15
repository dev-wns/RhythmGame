using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    private RectTransform tf;
    private float hitTiming;
    public int idx;
    float posX;

    private void Awake()
    {
        tf = GetComponent<RectTransform>();
    }

    public void SetNote( MetaData.Notes _data, float _timing )
    {
        idx = Mathf.FloorToInt( _data.x * 6f / 512f );
        float startPos = ( 1f * idx ) + ( 100f * idx );
        posX = -( ( 1f * 6 ) + ( 100f * 6 ) / 2f ) + startPos;
        tf.anchoredPosition = new Vector2( posX, 540 );
        hitTiming = _timing;
        //StartCoroutine( Destroy() );
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

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds( 1f );
        NotePool.Inst.Enqueue( this );
    }
}
