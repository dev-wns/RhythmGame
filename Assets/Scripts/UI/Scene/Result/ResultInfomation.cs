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

    private Color DisableColor = new Color( 1f, 1f, 1f, .25f );
    private readonly float duration = 1f;
    private void TextProgressEffect( in TextMeshProUGUI _text, int _value ) => _text.text = $"{_value}";

    private void Awake()
    {
        Result scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Result>();

        var song   = NowPlaying.CurrentSong;
        var result = Judgement.CurrentResult;

        // Song Infomation
        title.text  = $"{song.title} [{song.version}]";
        artist.text = song.artist;

        // Note Infomation
        totalNotes.text  = $"{NowPlaying.TotalNote + NowPlaying.TotalSlider}";
        noteCount.text   = $"{NowPlaying.TotalNote}";
        sliderCount.text = $"{NowPlaying.TotalSlider}";

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
        noSlider.color    = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider      ) ? Color.white : DisableColor;
        autoPlay.color    = GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay      ) ? Color.white : DisableColor;
        noFail.color      = GameSetting.CurrentGameMode.HasFlag( GameMode.NoFail        ) ? Color.white : DisableColor;
        fixedBPM.color    = GameSetting.CurrentGameMode.HasFlag( GameMode.FixedBPM      ) ? Color.white : DisableColor;
        hardJudge.color   = GameSetting.CurrentGameMode.HasFlag( GameMode.HardJudge     ) ? Color.white : DisableColor;
        onlyPerfect.color = GameSetting.CurrentGameMode.HasFlag( GameMode.KeyConversion ) ? Color.white : DisableColor;

        // Judgement
        totalJudge.text = $"{NowPlaying.TotalJudge}";
        DOTween.To( () => 0, x => TextProgressEffect( maximum,  x ), result.Maximum, duration );
        DOTween.To( () => 0, x => TextProgressEffect( perfect,  x ), result.Perfect, duration );
        DOTween.To( () => 0, x => TextProgressEffect( great,    x ), result.Great,   duration );
        DOTween.To( () => 0, x => TextProgressEffect( good,     x ), result.Good,    duration );
        DOTween.To( () => 0, x => TextProgressEffect( bad,      x ), result.Bad,     duration );
        DOTween.To( () => 0, x => TextProgressEffect( miss,     x ), result.Miss,    duration );
        DOTween.To( () => 0, x => TextProgressEffect( maxCombo, x ), result.Combo,   duration );
        DOTween.To( () => 0, x => TextProgressEffect( score,    x ), ( int )result.Score,   duration );
        DOTween.To( () => 0, x => accuracy.text = $"{( x * .01d ):F2}%", result.Accuracy, duration );

        // fast slow
        fast.text = $"{result.Fast}";
        slow.text = $"{result.Slow}";

        // bpm
        var pitch = GameSetting.CurrentPitch;
        if ( Global.Math.Abs( pitch - 1f ) < .0001f )
        {
            int mainBPM = Mathf.RoundToInt( ( float )song.mainBPM );
            if ( song.minBpm == song.maxBpm ) bpm.text = $"{mainBPM}";
            else bpm.text = $"{mainBPM} ({song.minBpm} ~ {song.maxBpm})";
        }
        else
        {
            int mainBPM = Mathf.RoundToInt( ( float )song.mainBPM * pitch  );
            if ( song.minBpm == song.maxBpm ) bpm.text = $"{mainBPM}";
            else bpm.text = $"{mainBPM} ({Mathf.RoundToInt( song.minBpm * pitch )} ~ {Mathf.RoundToInt( song.maxBpm * pitch )})";
        }
        bpm.color = pitch < 1f ? new Color( .5f, .5f, 1f ) :
                    pitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        // Score
        rank.sprite = result.Accuracy >= 9500 ? rankAtlas.GetSprite( "Ranking-S" ) :
                      result.Accuracy >= 9000 ? rankAtlas.GetSprite( "Ranking-A" ) :
                      result.Accuracy >= 8500 ? rankAtlas.GetSprite( "Ranking-B" ) :
                      result.Accuracy >= 8000 ? rankAtlas.GetSprite( "Ranking-C" ) :
                                                rankAtlas.GetSprite( "Ranking-D" );


        bool shouldMakeRecord = GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay );//||
                                                                                         //GameSetting.CurrentGameMode.HasFlag( GameMode.NoFail )   ||
                                                                                         //GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );

        useModeObj.SetActive( shouldMakeRecord );
        date.text = DateTime.Now.ToString( "yyyy. MM. dd @ hh:mm:ss tt" );

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
            spriteBg = Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );
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
                    spriteBg = Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), new Vector2( .5f, .5f ), GameSetting.PPU, 0, SpriteMeshType.FullRect );
                }
            }
        }

        originBg.sprite = spriteBg;
        originBg.rectTransform.sizeDelta = Global.Screen.GetRatio( spriteBg.texture );
    }
}