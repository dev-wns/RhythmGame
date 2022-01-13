using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BackgroundSystem : MonoBehaviour
{
    public FreeStyleScrollSong songScroller;
    public FadeBackground bgPrefab;
    private ObjectPool<FadeBackground> bgPool;

    private Coroutine currentCoroutine;
    private FadeBackground currentBackground;

    private void Awake()
    {
        bgPool = new ObjectPool<FadeBackground>( bgPrefab, 5 );
        songScroller.OnSelectSong += ChangeImage;
    }

    private void ChangeImage( Song _song )
    {
        if ( !ReferenceEquals( currentCoroutine, null ) )
        {
            StopCoroutine( currentCoroutine );
            currentCoroutine = null;
        }

        currentCoroutine = StartCoroutine( LoadBackground( _song.imagePath ) );
    }

    private IEnumerator LoadBackground( string _path )
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture( _path );
        yield return www.SendWebRequest();

        Texture2D tex = ( ( DownloadHandlerTexture )www.downloadHandler ).texture;
        var sprite = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );

        currentBackground?.Despawn();

        currentBackground = bgPool.Spawn();
        currentBackground.SetInfo( sprite );
        currentBackground.system = this;

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
