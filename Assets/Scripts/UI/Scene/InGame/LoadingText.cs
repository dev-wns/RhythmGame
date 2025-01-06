using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingText : RotateImage
{
    [Header( "Icon" )]
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI completedText;
    private static string[] textList = new string[] { "로딩중 ", "로딩중 .", "로딩중 ..", "로딩중 ..." };

    [Header( "Sound" )]
    public TextMeshProUGUI numSound;
    public TextMeshProUGUI numDuplicateSound;

    [Header( "Background" )]
    public BGASystem bgaSys;
    public TextMeshProUGUI backgroundType;

    public GameObject spriteGroup;
    public TextMeshProUGUI numTexture;
    public TextMeshProUGUI numDuplicateTexture;
    public TextMeshProUGUI background;
    public TextMeshProUGUI foreground;

    private void Awake()
    {
        InGame scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnLoadEnd += IconDisable;
        transform.position = new Vector3( transform.position.x + GameSetting.GearOffsetX, transform.position.y, transform.position.z );

        bgaSys.OnInitialize += Initialize;
        bgaSys.OnUpdateData += UpdateBackground;

        loadingText.color   = Color.white;
        completedText.color = new Color( 1f, 1f, 1f, 0f );

        StartCoroutine( UpdateKeySoundCount() );
    }

    private void Start()
    {
        if ( !ReferenceEquals( loadingText, null ) )
             StartCoroutine( ChangeText() );
    }

    private void Initialize( BackgroundType _type )
    {
        backgroundType.text = $"{_type}";
        if ( _type == BackgroundType.Sprite )
             spriteGroup.SetActive( true );
    }

    private void UpdateBackground( int _count, int _duplicate, int _background, int _foreground )
    {
        numTexture.text = $"{_count}";
        numDuplicateTexture.text = $"{_duplicate}";
        background.text = $"{_background}";
        foreground.text = $"{_foreground}";
    }

    private IEnumerator UpdateKeySoundCount()
    {
        while ( !NowPlaying.IsStart )
        {
            numSound.text          = $"{AudioManager.Inst.KeySoundCount}";
            numDuplicateSound.text = $"{AudioManager.Inst.TotalKeySoundCount}";
            yield return null;
        }
    }

    public void IconDisable()
    {
        loadingText.DOFade( 0f, .5f );
        completedText.DOFade( 1f, .5f );
    }

    private IEnumerator ChangeText()
    {
        int curIndex = 0;
        loadingText.gameObject.SetActive( true );
        loadingText.text = textList[curIndex];

        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .25f );

            if ( ++curIndex >= textList.Length )
                 curIndex = 0;

            loadingText.text = textList[curIndex];
        }
    }
}
