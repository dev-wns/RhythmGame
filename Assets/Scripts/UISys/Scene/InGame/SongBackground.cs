using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Video;

public class SongBackground : MonoBehaviour
{
    private RawImage image;
    private VideoPlayer vp;
    private bool canDestroyTex = false;

    private void PlayVideo() => vp.Play();

    private void Awake()
    {
        image = GetComponent<RawImage>();
        vp = GetComponent<VideoPlayer>();
        bool isEnabled = GameSetting.BGAOpacity <= .1f ? false : true;
        
        if ( isEnabled )
        {
            bool hasVideo = NowPlaying.Inst.CurrentSong.hasVideo;
            if ( hasVideo )
            {
                vp.url = @$"{NowPlaying.Inst.CurrentSong.videoPath}";
                NowPlaying.Inst.OnStart += PlayVideo;
                NowPlaying.Inst.OnPause += OnPause;
            }
            else
            {
                var path = NowPlaying.Inst.CurrentSong.imagePath;
                if ( path == string.Empty )
                {
                    gameObject.SetActive( false );
                }
                else
                {
                    StartCoroutine( LoadBackground( NowPlaying.Inst.CurrentSong.imagePath ) );
                }
                vp.enabled = false;
            }

            image.color = new Color( 1f, 1f, 1f, GameSetting.BGAOpacity * .01f );
        }
        else
        {
            vp.enabled = false;
            gameObject.SetActive( false );
        }
    }

    private void OnPause( bool _isPause )
    {
        if ( _isPause ) vp.Pause();
        else            vp.Play();
    }

    private void OnDestroy()
    {
        NowPlaying.Inst.OnStart -= PlayVideo;
        NowPlaying.Inst.OnPause -= OnPause;
        if ( canDestroyTex && image.texture )
        {
            DestroyImmediate( image.texture );
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
                    image.texture = tex;
                    image.rectTransform.sizeDelta = Globals.GetScreenRatio( tex, new Vector2( Screen.width, Screen.height ) );
                    canDestroyTex = true;
                }
            }
        }
    }
}
