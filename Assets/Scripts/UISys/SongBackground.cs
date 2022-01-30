using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SongBackground : MonoBehaviour
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        bool isEnabled = GameSetting.BGAOpacity <= .1f ? false : true;

        if ( isEnabled )
        {
            StartCoroutine( LoadBackground( NowPlaying.Inst.CurrentSong.imagePath ) );

            image.color = new Color( 1f, 1f, 1f, GameSetting.BGAOpacity * .01f );
        }
        else
        {
            gameObject.SetActive( false );
        }
    }

    private void OnDestroy()
    {
        if ( image.sprite )
        {
            if ( image.sprite.texture )
            {
                DestroyImmediate( image.sprite.texture );
            }
            Destroy( image.sprite );
        }
    }

    public IEnumerator LoadBackground( string _path )
    {
        bool isExist = System.IO.File.Exists( _path );
        if ( isExist )
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
                    image.sprite = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );

                    float width = tex.width;
                    float height = tex.height;

                    float offsetX = ( float )Screen.width / tex.width;
                    width *= offsetX;
                    height *= offsetX;

                    float offsetY = ( float )Screen.height / height;
                    if ( offsetY > 1f )
                    {
                        width *= offsetY;
                        height *= offsetY;
                    }

                    image.rectTransform.sizeDelta = new Vector2( width, height );
                }
            }
        }
    }
}
