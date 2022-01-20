using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileParser : FileReader
{
    public void ParseFilesInDirectories( ref List<Song> _songs )
    {
        _songs?.Clear();
        _songs ??= new List<Song>();

        var files = GetFilesInSubDirectories( GameSetting.SoundDirectoryPath, "*.wns" );
        for ( int i = 0; i < files.Length; i++ )
        {
            Song newSong = new Song();
            if ( TryParse( files[i], out newSong ) )
                 _songs.Add( newSong );
        }
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

                if ( Contains( "MinBPM:" ) )    _song.minBpm    = int.Parse( SplitAndTrim( ':' ) );
                if ( Contains( "MaxBPM:" ) )    _song.maxBpm    = int.Parse( SplitAndTrim( ':' ) );
                if ( Contains( "MedianBPM:" ) ) _song.medianBpm = int.Parse( SplitAndTrim( ':' ) );
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

            _chart.notes?.Clear();
            _chart.notes ??= new List<Note>();

            _chart.timings?.Clear();
            _chart.timings ??= new List<Timing>();


            while ( ReadLine() != "[Timings]" ) { }

            while ( ReadLine() != "[Notes]" )
            {
                Timing timing = new Timing();
                var split = line.Split( ',' );

                timing.time = float.Parse( split[0] );
                timing.bpm  = float.Parse( split[1] );

                _chart.timings.Add( timing );
            }

            while ( ReadLineEndOfStream() )
            {
                Note note = new Note();
                var split = line.Split( ',' );

                note.line           = int.Parse( split[0] );
                note.time           = float.Parse( split[1] );
                note.sliderTime     = float.Parse( split[2] );
                note.isSlider       = bool.Parse( split[3] );

                _chart.notes.Add( note );
            }
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
