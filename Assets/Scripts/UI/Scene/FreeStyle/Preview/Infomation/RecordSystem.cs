using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordSystem : MonoBehaviour
{
    public FreeStyleMainScroll mainScroll;
    public RecordInfomation prefab;
    private List<RecordInfomation> records = new List<RecordInfomation>();
    private CustomVerticalLayoutGroup group;

    private int recordCount;

    [Header("Hide Infomation")]
    public Image icon;
    private static bool IsHideRecord;

    [Header("Effect")]
    public float startPosX;
    public float duration;
    public float waitTime;
    private float offset;

    private void Awake()
    {
        mainScroll.OnSelectSong += UpdateRecord;
        
        for ( int i = 0; i < NowPlaying.MaxRecordSize; i++ )
        {
            RecordInfomation obj = Instantiate( prefab, transform );
            obj.SetActive( false );
            records.Add( obj );
        }

        offset = -startPosX / duration;
        group  = GetComponent<CustomVerticalLayoutGroup>();

        icon.color = IsHideRecord ? new Color( 1f, 1f, 1f, .25f ) : Color.white;
    }

    public void HideRecordInfomation()
    {
        IsHideRecord = !IsHideRecord;
        if ( IsHideRecord )
        {
            SoundManager.Inst.Play( SoundSfxType.MenuHover );
            icon.color = new Color( 1f, 1f, 1f, .25f );
            for ( int i = 0; i < NowPlaying.MaxRecordSize; i++ )
            {
                records[i].SetActive( false );
            }
        }
        else
        {
            SoundManager.Inst.Play( SoundSfxType.MenuClick );
            icon.color = Color.white;
            UpdateRecord( NowPlaying.CurrentSong );
        }
    }

    private void Start()
    {
        group.Initialize( true );
        group.SetLayoutVertical();
    }

    private void UpdateRecord( Song _song )
    {
        if ( IsHideRecord )
             return;
             
        NowPlaying.Inst.UpdateRecord();
        var datas = NowPlaying.Inst.RecordDatas;
        for ( int i = 0; i < NowPlaying.MaxRecordSize; i++ )
        {
            if ( i < datas.Count )
            {
                records[i].SetActive( true );
                records[i].SetInfo( datas[i] );
                records[i].Play( startPosX, offset, waitTime * ( i + 1 ) );
            }
            else
            {
                records[i].SetActive( false );
            }
        }
    }
}