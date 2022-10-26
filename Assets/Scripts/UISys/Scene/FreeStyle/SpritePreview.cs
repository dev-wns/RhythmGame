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
    private int startIndex;
    private int offset;

    private void Awake()
    {
        image = GetComponent<RawImage>();
        scroller.OnSelectSong += UpdateSpriteSample;
        scroller.OnPlaybackUpdate += ( double _playback ) => playback = _playback;
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
            image.enabled = false;
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

            offset = ( int )( ( sprites[0].start - _song.audioOffset ) * .5f );
            StartCoroutine( LoadTexture( _song ) );
            StartCoroutine( UpdatePreviewImage() );
        }
    }

    private IEnumerator LoadTexture( Song _song )
    {
        var dir = Path.GetDirectoryName( _song.filePath );

        for ( int i = 0; i < sprites.Count; i++ )
        {
            if ( sprites[i].start < _song.previewTime )
                 continue;

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
    }

    private IEnumerator UpdatePreviewImage()
    {
        SpriteSample curSample = new SpriteSample();
        int curIndex = startIndex;

        if ( curIndex < sprites.Count )
             curSample = sprites[curIndex];

        WaitUntil waitSampleEnd   = new WaitUntil( () => curSample.end + offset <= playback );
        while ( curIndex < sprites.Count )
        {
            curSample = sprites[curIndex];

            if ( textures.ContainsKey( curSample.name ) )
            {
                image.enabled = true;
                image.texture = textures[curSample.name];
            }

            yield return waitSampleEnd;
            curIndex += 1;
        }
    }
}
