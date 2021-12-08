using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class FreeStyle : Scene
{
    public GameObject songPrefab; // sound infomation prefab
    public Transform songContents;
    public VerticalScrollSnap snap;

    public TextMeshProUGUI time, bpm, combo, record, rate;
    public Image background, previewBG;

    private Coroutine curSoundLoad = null;
    private Coroutine curImageLoad = null;

    private int Index { get { return snap.SelectIndex; } }

    private void CoroutineRelease( Coroutine _coroutine ) { if ( !ReferenceEquals( null, _coroutine ) ) StopCoroutine( _coroutine ); }

    #region unity callbacks
    protected override void Awake()
    {
        base.Awake();

        SoundManager.Inst.Volume = 0.1f;

        foreach ( var data in MetaData.Songs )
        {
            // scrollview song contents
            GameObject obj = Instantiate( songPrefab, songContents );
            TextMeshProUGUI[] info = obj.GetComponentsInChildren<TextMeshProUGUI>();

            int idx = data.Version.IndexOf( "-" );
            info[0].text = data.Version.Substring( idx + 1, data.Version.Length - idx - 1 ).Trim();
            //info[1].text = data.version.Substring( 0, idx ); 
        }

        // details
        if ( MetaData.Songs.Count > 0 )
        {
            ChangePreview();
        }

        DontDestroyOnLoad( this );
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
            //NowPlaying.Inst.Initialized( GameManager.Datas[Index] );
            Change( SceneType.InGame );
        }
    }

    private void ChangePreview()
    {
        if ( snap.IsDuplicateKeyCheck ) return;

        ChangePreviewInfo();

        CoroutineRelease( curSoundLoad );
        //curSoundLoad = StartCoroutine( PreviewSoundPlay() );
        //curSoundLoadCoroutine = StartCoroutine( PreviewSoundPlay() );
    }

    private IEnumerator BackgroundsLoad()
    {
        // backgrounds
        Texture2D t = new Texture2D( 1, 1, TextureFormat.ARGB32, false );
        byte[] binaryData = System.IO.File.ReadAllBytes( MetaData.Songs[Index].ImagePath );
        while ( !t.LoadImage( binaryData ) ) yield return null;

        background.sprite = Sprite.Create( t, new Rect( 0, 0, t.width, t.height ), new Vector2( .5f, .5f ), 100, 0, SpriteMeshType.FullRect );
    }


    private void ChangePreviewInfo()
    {
        //MetaData data = GameManager.Datas[Index];
        //bpm.text = Mathf.FloorToInt( data.timings[0].bpm ).ToString();
        CoroutineRelease( curImageLoad );
        curImageLoad = StartCoroutine( BackgroundsLoad() );
    }

    private IEnumerator PreviewSoundPlay()
    {
        yield return YieldCache.WaitForSeconds( .5f );

        Song data = MetaData.Songs[Index];
        SoundManager.Inst.BGMPlay( SoundManager.Inst.Load( data.AudioPath ) );

        int time = data.PreviewTime;
        if ( time <= 0 ) SoundManager.Inst.Position = ( uint )( SoundManager.Inst.Length / 3f );
        else             SoundManager.Inst.Position = ( uint )time; 
    }
    #endregion
}