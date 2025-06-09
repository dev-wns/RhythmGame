using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
    [Header( "Icon" )]
    public GameObject icon;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI completedText;
    public  float rotateSpeed = 150f;
    private float curValue;
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
        scene.OnLoadEnd += () =>
        {
            loadingText.DOFade( 0f, .5f );
            completedText.DOFade( 1f, .5f );
        };
        transform.position = new Vector3( transform.position.x + GameSetting.GearOffsetX, transform.position.y, transform.position.z );

        //bgaSys.OnInitialize += Initialize;
        //bgaSys.OnUpdateData += UpdateBackground;

        loadingText.color = Color.white;
        completedText.color = new Color( 1f, 1f, 1f, 0f );

        StartCoroutine( RotateLoadingIcon() );
        StartCoroutine( UpdateKeySoundCount() );
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Start()
    {
        if ( !ReferenceEquals( loadingText, null ) )
              StartCoroutine( ChangeText() );
    }

    private IEnumerator UpdateKeySoundCount()
    {
        while ( !NowPlaying.IsStart )
        {
            numDuplicateSound.text = $"{AudioManager.Inst.TotalKeySoundCount}";
            yield return null;
        }
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

    private IEnumerator RotateLoadingIcon()
    {
        while ( true )
        {
            yield return null;

            curValue -= Time.deltaTime * rotateSpeed;
            icon.transform.rotation = Quaternion.Euler( new Vector3( 0f, 0f, curValue ) );
        }
    }
}
