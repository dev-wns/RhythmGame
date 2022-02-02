using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using System;
using System.IO;

public class FileParser : FileReader
{
    public void ParseFilesInDirectories( out ReadOnlyCollection<Song> _songs )
    {
        List<Song> songs = new List<Song>();

        var files = GetFilesInSubDirectories( GameSetting.SoundDirectoryPath, "*.wns" );
        for ( int i = 0; i < files.Length; i++ )
        {
            Song newSong = new Song();
            if ( TryParse( files[i], out newSong ) )
                 songs.Add( newSong );
        }

        _songs = new ReadOnlyCollection<Song>( songs );
    }

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
                if ( line == string.Empty ) break;

                if ( Contains( "AudioPath:" ) ) _song.audioPath = Path.Combine( directory, SplitAndTrim( ':' ) );
                if ( Contains( "ImagePath:" ) ) _song.imagePath = Path.Combine( directory, SplitAndTrim( ':' ) );
                if ( Contains( "VideoPath:" ) )
                {
                    string videoName = SplitAndTrim( ':' );

                    if ( videoName == string.Empty ) _song.hasVideo = false;
                    else
                    {
                        _song.videoPath = Path.Combine( directory, videoName );
                        _song.hasVideo = true;
                    }
                }

                if ( Contains( "Title:" ) )   _song.title   = SplitAndTrim( ':' );
                if ( Contains( "Artist:" ) )  _song.artist  = SplitAndTrim( ':' );
                if ( Contains( "Creator:" ) ) _song.creator = SplitAndTrim( ':' );
                if ( Contains( "Version:" ) ) _song.version = SplitAndTrim( ':' );

                if ( Contains( "PreviewTime:" ) ) _song.previewTime = int.Parse( SplitAndTrim( ':' ) );
                if ( Contains( "TotalTime:" ) )   _song.totalTime   = int.Parse( SplitAndTrim( ':' ) );

                if ( Contains( "NumNote:" ) )   _song.noteCount   = int.Parse( SplitAndTrim( ':' ) );
                if ( Contains( "NumSlider:" ) ) _song.sliderCount = int.Parse( SplitAndTrim( ':' ) );
                if ( Contains( "NumTiming:" ) ) _song.timingCount = int.Parse( SplitAndTrim( ':' ) );

                if ( Contains( "MinBPM:" ) ) _song.minBpm    = int.Parse( SplitAndTrim( ':' ) );
                if ( Contains( "MaxBPM:" ) ) _song.maxBpm    = int.Parse( SplitAndTrim( ':' ) );
                if ( Contains( "Median:" ) ) _song.medianBpm = double.Parse( SplitAndTrim( ':' ) );
                if ( Contains( "Virtual:" ) ) _song.isVirtual = int.Parse( SplitAndTrim( ':' ) ) == 1 ? true : false;
            }
        }
        catch ( Exception _error )
        {
            Debug.LogError( $"{_error}, {_path}" );
            Dispose();
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
            while ( ReadLine() != "[Timings]" ) { }

            #region Timings Parsing
            List<Timing> timings = new List<Timing>();

            while ( ReadLine() != "[Samples]" )
            {
                Timing timing = new Timing();
                var split = line.Split( ',' );

                timing.time        = double.Parse( split[0] ) * .001d;
                timing.beatLength  = double.Parse( split[1] );
                timing.bpm         = 1d / timing.beatLength * 60000d;

                timings.Add( timing );
            }

            if ( timings.Count == 0 )
                 throw new Exception( "Timing Parsing Error" );

            _chart.timings = new ReadOnlyCollection<Timing>( timings );
            #endregion

            #region Samples Parsing
            List<KeySample> samples = new List<KeySample>();
            while ( ReadLine() != "[KeySounds]" )
            {
                KeySample sample;
                var split = line.Split( ',' );

                sample.time = double.Parse( split[0] ) * .001d;
                sample.sound.volume = float.Parse( split[1] ) * .01f;
                sample.sound.name = split[2];

                samples.Add( sample );
            }
            _chart.samples = new ReadOnlyCollection<KeySample>( samples );
            #endregion

            #region HitSounds Parsing
            List<string> keySounds = new List<string>();
            while ( ReadLine() != "[Notes]" )
            {
                keySounds.Add( line );
            }
            _chart.keySoundNames = new ReadOnlyCollection<string>( keySounds );
            #endregion

            #region Notes Parsing
            List<Note> notes = new List<Note>();

            while ( ReadLineEndOfStream() )
            {
                Note note = new Note();
                var split = line.Split( ',' );

                note.lane           = int.Parse( split[0] );
                note.time           = double.Parse( split[1] ) * .001d;
                note.sliderTime     = double.Parse( split[2] ) * .001d;
                note.isSlider       = note.sliderTime > 0d ? true : false;

                var keySoundSplit = split[3].Split( ':' );
                note.keySound.volume = float.Parse( keySoundSplit[0] ) * .01f;
                note.keySound.name   = keySoundSplit[1];

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
            Dispose();
            return false;
        }

        return true;
    }
}
