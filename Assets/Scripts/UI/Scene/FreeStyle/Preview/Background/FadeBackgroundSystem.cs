using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class FadeBackgroundSystem : MonoBehaviour
{
    public FreeStyleMainScroll scroller;
    public FadeBackground bgPrefab;
    public Sprite defaultSprite;
    private ObjectPool<FadeBackground> bgPool;
    private FadeBackground background;
    private Coroutine coroutine;

    private void Awake()
    {
        bgPool = new ObjectPool<FadeBackground>( bgPrefab, 1 );
        scroller.OnSelectSong += ChangeImage;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void ChangeImage( Song _song )
    {
        if ( !ReferenceEquals( coroutine, null ) )
        {
            StopCoroutine( coroutine );
            coroutine = null;
        }
        coroutine = StartCoroutine( LoadBackground( _song.imagePath ) );
    }

    private IEnumerator LoadBackground( string _path )
    {
        Sprite sprite;
        bool isExist = System.IO.File.Exists( _path );
        bool useDefault = false;
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
                using ( UnityWebRequest www = UnityWebRequestTexture.GetTexture( _path, true ) )
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
                        try
                        { sprite = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect ); }
                        catch ( System.Exception )
                        {
                            sprite = defaultSprite;
                            useDefault = true;
                        }
                    }
                }
            }
        }
        else
        {
            sprite = defaultSprite;
            useDefault = true;
        }

        background?.Despawn();
        background = bgPool.Spawn();
        background.SetInfo( this, sprite, useDefault );

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
