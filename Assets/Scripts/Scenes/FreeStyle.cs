using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FreeStyle : Scene
{
    private List<Song> songs = new List<Song>();

    public GameObject songPrefab; // sound infomation prefab
    public VerticalScrollSystem scrollSystem;

    public TextMeshProUGUI time, bpm, combo, record, rate;
    public Image background, previewBG;

    private Coroutine curSoundLoad = null;

    private void CoroutineRelease( Coroutine _coroutine ) { if ( !ReferenceEquals( null, _coroutine ) ) StopCoroutine( _coroutine ); }
    private Song curSong;

    #region unity callbacks
    protected override void Awake()
    {
        base.Awake();
        
        // Osu Parsing
        string[] osuFiles = GetFilesInSubDirectories( GlobalSetting.OsuDirectoryPath, "*.osu" );
        for ( int i = 0; i < osuFiles.Length; i++ )
        {
            using ( Parser parser = new OsuParser( osuFiles[i] ) )
            {
                songs.Add( parser.PreRead() );
            }
        }

        // BMS Parsing
        string[] bmsFiles = GetFilesInSubDirectories( GlobalSetting.BmsDirectoryPath, "*.bms" );
        for ( int i = 0; i < bmsFiles.Length; i++ )
        {
            using ( Parser parser = new BmsParser( bmsFiles[i] ) )
            {
                songs.Add( parser.PreRead() );
            }
        }

        // Background Load and Create Scroll Contents
        foreach ( var data in songs )
        {
            // scrollview song contents
            GameObject obj = Instantiate( songPrefab, scrollSystem.transform );
            //obj.GetComponent<SoundInfomation>().song.Initialize( data );

            TextMeshProUGUI[] info = obj.GetComponentsInChildren<TextMeshProUGUI>();
            int idx = data.Version.IndexOf( "-" );
            info[0].text = data.Version.Substring( idx + 1, data.Version.Length - idx - 1 ).Trim();
            //info[1].text = data.version.Substring( 0, idx ); 
        }

        SoundManager.Inst.Volume = 0.1f;
        scrollSystem.OnInitialize += () => { ChangePreview(); };
        DontDestroyOnLoad( this );
    }

    private void Update()
    {
        if ( Input.GetKeyDown( KeyCode.UpArrow ) ) 
        {
            scrollSystem.PrevMove();
            ChangePreview();
        }
        if ( Input.GetKeyDown( KeyCode.DownArrow ) ) 
        {
            scrollSystem.NextMove();
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
        if ( scrollSystem.IsDuplicate ) return;
        
        curSong = songs[int.Parse( scrollSystem.curObject.name )];
        background.sprite = curSong.background;
        
        //CoroutineRelease( curSoundLoad );
        //curSoundLoad = StartCoroutine( PreviewSoundPlay() );
        //curSoundLoadCoroutine = StartCoroutine( PreviewSoundPlay() );
    }
    private IEnumerator PreviewSoundPlay()
    {
        yield return YieldCache.WaitForSeconds( .5f );

        SoundManager.Inst.BGMPlay( SoundManager.Inst.Load( curSong.AudioPath ) );

        int time = curSong.PreviewTime;
        if ( time <= 0 ) SoundManager.Inst.Position = ( uint )( SoundManager.Inst.Length / 3f );
        else             SoundManager.Inst.Position = ( uint )time; 
    }
    #endregion

    #region customize function
    private string[] GetFilesInSubDirectories( string _dirPath, string _extension )
    {
        List<string> path = new List<string>();

        string[] subDirectories = Directory.GetDirectories( _dirPath );
        foreach ( string subDir in subDirectories )
        {
            DirectoryInfo dirInfo = new DirectoryInfo( subDir );
            FileInfo[] files = dirInfo.GetFiles( _extension );
            for ( int i = 0; i < files.Length; i++ )
                path.Add( files[i].FullName );
        }

        return path.ToArray();
    }
    #endregion
}