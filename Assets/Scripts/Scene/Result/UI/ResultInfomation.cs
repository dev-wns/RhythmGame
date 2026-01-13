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

    [Header( "Song Info" )]
    public TextMeshProUGUI title;
    public TextMeshProUGUI artist;

    [Header( "Note Info" )]
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
        var song   = Browser.Inst.SelectedSong;

        bool isConvert   = GameSetting.CurrentGameMode.HasFlag( GameMode.ConvertKey ) &&  song.keyCount == 7;
        int  totalNote   = isConvert ? song.noteCount   - song.delNoteCount   : song.noteCount;
        int  totalSlider = isConvert ? song.sliderCount - song.delSliderCount : song.sliderCount;

        bool isNoSlider = GameSetting.CurrentGameMode.HasFlag( GameMode.NoSlider );
        totalNote       = isNoSlider ? totalNote + totalSlider : totalNote;
        totalSlider     = isNoSlider ? 0 : totalSlider;

        // Song Info
        title.text  = $"{song.title} [{song.version}]";
        artist.SetText( song.artist );

        // Note Info
        totalNotes.SetText(  "{0}", totalNote + totalSlider );
        noteCount.SetText(   "{0}", totalNote );
        sliderCount.SetText( "{0}", totalSlider );

        // Clear Type
        if ( Judgement.Inst.Great + Judgement.Inst.Good + Judgement.Inst.Bad + Judgement.Inst.Miss == 0 )
        {
            allPerfect.color = Color.white;
            fullCombo.color  = DisableColor;
            clear.color      = DisableColor;
        }
        else if ( Judgement.Inst.Miss == 0 )
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
        totalJudge.SetText( "{0}", totalNote + totalSlider );
        DOVirtual.Int(   0,          Judgement.Inst.Maximum,  duration, x => TextProgressEffect( maximum,  x ) );
        DOVirtual.Int(   0,          Judgement.Inst.Perfect,  duration, x => TextProgressEffect( perfect,  x ) );
        DOVirtual.Int(   0,          Judgement.Inst.Great,    duration, x => TextProgressEffect( great,    x ) );
        DOVirtual.Int(   0,          Judgement.Inst.Good,     duration, x => TextProgressEffect( good,     x ) );
        DOVirtual.Int(   0,          Judgement.Inst.Bad,      duration, x => TextProgressEffect( bad,      x ) );
        DOVirtual.Int(   0,          Judgement.Inst.Miss,     duration, x => TextProgressEffect( miss,     x ) );
        DOVirtual.Int(   0,          Judgement.Inst.Combo,    duration, x => TextProgressEffect( maxCombo, x ) );
        DOVirtual.Int(   0, (  int  )Judgement.Inst.Score,    duration, x => TextProgressEffect( score,    x ) );
        DOVirtual.Float( 0, ( float )Judgement.Inst.Accuracy, duration, x => accuracy.SetText( "{0:2}%",  x ) );
        

        // fast slow
        fast.SetText( "{0}", Judgement.Inst.Fast );
        slow.SetText( "{0}", Judgement.Inst.Slow );

        // bpm
        if ( Global.Math.Abs( GameSetting.SoundPitch - 1f ) < .0001f )
        {
            int mainBPM = Mathf.RoundToInt( ( float )song.mainBPM );
            if ( song.minBpm == song.maxBpm ) bpm.SetText( "{0}", mainBPM );
            else                              bpm.SetText( "{0} ({1} ~ {2})", mainBPM, song.minBpm, song.maxBpm );
        }
        else
        {
            int mainBPM = Mathf.RoundToInt( ( float )song.mainBPM * GameSetting.SoundPitch  );
            if ( song.minBpm == song.maxBpm ) bpm.SetText( "{0}", mainBPM );
            else                              bpm.SetText( "{0} ({1} ~ {2})", mainBPM, Mathf.RoundToInt( song.minBpm * GameSetting.SoundPitch ), Mathf.RoundToInt( song.maxBpm * GameSetting.SoundPitch ) );
        }
        bpm.color = GameSetting.SoundPitch < 1f ? new Color( .5f, .5f, 1f ) :
                    GameSetting.SoundPitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        // Score
        rank.sprite = Judgement.Inst.Accuracy >= 95d ? rankAtlas.GetSprite( "Ranking-S" ) :
                      Judgement.Inst.Accuracy >= 90d ? rankAtlas.GetSprite( "Ranking-A" ) :
                      Judgement.Inst.Accuracy >= 85d ? rankAtlas.GetSprite( "Ranking-B" ) :
                      Judgement.Inst.Accuracy >= 80d ? rankAtlas.GetSprite( "Ranking-C" ) :
                                                       rankAtlas.GetSprite( "Ranking-D" );

        // GameSettings
        scrollSpeed.SetText( "{0}", GameSetting.ScrollSpeed );
        random.SetText( GameSetting.ToString( GameSetting.CurrentRandom ) );
        pitch.SetText( "x{0:1}", GameSetting.SoundPitch );
        pitch.color = GameSetting.SoundPitch < 1f ? new Color( .5f, .5f, 1f ) :
                      GameSetting.SoundPitch > 1f ? new Color( 1f, .5f, .5f ) : Color.white;

        useModeObj.SetActive( GameSetting.CurrentGameMode.HasFlag( GameMode.AutoPlay ) );
        date.SetText( DateTime.Now.ToString( "yyyy. MM. dd @ hh:mm:ss tt" ) );

        // Background
        StartCoroutine( LoadBackground( Browser.Inst.SelectedSong.imagePath ) );
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
            originBg.rectTransform.sizeDelta = Global.UI.Screen.GetRatio( defaultOrigin.texture );
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
        originBg.rectTransform.sizeDelta = Global.UI.Screen.GetRatio( spriteBg.texture );
    }
}