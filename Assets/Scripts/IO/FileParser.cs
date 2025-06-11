using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                if ( Contains( "Volume:" ) )      _song.volume      = int.Parse( Split( ':' ) );

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

    public bool TryParse( string _path, out Chart _chart )
    {
        _chart = new Chart();
        try
        {
            OpenFile( _path );
            while ( ReadLine() != "[Timings]" )
            { }

            #region Timings
            Timing curTiming  = new Timing();
            Timing prevTiming = new Timing( double.MinValue, double.MinValue );
            List<Timing> timings = new List<Timing>();
            List<Timing> uninheritedTimings = new List<Timing>();

            bool hasFixedBPM = GameSetting.CurrentGameMode.HasFlag( GameMode.FixedBPM );
            while ( ReadLine() != "[Sprites]" )
            {
                var split = line.Split( ',' );

                curTiming.time          = double.Parse( split[0] ) / GameSetting.CurrentPitch;
                curTiming.beatLength    = double.Parse( split[1] ) / GameSetting.CurrentPitch;
                curTiming.bpm           = ( 1d / curTiming.beatLength ) * 60000d;
                curTiming.isUninherited = int.Parse( split[2] );

                if ( hasFixedBPM )
                {
                    curTiming.bpm = NowPlaying.CurrentSong.mainBPM;
                    uninheritedTimings.Add( curTiming );
                    timings.Add( curTiming );

                    while ( ReadLine() != "[Sprites]" ) { }
                    break;
                }

                if ( curTiming.isUninherited == 1 )
                     uninheritedTimings.Add( curTiming );

                timings.Add( curTiming );
                prevTiming = curTiming;
            }
            #endregion

            #region Sprite Samples
            List<SpriteSample> backgrounds = new List<SpriteSample>();
            List<SpriteSample> foregrounds = new List<SpriteSample>();
            while ( ReadLine() != "[Samples]" )
            {
                SpriteSample sprite;
                var split = line.Split( ',' );

                sprite.type  = ( SpriteType )int.Parse( split[0] );
                sprite.start = double.Parse( split[1] ) / GameSetting.CurrentPitch;
                sprite.end   = double.Parse( split[2] ) / GameSetting.CurrentPitch;
                sprite.name  = split[3];

                if      ( sprite.type == SpriteType.Background ) backgrounds.Add( sprite );
                else if ( sprite.type == SpriteType.Foreground ) foregrounds.Add( sprite );
            }
            #endregion

            #region BGM
            List<KeySound> samples = new List<KeySound>();
            while ( ReadLine() != "[Notes]" )
            {
                KeySound sample;
                var split = line.Split( ',' );

                sample.time   = double.Parse( split[0] ) / GameSetting.CurrentPitch;
                sample.volume = float.Parse( split[1] ) * .01f;
                sample.name   = split[2];

                samples.Add( sample );
            }
            #endregion

            #region Notes
            List<Note>     notes     = new List<Note>();
            bool isConvert      = GameSetting.HasFlag( GameMode.ConvertKey ) && NowPlaying.CurrentSong.keyCount == 7;
            bool isNoSliderFlag = GameSetting.HasFlag( GameMode.NoSlider );
            while ( ReadLineEndOfStream() )
            {
                Note note = new Note();
                var split = line.Split( ',' );

                note.lane       = int.Parse( split[0] );
                note.time       = double.Parse( split[1] ) / GameSetting.CurrentPitch;
                note.endTime    = double.Parse( split[2] ) / GameSetting.CurrentPitch;
                note.isSlider   = isNoSliderFlag ? false : note.endTime > 0d;

                var keySoundSplit = split[3].Split( ':' );
                note.keySound = new KeySound( note.time, keySoundSplit[1], float.Parse( keySoundSplit[0] ) * .01f );

                if ( isConvert && note.lane == 3 )
                {
                    // 잘려진 노트는 키음만 자동재생되도록 한다.
                    samples.Add( note.keySound );
                    continue;
                }

                if ( isConvert && note.lane > 3 )
                     note.lane -= 1;

                notes.Add( note );
            }

            if ( timings.Count == 0 )
                 throw new Exception( "Note Parsing Error" );

            // 배경음이 없으면, 프리뷰 음악을 재생한다.
            if ( samples.Count < 0 )
                 samples.Add( new KeySound( GameSetting.SoundOffset, NowPlaying.CurrentSong.audioName, 1f ) );

            // 특정모드 선택으로 잘린 키음이 추가될 수 있다. ( 시간 오름차순 정렬 )
            samples.Sort( delegate ( KeySound _A, KeySound _B )
            {
                if      ( _A.time > _B.time ) return 1;
                else if ( _A.time < _B.time ) return -1;
                else                          return 0;
            } );

            _chart.notes       = new ReadOnlyCollection<Note>( notes );
            _chart.timings     = new ReadOnlyCollection<Timing>( timings );
            _chart.samples     = new ReadOnlyCollection<KeySound>( samples );
            _chart.backgrounds = new ReadOnlyCollection<SpriteSample>( backgrounds );
            _chart.foregrounds = new ReadOnlyCollection<SpriteSample>( foregrounds );
            #endregion
        }
        catch ( Exception _error )
        {
            Debug.LogError( _error.Message );
            return false;
        }

        return true;
    }

    public bool TryPreviewParse( string _path, out Chart _chart )
    {
        _chart = new Chart();
        try
        {
            OpenFile( _path );
            while ( ReadLine() != "[Timings]" )
            { }

            #region Timings
            Timing curTiming = new Timing();
            List<Timing> timings = new List<Timing>();

            while ( ReadLine() != "[Sprites]" )
            {
                var split = line.Split( ',' );

                curTiming.time       = double.Parse( split[0] );
                curTiming.beatLength = double.Parse( split[1] );
                curTiming.bpm        = ( 1d / curTiming.beatLength ) * 60000d;

                timings.Add( curTiming );
            }

            _chart.timings = new ReadOnlyCollection<Timing>( timings );
            #endregion

            #region Notes
            while ( ReadLine() != "[Notes]" ) { }

            List<Note> notes = new List<Note>();
            while ( ReadLineEndOfStream() )
            {
                Note note = new Note();
                var split = line.Split( ',' );

                note.lane       = int.Parse( split[0] );
                note.time       = double.Parse( split[1] );
                note.endTime = double.Parse( split[2] );
                note.isSlider   = note.endTime > 0d ? true : false;

                notes.Add( note );
            }

            if ( timings.Count == 0 )
                throw new Exception( "Note Parsing Error" );

            _chart.notes = new ReadOnlyCollection<Note>( notes );
            #endregion
        }
        catch ( Exception _error )
        {
            Debug.LogError( _error.Message );
            return false;
        }

        return true;
    }
}
