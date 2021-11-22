using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FreeStyle : Scene
{
    public GameObject prefab; // sound infomation prefab
    public Transform scrollSoundsContent;
    public VerticalScrollSnap snap;

    public TextMeshProUGUI time, bpm, combo, record, rate;
    public Image background, previewBG;

    private Coroutine curSoundLoadCoroutine;
    private FMOD.Sound sound;
    private int Index { get { return snap.SelectIndex; } }

    #region unity callbacks
    protected override void Awake()
    {
        base.Awake();

        SoundManager.Inst.Volume = 0.1f;

        foreach ( var data in GameManager.datas )
        {
            // scrollview song contents
            GameObject obj = Instantiate( prefab, scrollSoundsContent );
            TextMeshProUGUI[] info = obj.GetComponentsInChildren<TextMeshProUGUI>();

            int idx = data.version.IndexOf( "-" );
            info[0].text = data.version.Substring( idx + 1, data.version.Length - idx - 1 ).Trim();
            //info[1].text = data.version.Substring( 0, idx ); 
        }

        // details
        if ( GameManager.datas.Count > 0 )
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
            Change( SceneType.InGame );
        }
    }

    private void ChangePreview()
    {
        if ( snap.IsDuplicateKeyCheck ) return;

        GameManager.SelectData = GameManager.datas[Index];

        ChangePreviewInfo();

        if ( !ReferenceEquals( curSoundLoadCoroutine, null ) )
        {
            StopCoroutine( curSoundLoadCoroutine );
        }
        curSoundLoadCoroutine = StartCoroutine( PreviewSoundPlay() );
    }

    private void ChangePreviewInfo()
    {
        MetaData data = GameManager.datas[Index];
        bpm.text = Mathf.FloorToInt( ( float )data.timings[0].bpm ).ToString();

        background.sprite = GameManager.datas[Index].background;
    }

    private IEnumerator PreviewSoundPlay()
    {
        yield return new WaitForSecondsRealtime( .1f );

        MetaData data = GameManager.datas[Index];
        sound = data.sound;

        SoundManager.Inst.Stop();
        SoundManager.Inst.Play( sound );

        FMOD.Channel channel;
        SoundManager.Inst.channelGroup.getChannel( 0, out channel );

        int time = data.previewTime;
        if ( time <= 0 )
        {
            uint length = 0;
            sound.getLength( out length, FMOD.TIMEUNIT.MS );
            time = ( int )( length / 3.65f );
        }

        channel.setPosition( ( uint )time, FMOD.TIMEUNIT.MS );
    }
    #endregion
}