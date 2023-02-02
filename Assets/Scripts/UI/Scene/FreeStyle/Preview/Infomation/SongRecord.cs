using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SongRecord : MonoBehaviour
{
    public FreeStyleMainScroll mainScroll;
    public RecordInfomation prefab;
    public GameObject noRecord;
    private List<RecordInfomation> records = new List<RecordInfomation>();
    private CustomVerticalLayoutGroup group;

    private void Awake()
    {
        mainScroll.OnSelectSong += UpdateRecord;
        
        for ( int i = 0; i < 5; i++ )
        {
            RecordInfomation obj = Instantiate( prefab, transform );
            obj.SetActive( false );
            records.Add( obj );
        }

        group = GetComponent<CustomVerticalLayoutGroup>();
    }

    private void Start()
    {
        group.Initialize( true );
        group.SetLayoutVertical();
    }

    private void UpdateRecord( Song _song )
    {
        NowPlaying.Inst.UpdateRecord();
        var datas = NowPlaying.Inst.RecordDatas;
        noRecord.SetActive( datas.Count == 0 );
        for ( int i = 0; i < 5; i++ )
        {
            if ( i < datas.Count )
            {
                records[i].SetActive( true );
                records[i].SetInfo( i, datas[i] );
            }
            else
            {
                records[i].SetActive( false );
            }
        }
    }
}