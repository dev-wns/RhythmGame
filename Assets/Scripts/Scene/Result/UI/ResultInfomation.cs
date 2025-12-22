using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;
using UnityEngine.UI;

public class ResultInfomation : MonoBehaviour
{
    public SpriteAtlas rankAtlas;

    [Header( "Song Infomation" )]
    public TextMeshProUGUI title;
    public TextMeshProUGUI artist;

    [Header( "Note Infomation" )]
    public TextMeshProUGUI totalNotes;
    public TextMeshProUGUI noteCount;
    public TextMeshProUGUI sliderCount;

    public TextMeshProUGUI clear;
    public TextMeshProUGUI fullCombo;
    public TextMeshProUGUI allPerfect;

    [Header( "Mode" )]
    public GameObject useModeObj;

    public TextMeshProUGUI autoPlay;
    public TextMeshProUGUI noFail;
    public TextMeshProUGUI noSlider;
    public TextMeshProUGUI fixedBPM;
    public TextMeshProUGUI hardJudge;
    public TextMeshProUGUI onlyPerfect;

    [Header( "Judgement" )]
    public TextMeshProUGUI totalJudge;
    public TextMeshProUGUI maximum;
    public TextMeshProUGUI perfect;
    public TextMeshProUGUI great;
    public TextMeshProUGUI good;
    public TextMeshProUGUI bad;
    public TextMeshProUGUI miss;

    [Header( "Fast Slow" )]
    public TextMeshProUGUI bpm;
    public TextMeshProUGUI fast;
    public TextMeshProUGUI slow;

    [Header( "Result" )]
    public TextMeshProUGUI maxCombo;
    public TextMeshProUGUI score;
    public TextMeshProUGUI accuracy;

    [Header( "Rank" )]
    public Image rank;

    [Header( "Date" )]
    public TextMeshProUGUI date;

    [Header( "Background" )]
    public Sprite defaultOrigin;
    public Image originBg;
    private Sprite spriteBg;
    private Texture2D texture;

    [Header( "GameSettings" )]
    public TextMeshProUGUI scrollSpeed;
    public TextMeshProUGUI pitch;
    public TextMeshProUGUI random;

    private Color DisableColor = new Color( 1f, 1f, 1f, .25f );
    private readonly float duration = 1.5f;
    private void TextProgressEffect( in TextMeshProUGUI _text, int _value ) => _text.SetText( "{0}", _value );

