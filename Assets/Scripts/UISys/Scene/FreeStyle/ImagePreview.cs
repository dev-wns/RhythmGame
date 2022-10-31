using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using DG.Tweening;

public class ImagePreview : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public FadeBackground bgPrefab;
    public Sprite defaultSprite;
    public RectTransform previewObject;
    private RawImage previewImage;
    private Texture2D prevTexture;
    private ObjectPool<FadeBackground> bgPool;
    private FadeBackground background;
    private Coroutine curLoadBG, curLoadPreview;

    private void Awake()
    {
        bgPool = new ObjectPool<FadeBackground>( bgPrefab, 5 );
        scroller.OnSelectSong += ChangeImage;

        if ( !previewObject.TryGetComponent( out previewImage ) )
             Debug.LogError( "Preview BGA object is not found." );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        ClearPreviewTexture();
    }

    private void ClearPreviewTexture()
    {
        if ( prevTexture )
        {
            if ( ReferenceEquals( prevTexture, defaultSprite.texture ) )
                 return;

            DestroyImmediate( prevTexture );
        }
    }

    private void ChangeImage( Song _song )
    {
        if ( !ReferenceEquals( curLoadBG, null ) )
        {
            StopCoroutine( curLoadBG );
            curLoadBG = null;
        }
        curLoadBG = StartCoroutine( LoadBackground( _song.imagePath ) );

        if ( !_song.hasVideo && !_song.hasSprite )
        {
            ClearPreviewTexture();
            if ( !ReferenceEquals( curLoadPreview, null ) )
            {
                StopCoroutine( curLoadPreview );
                curLoadPreview = null;
            }

            curLoadPreview = StartCoroutine( LoadPreviewImage( _song.imagePath ) );
        }
    }

    private IEnumerator LoadBackground( string _path )
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

        // 원시 버젼 메모리 재할당이 큼
        //Texture2D tex = new Texture2D( 1, 1, TextureFormat.ARGB32, false );
        //byte[] binaryData = File.ReadAllBytes( _path );

        //while ( !tex.LoadImage( binaryData ) ) yield return null;
        //background = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );
    }

    private IEnumerator LoadPreviewImage( string _path )
    {
        bool isExist = System.IO.File.Exists( _path );
        if ( isExist )
        {
            var ext = System.IO.Path.GetExtension( _path );
            if ( ext.Contains( ".bmp" ) )
            {
                BMPLoader loader = new BMPLoader();
                BMPImage img = loader.LoadBMP( _path );
                prevTexture = img.ToTexture2D();
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

                        prevTexture = handler.texture;
                    }
                }
            }
        }
        else
            prevTexture = defaultSprite.texture;

        var texSize = Global.Math.GetScreenRatio( prevTexture, new Vector2( 752f, 423f ) );
        previewObject.sizeDelta = texSize;

        previewImage.texture = prevTexture;
        previewObject.localScale = new Vector3( 0f, 1f, 1f );
        previewImage.enabled = true;
        previewObject.DOScaleX( 1f, .25f );
    }

    public void DeSpawn( FadeBackground _bg )
    {
        bgPool.Despawn( _bg );
    }
}
