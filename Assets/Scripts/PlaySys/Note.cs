using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    private static readonly Vector3 InitScale = new Vector3( GlobalSetting.NoteWidth, GlobalSetting.NoteHeight, 1f );
    public float timing { get; private set; }
    public float calcTiming { get; private set; }
    public float endTiming { get; private set; }
    public float calcEndTiming { get; private set; }
    public bool IsLN { get; private set; }
    public bool isHolding;
    public float column;
    private SpriteRenderer rdr;

    public void Initialized( NoteData _data )
    {
        timing = _data.time;
        calcTiming = _data.calcTime;
        endTiming = _data.LNEndTime;
        calcEndTiming = _data.calcEndTime;

        column = GlobalSetting.NoteStartPos + ( _data.line * GlobalSetting.NoteWidth ) + ( ( _data.line + 1 ) * GlobalSetting.NoteBlank );

        if ( _data.type == 128 )
        {
            transform.localScale = new Vector3( GlobalSetting.NoteWidth, Mathf.Abs( ( calcEndTiming * NowPlaying.Weight ) - ( calcTiming * NowPlaying.Weight ) ), 1f );
            IsLN = true;
        }
        else
        {
            transform.localScale = InitScale;
            IsLN = false;
        }

        if ( _data.line == 1 || _data.line == 4 ) rdr.color = Color.blue;
        else                                      rdr.color = Color.white;
    }
    private void OnScrollSpeedChange()
    {
        if ( IsLN ) transform.localScale = new Vector3( GlobalSetting.NoteWidth, Mathf.Abs( ( calcEndTiming * NowPlaying.Weight ) - ( calcTiming * NowPlaying.Weight ) ), 1f );
    }

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        transform.localScale = InitScale;
        GlobalSetting.OnScrollChanged += OnScrollSpeedChange;
    }

    private void LateUpdate()
    {
        if ( isHolding )
        {
            if ( transform.localScale.y > 0 )
            {
                transform.localScale = new Vector3( GlobalSetting.NoteWidth, ( calcEndTiming * NowPlaying.Weight ) - ( NowPlaying.PlaybackChanged * NowPlaying.Weight ), 1f );
            }

            transform.position = new Vector3( column, GlobalSetting.JudgeLine, 2f );
        }else
        transform.position = new Vector3( column, GlobalSetting.JudgeLine + ( ( calcTiming - NowPlaying.PlaybackChanged ) * NowPlaying.Weight ), 2f );
    }
}
