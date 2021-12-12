using System.IO;
using System.Collections;
using System.Collections.Generic;
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

        scrollSystem.OnInitialize += () => { ChangePreview(); };
    }

    private void Update()
    {
        if ( !SoundManager.Inst.IsPlaying() )
        {
            SoundManager.Inst.Play();

            // 중간부터 재생
            int time = curSong.PreviewTime;
            if ( time <= 0 ) SoundManager.Inst.SetPosition( ( uint )( SoundManager.Inst.Length / 3f ) );
            else             SoundManager.Inst.SetPosition( ( uint )time );
        }

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

        if ( Input.GetKeyDown( KeyCode.A ) )
            SoundManager.Inst.UseLowEqualizer( true );

        if ( Input.GetKeyDown( KeyCode.S ) )
            SoundManager.Inst.UseLowEqualizer( false );

        if ( Input.GetKeyDown( KeyCode.S ) )
            SoundManager.Inst.UseLowEqualizer( false );

        if ( Input.GetKeyDown( KeyCode.LeftArrow ) )
            SoundManager.Inst.SetPitch( SoundManager.Inst.Pitch - .1f );

        if ( Input.GetKeyDown( KeyCode.RightArrow ) )
            SoundManager.Inst.SetPitch( SoundManager.Inst.Pitch + .1f );
    }

    private void ChangePreview()
    {
        if ( scrollSystem.IsDuplicate ) return;
        
        curSong = songs[int.Parse( scrollSystem.curObject.name )];
        background.sprite = curSong.background;

        Globals.Timer.Start();
        {
            SoundManager.Inst.Load( curSong.AudioPath, Sound.LoadType.Stream );
            SoundManager.Inst.Play();
        }
        Debug.Log( $"Sound Load {Globals.Timer.End()} ms" );

        // 중간부터 재생
        int time = curSong.PreviewTime;
        if ( time <= 0 ) SoundManager.Inst.SetPosition( ( uint )( SoundManager.Inst.Length / 3f ) );
        else             SoundManager.Inst.SetPosition( ( uint )time );

    }
    #endregion

    #region customize function
    private string[] GetFilesInSubDirectories( string _dirPath, string _extension )
    {
        List<string> path = new List<string>();

        string[] subDirectories;
        try { subDirectories = Directory.GetDirectories( _dirPath ); }
        catch ( System.Exception e )
        {
            // 대부분 폴더가 없는 경우.
            Debug.Log( e.ToString() );
            return path.ToArray(); 
        }

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