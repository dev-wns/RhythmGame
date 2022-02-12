using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class ResultInfomation : MonoBehaviour
{
    [Header("Song Infomation")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI artist;

    [Header( "Hit Count" )]
    public TextMeshProUGUI totalNotes;
    public TextMeshProUGUI perfect, great, good, bad, miss;

    [Header( "Result" )]
    public TextMeshProUGUI maxCombo;
    public TextMeshProUGUI score;
    public TextMeshProUGUI rate;

    [Header( "Rank" )]
    public Image rank;

    [Header( "Background" )]
    public Sprite defaultOrigin;
    public Sprite defaultCircle;
    public Image originBg;
    public Image circleBg;
    private Sprite spriteBg;
    private Texture2D tex;

    private void Awake()
    {
        Judgement judge = null;
        var obj = GameObject.FindGameObjectWithTag( "Judgement" );
        obj?.TryGetComponent( out judge );
        if ( judge == null ) return;

        var song = NowPlaying.Inst.CurrentSong;
        // Song Infomation
        title.text = song.title;
        artist.text = song.artist;

        // Hit Count 
        totalNotes.text = ( song.noteCount + song.sliderCount ).ToString();
        perfect.text    = judge.GetResult( HitResult.Perfect ).ToString();
        great.text      = judge.GetResult( HitResult.Great ).ToString();
        good.text       = judge.GetResult( HitResult.Good ).ToString();
        bad.text        = judge.GetResult( HitResult.Bad ).ToString();
        miss.text       = judge.GetResult( HitResult.Miss ).ToString();

        // Result
        maxCombo.text = judge.GetResult( HitResult.Combo ).ToString();
        score.text    = judge.GetResult( HitResult.Score ).ToString();
        rate.text     = $"{( judge.GetResult( HitResult.Rate ) / 100d ):F2}%";

        // background
        StartCoroutine( LoadBackground( NowPlaying.Inst.CurrentSong.imagePath ) );

        Destroy( judge );
    }

    private void OnDestroy()
    {
        if ( tex ) DestroyImmediate( tex );
    }

    private IEnumerator LoadBackground( string _path )
    {
        if ( !System.IO.File.Exists( _path ) )
        {
            originBg.sprite = defaultOrigin;
            circleBg.sprite = defaultCircle;

            originBg.rectTransform.sizeDelta = Globals.GetScreenRatio( defaultOrigin.texture, new Vector2( Screen.width, Screen.height ) );
            circleBg.rectTransform.sizeDelta = Globals.GetScreenRatio( defaultCircle.texture, new Vector2( 500f, 500f ) );
            yield break;
        }

        Texture2D tex;
        var ext = System.IO.Path.GetExtension( _path );
        if ( ext.Contains( ".bmp" ) )
        {
            BMPLoader loader = new BMPLoader();
            BMPImage img = loader.LoadBMP( _path );
            tex = img.ToTexture2D();
            spriteBg = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );
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

                    tex = handler.texture;
                    spriteBg = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );
                }
            }
        }

        originBg.sprite = spriteBg;
        circleBg.sprite = spriteBg;

        originBg.rectTransform.sizeDelta = Globals.GetScreenRatio( spriteBg.texture, new Vector2( Screen.width, Screen.height ) );
        circleBg.rectTransform.sizeDelta = Globals.GetScreenRatio( spriteBg.texture, new Vector2( 500f, 500f ) );
    }
}
