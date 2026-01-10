using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileParser : FileConverter
{
    public bool TryParse( string _path, out Song _song )
    {
        _song = new Song();

        try
        {
            _song.filePath  = Path.ChangeExtension( _path, "wns" );
            _song.directory = Path.GetDirectoryName( _path );

            if ( !File.Exists( _song.filePath ) )
                  Convert( _path ); // osu to wns

            // .wns parsing
            OpenFile( _song.filePath );
            while ( ReadLine() != "[Timings]" )
            {
                // General
                if ( Contains( "Title:" ) )   _song.title   = Replace( "Title:",   string.Empty );
                if ( Contains( "Artist:" ) )  _song.artist  = Replace( "Artist:",  string.Empty );
                if ( Contains( "Source:" ) )  _song.source  = Replace( "Source:",  string.Empty );
                if ( Contains( "Creator:" ) ) _song.creator = Replace( "Creator:", string.Empty );
                if ( Contains( "Version:" ) ) _song.version = Replace( "Version:", string.Empty );

                if ( Contains( "AudioOffset:" ) ) _song.audioOffset = int.Parse( Split( ':' ) );
                if ( Contains( "VideoOffset:" ) ) _song.videoOffset = int.Parse( Split( ':' ) );
                if ( Contains( "PreviewTime:" ) ) _song.previewTime = int.Parse( Split( ':' ) );
                if ( Contains( "Volume:" ) )      _song.volume      = float.Parse( Split( ':' ) ) * .01f;

                if ( Contains( "ImageName:" ) )
                {
                    _song.imageName = Split( ':' );
                    _song.imagePath = Path.Combine( directory, _song.imageName );
                }
                if ( Contains( "AudioName:" ) )
                {
                    _song.audioName = Split( ':' );
                    _song.audioPath = Path.Combine( directory, _song.audioName );
                }
                if ( Contains( "VideoName:" ) )
                {
                    _song.videoName = Split( ':' );
                    _song.videoPath = Path.Combine( directory, _song.videoName );
                }

                if ( Contains( "TotalTime:" ) ) _song.totalTime = int.Parse( Split( ':' ) );
                if ( Contains( "Notes:" ) )
                {
                    string[] splitDatas = line.Split( ':' );
                    _song.keyCount       = int.Parse( splitDatas[1] );
                    _song.keyCount       = _song.keyCount == 8 ? 7 : _song.keyCount;
                    _song.noteCount      = int.Parse( splitDatas[2] );
                    _song.sliderCount    = int.Parse( splitDatas[3] );
                    _song.delNoteCount   = int.Parse( splitDatas[4] );
                    _song.delSliderCount = int.Parse( splitDatas[5] );
                }
                if ( Contains( "BPM:" ) )
                {
                    string[] splitDatas = line.Split( ':' );
                    _song.minBpm = int.Parse( splitDatas[1] );
                    _song.maxBpm = int.Parse( splitDatas[2] );
                    _song.mainBPM = double.Parse( splitDatas[3] );
                }
                if ( Contains( "DataExist:" ) )
                {
                    string[] splitDatas = line.Split( ':' );
                    _song.isOnlyKeySound = int.Parse( splitDatas[1] ) == 1;
                    _song.hasKeySound    = int.Parse( splitDatas[2] ) == 1;
                    _song.hasVideo       = int.Parse( splitDatas[3] ) == 1;
                    _song.hasSprite      = int.Parse( splitDatas[4] ) == 1;
                }
            }

            // delete 채보 제거
            if ( _song.noteCount + _song.sliderCount < 10 )
                 throw new Exception( $"Too few notes." );

            Dispose();
        }
        catch ( Exception _error )
        {
            Dispose();
            #if !UNITY_EDITOR
            // 에러 내용 텍스트 파일로 작성하기
            // ------------------------------
            // 미처리된 파일 Failed 폴더로 이동
            //Move( Path.Combine( dir, $"{Path.GetFileNameWithoutExtension( path )}.osu" ), GameSetting.FailedPath ); // 원본파일
            //Move( path,                                                                   GameSetting.FailedPath ); // 변환파일
            #else
            // 에러 위치 찾기
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace( _error, true );
            Debug.LogWarning( $"{trace.GetFrame( 0 ).GetFileLineNumber()} {_error.Message}  {Path.GetFileName( _path )}" );
            #endif

            return false;
        }

        return true;
    }

    public bool TryParse( string _path, out List<Note> _notes, out List<Timing> _timings, out List<SpriteSample> _sprites, out List<KeySound> _samples )
    {
        _notes   = new List<Note>();
        _timings = new List<Timing>();
        _samples = new List<KeySound>();
        _sprites = new List<SpriteSample>();

        try
        {
            OpenFile( _path );
            while ( ReadLine() != "[Timings]" ) { }

            #region Timings
            //Timing curTiming  = new Timing();
            //Timing prevTiming = new Timing( double.MinValue, double.MinValue );
            
            //List<Timing> uninheritedTimings = new List<Timing>();
            //bool hasFixedBPM = GameSetting.CurrentGameMode.HasFlag( GameMode.FixedBPM );
            while ( ReadLine() != "[Sprites]" )
            {
                var split = line.Split( ',' );

                Timing timing        = new Timing();
                timing.time          = double.Parse( split[0] );// / GameSetting.SoundPitch;
                timing.beatLength    = double.Parse( split[1] );// / GameSetting.SoundPitch;
                timing.bpm           = ( 1d / timing.beatLength ) * 60000d;
                timing.isUninherited = int.Parse( split[2] );

                //if ( hasFixedBPM )
                //{
                //    curTiming.bpm = Browser.Inst.CurrentSong.mainBPM;
                //    uninheritedTimings.Add( curTiming );
                //    _timings.Add( curTiming );

                //    while ( ReadLine() != "[Sprites]" ) { }
                //    break;
                //}

                //if ( timing.isUninherited == 1 )
                //     uninheritedTimings.Add( timing );

                _timings.Add( timing );
                //prevTiming = curTiming;
            }
            #endregion

            #region Sprite Samples
            while ( ReadLine() != "[Samples]" )
            {
                var split = line.Split( ',' );

                SpriteSample sprite = new SpriteSample();
                sprite.type         = ( SpriteType )int.Parse( split[0] );
                sprite.start        = double.Parse( split[1] );// / GameSetting.SoundPitch;
                sprite.end          = double.Parse( split[2] );// / GameSetting.SoundPitch;
                sprite.name         = split[3];

                _sprites.Add( sprite );
            }
            #endregion

            #region BGM
            while ( ReadLine() != "[Notes]" )
            {
                var split = line.Split( ',' );

                KeySound sample = new KeySound();
                sample.time     = double.Parse( split[0] );// / GameSetting.SoundPitch;
                sample.volume   = float.Parse( split[1] ) * .01f;
                sample.name     = split[2];

                _samples.Add( sample );
            }
            #endregion

            #region Notes
            //bool isConvert      = GameSetting.HasFlag( GameMode.ConvertKey ) && Browser.Inst.CurrentSong.keyCount == 7;
            //bool isNoSliderFlag = GameSetting.HasFlag( GameMode.NoSlider );
            while ( ReadLineEndOfStream() )
            {
                var split = line.Split( ',' );

                Note note       = new Note();
                note.lane       = int.Parse( split[0] );
                note.time       = double.Parse( split[1] );// / GameSetting.SoundPitch;
                note.endTime    = double.Parse( split[2] );// / GameSetting.SoundPitch;
                note.isSlider   = note.endTime > 0d;
                //note.isSlider   = isNoSliderFlag ? false : note.endTime > 0d;

                var keySoundSplit = split[3].Split( ':' );
                note.keySound = new KeySound( note.time, keySoundSplit[1], float.Parse( keySoundSplit[0] ) * .01f );
                _notes.Add( note );
            }

            if ( _timings.Count == 0 )
                 throw new Exception( "Note Parsing Error" );
            #endregion
        }
        catch ( Exception _error )
        {
            Debug.LogError( _error.Message );
            return false;
        }

        return true;
    }

    /// <summary> 프리스타일에서 사용할 최소 차트 </summary>
    public bool TryParse( in Song _song, out List<Note> _notes, out List<Timing> _timings )
    {
        _notes   = new List<Note>();
        _timings = new List<Timing>();
        //_sprites = new List<SpriteSample>();
        //_samples = new List<KeySound>();
        try
        {
            OpenFile( _song.filePath );
            while ( ReadLine() != "[Timings]" )
            { }

            #region Timings
            while ( ReadLine() != "[Sprites]" )
            {
                var split = line.Split( ',' );

                Timing timing     = new Timing();
                timing.time       = double.Parse( split[0] );
                timing.beatLength = double.Parse( split[1] );
                timing.bpm        = ( 1d / timing.beatLength ) * 60000d;

                _timings.Add( timing );
            }
            #endregion

            //#region Sprite Samples
            //while ( ReadLine() != "[Samples]" )
            //{
            //    var split = line.Split( ',' );

            //    SpriteSample sprite = new SpriteSample();
            //    sprite.type  = ( SpriteType ) int.Parse( split[0] );
            //    sprite.start = double.Parse( split[1] );
            //    sprite.end   = double.Parse( split[2] );
            //    sprite.name  = split[3];

            //    _sprites.Add( sprite );
            //}
            //#endregion

            #region Notes
            while ( ReadLine() != "[Notes]" ) { }

            while ( ReadLineEndOfStream() )
            {
                Note note = new Note();
                var split = line.Split( ',' );

                note.lane     = int.Parse( split[0] );
                note.time     = double.Parse( split[1] );
                note.endTime  = double.Parse( split[2] );
                note.isSlider = note.endTime > 0d;

                // note.distance = GetDistance( note.time, _song.mainBPM, _timings );
                // if ( note.endTime > 0d )
                // {
                //     note.isSlider    = true;
                //     note.endDistance = GetDistance( note.endTime, _song.mainBPM, _timings );
                // }

                _notes.Add( note );
            }

            if ( _timings.Count == 0 )
                 throw new Exception( "Note Parsing Error" );
            #endregion
        }
        catch ( Exception _error )
        {
            Debug.LogError( _error.Message );
            return false;
        }

        return true;
    }

    public double GetDistance( double _time, double _mainBPM, List<Timing> _timings )
    {
        double result  = 0d;
        for ( int i = 0; i < _timings.Count; i++ )
        {
            double time = _timings[i].time;
            double bpm  = _timings[i].bpm / _mainBPM;

            // 지나간 타이밍에 대한 거리
            if ( i + 1 < _timings.Count && _timings[i + 1].time < _time )
            {
                result += bpm * ( _timings[i + 1].time - time );
                continue;
            }

            // 현재 타이밍에 대한 거리
            result += bpm * ( _time - time );
            break;
        }

        return result;
    }
}
