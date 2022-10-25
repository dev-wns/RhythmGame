using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;

public class SpritePreview : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    private RawImage image;
    private List<SpriteSample> sprites = new List<SpriteSample>();
    private Dictionary<string/* Sprite Name */, Texture2D> textures = new Dictionary<string, Texture2D>();
    private double playback;

    private void Awake()
    {
        image = GetComponent<RawImage>();
        scroller.OnSelectSong += UpdateSpriteSample;
        scroller.OnPlaybackUpdate += UpdatePreview;
    }

    private void OnDestroy()
    {
        Clear();
    }

    private void Clear()
    {
        StopAllCoroutines();
        sprites.Clear();
        foreach ( Texture2D texture in textures.Values )
        {
            DestroyImmediate( texture );
        }
        textures.Clear();
    }

    private void UpdateSpriteSample( Song _song )
    {
        Clear();

        if ( !_song.hasVideo && _song.hasSprite )
        {
            curIndex = 0;
            float previewTime = _song.previewTime <= 0 ? _song.totalTime * Mathf.PI * .1f : _song.previewTime;
            using ( StreamReader reader = new StreamReader( @$"\\?\{_song.filePath}" ) )
            {
                string line;
                while ( ( line = reader.ReadLine() ) != "[Sprites]" ) { }
                while ( ( line = reader.ReadLine() ) != "[Samples]" )
                {
                    SpriteSample sprite;
                    var split = line.Split( ',' );

                    sprite.type  = ( SpriteType )int.Parse( split[0] );
                    if ( sprite.type != SpriteType.Background )
                         continue;

                    sprite.start = double.Parse( split[1] );
                    sprite.end   = double.Parse( split[2] );
                    sprite.name  = split[3];

                    if ( sprite.start <= previewTime )
                         startIndex = sprites.Count;

                    sprites.Add( sprite );
                }
            }

            offset = ( int )( _song.audioOffset - ( sprites[0].start * .5f ) );
            curIndex = startIndex;
            StartCoroutine( LoadTexture( _song ) );
            StartCoroutine( UpdatePreviewImage( _song ) );
        }
    }

    private IEnumerator LoadTexture( Song _song )
    {
        var dir = Path.GetDirectoryName( _song.filePath );

        for ( int i = 0; i < sprites.Count; i++ )
        {
            if ( !textures.ContainsKey( sprites[i].name ) )
            {
                Texture2D tex;
                var path = @Path.Combine( dir, sprites[i].name );
                if ( Path.GetExtension( path ).Contains( ".bmp" ) )
                    {
                        BMPLoader loader = new BMPLoader();
                        BMPImage img = loader.LoadBMP( path );
                        tex = img.ToTexture2D( TextureFormat.RGB24 );
                    }
                else
                {
                    using ( UnityWebRequest www = UnityWebRequestTexture.GetTexture( path ) )
                    {
                        www.method = UnityWebRequest.kHttpVerbGET;
                        using ( DownloadHandlerTexture handler = new DownloadHandlerTexture() )
                        {
                            www.downloadHandler = handler;
                            yield return www.SendWebRequest();

                            if ( www.result == UnityWebRequest.Result.ConnectionError ||
                                 www.result == UnityWebRequest.Result.ProtocolError )
                            {
                                Debug.LogError( $"UnityWebRequest Error : {www.error}" );
                                throw new System.Exception( $"UnityWebRequest Error : {www.error}" );
                            }
                            tex = handler.texture;
                        }
                    }
                }
                textures.Add( sprites[i].name, tex );
                yield return null;
            }
        }

        //yield return StartCoroutine( UpdatePreviewImage( _song ) );
    }

    int startIndex;
    int curIndex;
    SpriteSample curSample;
    int offset;
    private void UpdatePreview( double _playback )
    {
        playback = _playback;
        //if ( sprites.Count > 0 )
        //{
        //    curSample = sprites[curIndex];
        //    if ( curSample.start <= _playback + offset )
        //    {
        //        if ( textures.ContainsKey( curSample.name ) )
        //            image.texture = textures[curSample.name];
        //    }

        //    if ( curSample.end <= _playback + offset )
        //    {
        //        curIndex = curIndex + 1 < sprites.Count ? ++curIndex : startIndex;
        //    }
        //}
    }

    private IEnumerator UpdatePreviewImage( Song _song )
    {
        if ( curIndex < sprites.Count )
             curSample = sprites[curIndex];

        //WaitUntil waitSampleStart = new WaitUntil( () => curSample.start - offset <= playback );
        WaitUntil waitSampleEnd   = new WaitUntil( () => curSample.end   - offset <= playback );

        while ( curIndex < sprites.Count )
        {
            curSample = sprites[curIndex];

            //yield return waitSampleStart;
            if ( textures.ContainsKey( curSample.name ) )
                 image.texture = textures[curSample.name];

            yield return waitSampleEnd;
            curIndex += 1;
        }
    }
}