    private void Awake()
    {
        var song   = NowPlaying.CurrentSong;
        var result = Judgement.CurrentResult;

        // Song Infomation
        title.text  = $"{song.title} [{song.version}]";
        artist.SetText( song.artist );

        // Note Infomation
        totalNotes.SetText(  "{0}", NowPlaying.TotalNote + NowPlaying.TotalSlider );
        noteCount.SetText(   "{0}", NowPlaying.TotalNote );
        sliderCount.SetText( "{0}", NowPlaying.TotalSlider );

        // Clear Type
        if ( result.Great + result.Good + result.Bad + result.Miss == 0 )
        {
            allPerfect.color = Color.white;
            fullCombo.color  = DisableColor;
            clear.color      = DisableColor;
        }
        else if ( result.Miss == 0 )
        {
            allPerfect.color = DisableColor;
            fullCombo.color  = Color.white;
            clear.color      = DisableColor;
        }
        else
        {
            allPerfect.color = DisableColor;
            fullCombo.color  = DisableColor;
            clear.color      = Color.white;
        }

        //Mode
        noSlider.color    = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider   ) ? Color.white : DisableColor;
        autoPlay.color    = GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay   ) ? Color.white : DisableColor;
        noFail.color      = GameSetting.CurrentGameMode.HasFlag( GameMode.NoFail     ) ? Color.white : DisableColor;
        fixedBPM.color    = GameSetting.CurrentGameMode.HasFlag( GameMode.FixedBPM   ) ? Color.white : DisableColor;
        onlyPerfect.color = GameSetting.CurrentGameMode.HasFlag( GameMode.ConvertKey ) ? Color.white : DisableColor;

        // Judgement
        totalJudge.SetText( "{0}", NowPlaying.TotalJudge );
        DOVirtual.Int(   0,          result.Maximum,  duration, x => TextProgressEffect( maximum,  x ) );
        DOVirtual.Int(   0,          result.Perfect,  duration, x => TextProgressEffect( perfect,  x ) );
        DOVirtual.Int(   0,          result.Great,    duration, x => TextProgressEffect( great,    x ) );
        DOVirtual.Int(   0,          result.Good,     duration, x => TextProgressEffect( good,     x ) );
        DOVirtual.Int(   0,          result.Bad,      duration, x => TextProgressEffect( bad,      x ) );
        DOVirtual.Int(   0,          result.Miss,     duration, x => TextProgressEffect( miss,     x ) );
        DOVirtual.Int(   0,          result.Combo,    duration, x => TextProgressEffect( maxCombo, x ) );
        DOVirtual.Int(   0, (  int  )result.Score,    duration, x => TextProgressEffect( score,    x ) );
        DOVirtual.Float( 0, ( float )result.Accuracy, duration, x => accuracy.SetText( "{0:2}%",  x ) );
        

        // fast slow
        fast.SetText( "{0}", result.Fast );
        slow.SetText( "{0}", result.Slow );

        // bpm
        if ( Global.Math.Abs( GameSetting.CurrentPitch - 1f ) < .0001f )
        {
            int mainBPM = Mathf.RoundToInt( ( float )song.mainBPM );
            if ( song.minBpm == song.maxBpm ) bpm.SetText( "{0}", mainBPM );
            else                              bpm.SetText( "{0} ({1} ~ {2})", mainBPM, song.minBpm, song.maxBpm );
        }
        else
        {
            int mainBPM = Mathf.RoundToInt( ( float )song.mainBPM * GameSetting.CurrentPitch  );
            if ( song.minBpm == song.maxBpm ) bpm.SetText( "{0}", mainBPM );
            else                              bpm.SetText( "{0} ({1} ~ {2})", mainBPM, Mathf.RoundToInt( song.minBpm * GameSetting.CurrentPitch ), Mathf.RoundToInt( song.maxBpm * GameSetting.CurrentPitch ) );
        }
        bpm.color = GameSetting.CurrentPitch < 1f ? new Color( .5f, .5f, 1f ) :
                    GameSetting.CurrentPitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        // Score
        rank.sprite = result.Accuracy >= 95d ? rankAtlas.GetSprite( "Ranking-S" ) :
                      result.Accuracy >= 90d ? rankAtlas.GetSprite( "Ranking-A" ) :
                      result.Accuracy >= 85d ? rankAtlas.GetSprite( "Ranking-B" ) :
                      result.Accuracy >= 80d ? rankAtlas.GetSprite( "Ranking-C" ) :
                                               rankAtlas.GetSprite( "Ranking-D" );

        // GameSettings
        scrollSpeed.SetText( "{0}", GameSetting.ScrollSpeed );
        random.SetText( GameSetting.ToString( GameSetting.CurrentRandom ) );
        pitch.SetText( "x{0:1}", GameSetting.CurrentPitch );
        pitch.color = GameSetting.CurrentPitch < 1f ? new Color( .5f, .5f, 1f ) :
                      GameSetting.CurrentPitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        useModeObj.SetActive( GameSetting.HasFlag( GameMode.AutoPlay ) );
        date.SetText( DateTime.Now.ToString( "yyyy. MM. dd @ hh:mm:ss tt" ) );

        // Background
        StartCoroutine( LoadBackground( NowPlaying.CurrentSong.imagePath ) );
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        if ( texture != null )
             DestroyImmediate( texture, true );
    }

    private IEnumerator LoadBackground( string _path )
    {
        if ( !System.IO.File.Exists( _path ) )
        {
            originBg.sprite = defaultOrigin;
            originBg.rectTransform.sizeDelta = Global.Screen.GetRatio( defaultOrigin.texture );
            yield break;
        }

        var ext = System.IO.Path.GetExtension( _path );
        if ( ext.Contains( ".bmp" ) )
        {
            BMPLoader loader = new BMPLoader();
            BMPImage img = loader.LoadBMP( _path );
            texture = img.ToTexture2D();
            spriteBg = Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( .5f, .5f ), 100, 0, SpriteMeshType.FullRect );
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
                        throw new Exception( $"UnityWebRequest Error : {www.error}" );
                    }

                    texture = handler.texture;
                    spriteBg = Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( .5f, .5f ), 100, 0, SpriteMeshType.FullRect );
                }
            }
        }

        originBg.sprite = spriteBg;
        originBg.rectTransform.sizeDelta = Global.Screen.GetRatio( spriteBg.texture );
    }
}