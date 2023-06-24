using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

public class FileParser : FileReader
{
    public bool TryParse( string _path, out Song _song )
    {
        _song = new Song();

        try
        {
            OpenFile( _path );

            string directory = Path.GetDirectoryName( _path );
            _song.filePath   = _path;

            while ( ReadLine() != "[Timings]" )
            {
                if ( Contains( "ImagePath:" ) )
                {
                    var imageName = Split( ':' );
                    _song.imagePath = imageName == string.Empty ? string.Empty :
                                                                  Path.Combine( directory, imageName );
                }
                if ( Contains( "AudioPath:" ) )
                {
                    var soundName = Split( ':' );
                    _song.audioPath = soundName == string.Empty ? string.Empty :
                                                                  Path.Combine( directory, soundName );
                }
                if ( Contains( "VideoPath:" ) )
                {
                    string videoName = Split( ':' );
                    _song.hasVideo  = videoName != string.Empty;
                    _song.videoPath = _song.hasVideo ? Path.Combine( directory, videoName ) : string.Empty;
                }
                if ( Contains( "VideoOffset:" ) ) _song.videoOffset = int.Parse( Split( ':' ) );

                if ( Contains( "Title:" ) )   _song.title   = Replace( "Title:",   string.Empty );
                if ( Contains( "Artist:" ) )  _song.artist  = Replace( "Artist:",  string.Empty );
                if ( Contains( "Source:" ) )  _song.source  = Replace( "Source:",  string.Empty );
                if ( Contains( "Creator:" ) ) _song.creator = Replace( "Creator:", string.Empty );
                if ( Contains( "Version:" ) ) _song.version = Replace( "Version:", string.Empty );

                if ( Contains( "PreviewTime:" ) ) _song.previewTime = int.Parse( Split( ':' ) );
                if ( Contains( "TotalTime:" ) )   _song.totalTime   = int.Parse( Split( ':' ) );

                if ( Contains( "KeyCount:" ) )
                {
                    _song.keyCount = int.Parse( Split( ':' ) );
                    _song.keyCount = _song.keyCount == 8 ? 7 : _song.keyCount;
                }
                if ( Contains( "NumNote:" ) )      _song.noteCount      = int.Parse( Split( ':' ) );
                if ( Contains( "NumSlider:" ) )    _song.sliderCount    = int.Parse( Split( ':' ) );
                if ( Contains( "NumDelNote:" ) )   _song.delNoteCount   = int.Parse( Split( ':' ) );
                if ( Contains( "NumDelSlider:" ) ) _song.delSliderCount = int.Parse( Split( ':' ) );

                if ( Contains( "MinBPM:" ) )  _song.minBpm    = int.Parse( Split( ':' ) );
                if ( Contains( "MaxBPM:" ) )  _song.maxBpm    = int.Parse( Split( ':' ) );
                if ( Contains( "Median:" ) )  _song.medianBpm = double.Parse( Split( ':' ) );

                if ( Contains( "DataExist:" ) )
                {
                    string[] splitDatas = line.Split( ':' );
                    _song.isOnlyKeySound = int.Parse( splitDatas[1] ) == 1;
                    _song.hasKeySound    = int.Parse( splitDatas[2] ) == 1;
                    _song.hasVideo       = int.Parse( splitDatas[3] ) == 1;
                    _song.hasSprite      = int.Parse( splitDatas[4] ) == 1;
                }
            }

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
            Move( Path.Combine( dir, $"{Path.GetFileNameWithoutExtension( path )}.osu" ), GameSetting.FailedPath ); // 원본파일
            Move( path,                                                                   GameSetting.FailedPath ); // 변환파일
            #else
            // 에러 위치 찾기
            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace( _error, true );
            Debug.LogWarning( $"{trace.GetFrame( 0 ).GetFileLineNumber()} {_error.Message}  {Path.GetFileName( path )}" );
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
            List<Timing> timings = new List<Timing>();
            List<Timing> uninheritedTimings = new List<Timing>();

            bool hasFixedBPM = GameSetting.CurrentGameMode.HasFlag( GameMode.FixedBPM );
            while ( ReadLine() != "[Sprites]" )
            {
                Timing timing = new Timing();
                var split = line.Split( ',' );

                timing.time          = double.Parse( split[0] ) * .001d / GameSetting.CurrentPitch;
                timing.beatLength    = double.Parse( split[1] );
                timing.bpm           = ( 1d / timing.beatLength * 60000d ) * GameSetting.CurrentPitch;

                if ( uninheritedTimings.Count == 0 )
                {
                    double firstTime = timing.time;
                    double spb       = ( 60d / timing.bpm ) * 4;
                    while ( firstTime > NowPlaying.WaitTime )
                    {
                        firstTime -= spb;
                    }

                    timing.time = firstTime;
                }

                if ( hasFixedBPM )
                {
                    timing.bpm = NowPlaying.CurrentSong.medianBpm * GameSetting.CurrentPitch;
                    uninheritedTimings.Add( timing );
                    timings.Add( timing );

                    while ( ReadLine() != "[Sprites]" ) { }
                    break;
                }

                if ( int.Parse( split[2] ) == 1 )
                {
                    uninheritedTimings.Add( timing );
                }

                timings.Add( timing );
            }

            _chart.uninheritedTimings = new ReadOnlyCollection<Timing>( uninheritedTimings );
            _chart.timings            = new ReadOnlyCollection<Timing>( timings );
#endregion
            
#region Sprite Samples
            List<SpriteSample> sprites = new List<SpriteSample>();
            while ( ReadLine() != "[Samples]" )
            {
                SpriteSample sprite;
                var split = line.Split( ',' );

                sprite.type  = ( SpriteType )int.Parse( split[0] );
                sprite.start = double.Parse( split[1] ) * .001d / GameSetting.CurrentPitch;
                sprite.end   = double.Parse( split[2] ) * .001d / GameSetting.CurrentPitch;
                sprite.name  = split[3];

                sprites.Add( sprite );
            }
            _chart.sprites = new ReadOnlyCollection<SpriteSample>( sprites );
#endregion
#region Key Samples
            List<KeySound> keySounds = new List<KeySound>();
            while ( ReadLine() != "[Notes]" )
            {
                KeySound sample;
                var split = line.Split( ',' );

                sample.time = double.Parse( split[0] ) * .001d / GameSetting.CurrentPitch;
                sample.volume = float.Parse( split[1] ) * .01f;
                sample.name = split[2];
                //sample.sound = new FMOD.Sound();
                //sample.hasSound = sample.name == string.Empty ? false : true;

                keySounds.Add( sample );
            }
            _chart.samples = new ReadOnlyCollection<KeySound>( keySounds );
#endregion
#region Notes
            List<Note> notes = new List<Note>();

            while ( ReadLineEndOfStream() )
            {
                Note note = new Note();
                var split = line.Split( ',' );

                note.lane           = int.Parse( split[0] );
                note.time           = double.Parse( split[1] ) * .001d / GameSetting.CurrentPitch;
                note.sliderTime     = double.Parse( split[2] ) * .001d / GameSetting.CurrentPitch;
                note.isSlider       = note.sliderTime > 0d ? true : false;

                var keySoundSplit = split[3].Split( ':' );
                note.keySound.volume   = float.Parse( keySoundSplit[0] ) * .01f;
                note.keySound.name     = keySoundSplit[1];
                //note.keySound.hasSound = note.keySound.name == string.Empty ? false : true;

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
