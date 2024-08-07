using System.Collections.Generic;
using UnityEngine;

public class RecordSystem : MonoBehaviour
{
    public FreeStyleMainScroll mainScroll;
    public RecordInfomation prefab;
    private List<RecordInfomation> records = new List<RecordInfomation>();
    private CustomVerticalLayoutGroup group;

    [Header("Hide Infomation")]
    //public IconController icon;
    private static bool IsHideRecord;

    [Header("Effect")]
    public float startPosX;

    private void Awake()
    {
        group  = GetComponent<CustomVerticalLayoutGroup>();
        mainScroll.OnSelectSong += UpdateRecord;
        
        for ( int i = 0; i < NowPlaying.MaxRecordSize; i++ )
        {
            RecordInfomation obj = Instantiate( prefab, transform );
            records.Add( obj );
        }

        group.Initialize( true );
        group.SetLayoutVertical();

        for ( int i = 0; i < NowPlaying.MaxRecordSize; i++ )
        {
            records[i].Initialize( i, startPosX );
        }

        //icon.Color = !IsHideRecord ? new Color( 1f, 1f, 1f, .25f ) : Color.white;
    }

    public void HideRecordInfomation()
    {
        //icon.Play();
        IsHideRecord = !IsHideRecord;
        if ( IsHideRecord )
        {
            SoundManager.Inst.Play( SoundSfxType.MenuHover );
            //icon.Color = Color.white;
            for ( int i = 0; i < NowPlaying.MaxRecordSize; i++ )
            {
                records[i].SetActive( false );
            }
        }
        else
        {
            SoundManager.Inst.Play( SoundSfxType.MenuClick );
            //icon.Color = new Color( 1f, 1f, 1f, .25f );
            UpdateRecord( NowPlaying.CurrentSong );
        }
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
            }
            else
            {
                records[i].SetActive( false );
            }
        }
    }
}