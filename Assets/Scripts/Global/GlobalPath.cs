using System.Collections.Generic;
using UnityEngine;

namespace Global.IO
{
    public static class Path
    {
        public static readonly string SoundDirectory  = System.IO.Path.Combine( Application.streamingAssetsPath, "Songs" );
        public static readonly string FailedDirectory = System.IO.Path.Combine( Application.streamingAssetsPath, "Failed" );
        public static readonly string RecordDirectory = System.IO.Path.Combine( Application.streamingAssetsPath, "Records" );
        public static readonly string DataDirectory   = Application.streamingAssetsPath;

        public static string[] GetFilesInSubDirectories( string _dir, string _extension )
        {
            List<string> paths = new List<string>();
            try
            {
                string[] subDirectories = System.IO.Directory.GetDirectories( _dir );
                paths.Capacity = subDirectories.Length;
                for ( int i = 0; i < subDirectories.Length; i++ )
                {
                    System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo( subDirectories[i] );
                    System.IO.FileInfo[] files      = dirInfo.GetFiles( _extension );

                    for ( int j = 0; j < files.Length; j++ )
                        paths.Add( files[j].FullName );
                }
            }
            catch ( System.Exception _error )
            {
                // 대부분 폴더가 없는 경우
                Debug.LogError( $"{_error}, {_dir}" );
            }

            return paths.ToArray();
        }
    }
}