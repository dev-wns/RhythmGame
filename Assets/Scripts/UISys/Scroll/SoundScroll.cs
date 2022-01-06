using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class SoundScroll : HideScroll
{
    public OptionBase songPrefab; // sound infomation prefab
    private RectTransform rt;

    protected override void Awake()
    {
        base.Awake();

        SelectPosition( GameManager.Inst.CurrentSoundIndex );
    }

    protected override void CreateContents()
    {
        rt = GetComponent<RectTransform>();

        // Create Scroll Contents
        contents.Capacity = GameManager.Inst.Songs.Count;
        for ( int i = 0; i < GameManager.Inst.Songs.Count; i++ )
        {
            // scrollview song contents
            var obj = Instantiate( songPrefab, rt );

            // 사운드 이름 설정
            Song data = GameManager.Inst.Songs[i];
            System.Text.StringBuilder artist = new System.Text.StringBuilder();
            artist.Capacity = data.artist.Length + 8 + data.creator.Length;
            artist.Append( data.artist ).Append( " // " ).Append( data.creator );

            TextMeshProUGUI[] info = obj.GetComponentsInChildren<TextMeshProUGUI>();
            info[0].text = data.title;
            info[1].text = data.version;

            contents.Add( obj );
        }
    }

    public override void PrevMove()
    {
        base.PrevMove();
        if ( !IsLoop && IsDuplicate ) return;

        GameManager.Inst.SelectSong( curIndex );
    }

    public override void NextMove()
    {
        base.NextMove();
        if ( !IsLoop && IsDuplicate ) return;

        GameManager.Inst.SelectSong( curIndex );
    }
}
