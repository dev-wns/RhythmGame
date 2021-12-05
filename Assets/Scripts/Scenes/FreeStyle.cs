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

        foreach ( var data in GameManager.songs )
        {
            // scrollview song contents
            GameObject obj = Instantiate( prefab, scrollSoundsContent );
            TextMeshProUGUI[] info = obj.GetComponentsInChildren<TextMeshProUGUI>();

            int idx = data.Version.IndexOf( "-" );
            info[0].text = data.Version.Substring( idx + 1, data.Version.Length - idx - 1 ).Trim();
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

        //curSoundLoadCoroutine = StartCoroutine( PreviewSoundPlay() );
    }
    private IEnumerator BackgroundsLoad()
    {
        //// backgrounds
        //UnityWebRequest www = UnityWebRequestTexture.GetTexture( GameManager.songs[_idx].ImagePath );
        //
        //yield return www.SendWebRequest();
        //if ( www.result != UnityWebRequest.Result.Success )
        //{
        //    Debug.Log( www.error );
        //}
        //else
        //{
        //    Texture2D tex = ( ( DownloadHandlerTexture )www.downloadHandler ).texture;
        //    Sprite sprite = Sprite.Create( tex, new Rect( 0f, 0f, tex.width, tex.height ), new Vector2( 0.5f, 0.5f ) );
        //
        //    background.sprite = sprite;
        //}

        Texture2D t = new Texture2D( 1, 1, TextureFormat.ARGB32, false );
        byte[] binaryData = System.IO.File.ReadAllBytes( GameManager.songs[Index].ImagePath );
        while ( !t.LoadImage( binaryData ) ) yield return null;

        background.sprite = Sprite.Create( t, new Rect( 0, 0, t.width, t.height ), new Vector2( .5f, .5f ), 100, 0, SpriteMeshType.FullRect );
        // background.sprite = Sprite.Create( t, new Rect( 0, 0, t.width, t.height ), new Vector2( .5f, .5f ) );
    }
    private Coroutine loadIageCoroutine = null;
    private void ChangePreviewInfo()
    {
        if ( !ReferenceEquals( null, loadIageCoroutine ) ) StopCoroutine( loadIageCoroutine );
        //MetaData data = GameManager.Datas[Index];
        //bpm.text = Mathf.FloorToInt( data.timings[0].bpm ).ToString();
        loadIageCoroutine = StartCoroutine( BackgroundsLoad() );
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