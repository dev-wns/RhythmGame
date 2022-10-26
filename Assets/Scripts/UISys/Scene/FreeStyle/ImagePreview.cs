using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ImagePreview : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public FadeBackground bgPrefab;
    public Sprite defaultSprite;
    public RawImage previewImage;
    private ObjectPool<FadeBackground> bgPool;
    private FadeBackground background;
    private Coroutine coroutine;

    private void Awake()
    {
        bgPool = new ObjectPool<FadeBackground>( bgPrefab, 5 );
        scroller.OnSelectSong += ChangeImage;
    }

    private void ChangeImage( Song _song )
    {
        if ( !ReferenceEquals( coroutine, null ) )
        {
            StopCoroutine( coroutine );
            coroutine = null;
        }

        bool isImageType = !_song.hasVideo && !_song.hasSprite;
        coroutine = StartCoroutine( LoadBackground( _song.imagePath, isImageType ) );
    }

    private IEnumerator LoadBackground( string _path, bool _isImageType )
    {
        Sprite sprite;
        bool isExist = System.IO.File.Exists( _path );
        if ( isExist )
        {
            var ext = System.IO.Path.GetExtension( _path );
            if ( ext.Contains( ".bmp" ) )
            {
                BMPLoader loader = new BMPLoader();
                BMPImage img = loader.LoadBMP( _path );
                Texture2D tex = img.ToTexture2D();
                sprite = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );
            }
            else
            {
                using ( UnityWebRequest www = UnityWebRequestTexture.GetTexture( _path ) )
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

                        Texture2D tex = handler.texture;
                        sprite = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );
                    }
                }
            }
        }
        else sprite = defaultSprite;

        background?.Despawn();
        background = bgPool.Spawn();
        background.SetInfo( this, sprite, !isExist );

        if ( _isImageType )
        {
            //previewImage.enabled = true;
            previewImage.texture = sprite.texture;
        }

        // 원시 버젼 메모리 재할당이 큼
        //Texture2D tex = new Texture2D( 1, 1, TextureFormat.ARGB32, false );
        //byte[] binaryData = File.ReadAllBytes( _path );

        //while ( !tex.LoadImage( binaryData ) ) yield return null;
        //background = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );
    }

    public void DeSpawn( FadeBackground _bg )
    {
        bgPool.Despawn( _bg );
    }
}
