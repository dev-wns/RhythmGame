using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class FreeStyle : Scene
{
    public GameObject prefab; // sound infomation prefab
    public Transform scrollSoundsContent;
    public VerticalScrollSnap snap;

    public TextMeshProUGUI time, bpm, combo, record, rate;
    public Image background, previewBG;

    private Coroutine curSoundLoadCoroutine;

    private int Index { get { return snap.SelectIndex; } }

    #region unity callbacks
    protected override void Awake()
    {
        base.Awake();

        SoundManager.Inst.Volume = 0.1f;

        foreach ( var data in GameManager.Datas )
        {
            // scrollview song contents
            GameObject obj = Instantiate( prefab, scrollSoundsContent );
            TextMeshProUGUI[] info = obj.GetComponentsInChildren<TextMeshProUGUI>();

            int idx = data.version.IndexOf( "-" );
            info[0].text = data.version.Substring( idx + 1, data.version.Length - idx - 1 ).Trim();
            //info[1].text = data.version.Substring( 0, idx ); 
        }

        // details
        if ( GameManager.Datas.Count > 0 )
        {
            ChangePreview();
        }
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.UpArrow ) ) 
        {
            snap.SnapUp();
            ChangePreview();
        }
        if ( Input.GetKeyDown( KeyCode.DownArrow ) ) 
        {
            snap.SnapDown();
            ChangePreview();
        }

        if ( Input.GetKeyDown( KeyCode.Return ) )
        {
            NowPlaying.Inst.Initialized( GameManager.Datas[Index] );
            Change( SceneType.InGame );
        }
    }

    private void ChangePreview()
    {
        if ( snap.IsDuplicateKeyCheck ) return;

        ChangePreviewInfo();

        if ( !ReferenceEquals( curSoundLoadCoroutine, null ) )
        {
            StopCoroutine( curSoundLoadCoroutine );
        }
        curSoundLoadCoroutine = StartCoroutine( PreviewSoundPlay() );
    }
    private void ChangePreviewInfo()
    {
        MetaData data = GameManager.Datas[Index];
        bpm.text = Mathf.FloorToInt( data.timings[0].bpm ).ToString();

        background.sprite = GameManager.Datas[Index].background;
    }

    private IEnumerator PreviewSoundPlay()
    {
        yield return YieldCache.WaitForSeconds( .5f );

        MetaData data = GameManager.Datas[Index];
        SoundManager.Inst.LoadAndPlay( data.audioPath );

        int time = data.previewTime;
        if ( time <= 0 ) SoundManager.Inst.Position = ( uint )( SoundManager.Inst.Length / 3f );
        else             SoundManager.Inst.Position = ( uint )time; 
    }
    #endregion
}