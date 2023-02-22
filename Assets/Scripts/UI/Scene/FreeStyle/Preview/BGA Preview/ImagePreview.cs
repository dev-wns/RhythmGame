using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ImagePreview : FreeStylePreview
{
    public Sprite defaultSprite;
    private Texture2D prevTexture;
    private Coroutine coroutine;

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

    protected override void Restart( Song _song ) { }

    protected override void UpdatePreview( Song _song )
    {
        if ( !ReferenceEquals( coroutine, null ) )
        {
            StopCoroutine( coroutine );
            coroutine = null;
        }

        if ( !_song.hasVideo && !_song.hasSprite )
        {
            ClearPreviewTexture();
            coroutine = StartCoroutine( LoadPreviewImage( _song.imagePath ) );
        }
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


        tf.sizeDelta = Global.Math.GetScreenRatio( prevTexture, sizeCache );
        previewImage.texture = prevTexture;
        previewImage.enabled = true;
        //PlayScaleEffect();
    }
}
