using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SpritePreview : FreeStylePreview
{
    private List<SpriteSample> sprites = new List<SpriteSample>();
    private Dictionary<string/* Sprite Name */, Texture2D> textures = new Dictionary<string, Texture2D>();
    private int startIndex;
    private double offset = 0d;
    private double previewTime;
    private Coroutine updatePreview;

    private void OnDestroy()
    {
        Clear();
    }

    private void Clear()
    {
        StopAllCoroutines();
        updatePreview = null;

        foreach ( Texture2D texture in textures.Values )
        {
            DestroyImmediate( texture );
        }
        textures.Clear();
        sprites.Clear();
    }

    protected override void Restart( Song _song )
    {
        if ( !ReferenceEquals( updatePreview, null ) )
        {
            StopCoroutine( updatePreview );
            updatePreview = null;
        }

        if ( !_song.hasVideo && _song.hasSprite )
        {
            updatePreview = StartCoroutine( UpdatePreviewImage() );
        }
    }

    protected override void UpdatePreview( Song _song )
    {
        Clear();

        if ( !_song.hasVideo && _song.hasSprite )
        {
            previewImage.enabled = false;
            previewTime = _song.previewTime <= 0 ? _song.totalTime * .314f : _song.previewTime;
            using ( StreamReader reader = new StreamReader( @$"\\?\{_song.filePath}" ) )
            {
                string line;
                while ( ( line = reader.ReadLine() ) != "[Sprites]" ) { }
                while ( ( line = reader.ReadLine() ) != "[Samples]" )
                {
                    SpriteSample sprite;
                    var split = line.Split( ',' );

                    sprite.type = ( SpriteType )int.Parse( split[0] );
                    if ( sprite.type != SpriteType.Background )
                        continue;

                    sprite.start = double.Parse( split[1] );
                    sprite.end = double.Parse( split[2] );
                    sprite.name = split[3];

                    //if ( sprites.Count == 0 )
                    //     offset = ( sprite.start - _song.audioOffset ) * .5f;

                    if ( sprite.start < previewTime - offset )
                        startIndex = sprites.Count;

                    sprites.Add( sprite );
                }
            }

            StartCoroutine( LoadTexture( _song ) );
            updatePreview = StartCoroutine( UpdatePreviewImage() );
        }
    }

    private IEnumerator LoadTexture( Song _song )
    {
        var dir = Path.GetDirectoryName( _song.filePath );
        for ( int i = startIndex; i < sprites.Count; i++ )
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
                    using ( UnityWebRequest www = UnityWebRequestTexture.GetTexture( path, true ) )
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

        WaitUntil waitSampleStart = new WaitUntil( () => curSample.start <= AudioManager.Inst.Position - offset );
        WaitUntil waitSampleEnd   = new WaitUntil( () => curSample.end   <= AudioManager.Inst.Position - offset );
        yield return waitSampleStart;

        // Wait First Texture
        yield return new WaitUntil( () => textures.ContainsKey( curSample.name ) );
        tf.sizeDelta = Global.Math.GetScreenRatio( textures[curSample.name], sizeCache );
        previewImage.enabled = true;

        while ( curIndex < sprites.Count )
        {
            yield return waitSampleStart;

            if ( textures.ContainsKey( curSample.name ) )
            {
                previewImage.texture = textures[curSample.name];
            }

            yield return waitSampleEnd;
            if ( ++curIndex < sprites.Count )
                curSample = sprites[curIndex];
        }
    }
}
