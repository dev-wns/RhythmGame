using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrollSpeedUI : MonoBehaviour
{
    [SerializeField] private InGame scene;

    [Header( "Canvas" )]
    public GameObject canvas;
    private CanvasGroup canvasGroup;

    [Header( "Background" )]
    public RectTransform background;
    private Image   bgImage;
    private Vector2 bgStartSize;
    private Vector2 bgEndSize;
    private readonly Color BGStartColor = new Color( 0f, 0f, 0f, .75f );

    [Header( "Value Text" )]
    public TextMeshProUGUI speedText;
    private float textStartSize;
    private readonly float TextSizeOffset = 2;
    private Sequence textSeq;

    // Etc
    private readonly float Duration = .15f;
    private bool isActive;
    private bool isProcessing;

    // Time
    private float timer;
    private readonly float PanelShowTime = 1.5f;

    private void Awake()
    {
        GameSetting.OnChangedSpeed += UpdateSpeed;

        bgStartSize = new Vector2( background.sizeDelta.x, 0f );
        bgEndSize = background.sizeDelta;

        bgImage = background.GetComponent<Image>();

        if ( canvas.TryGetComponent( out canvasGroup ) )
        {
            canvasGroup.alpha = 0f;
        }

        textStartSize = speedText.fontSize;
    }

    private void OnDestroy()
    {
        GameSetting.OnChangedSpeed -= UpdateSpeed;
    }

    private void Start()
    {
        textSeq = DOTween.Sequence().Pause().SetAutoKill( false );
        textSeq.Append( DOTween.To( () => textStartSize + TextSizeOffset, x => speedText.fontSize = x, textStartSize, Duration ) );
    }

    private void Update()
    {
        if ( !isActive || isProcessing )
            return;

        timer += Time.deltaTime;
        if ( PanelShowTime < timer )
        {
            isProcessing = true;
            canvasGroup.DOFade( 0f, .15f ).OnComplete( () =>
            {
                canvas.SetActive( false );
                isActive     = false;
                isProcessing = false;
                timer = 0f;
            } );
        }
    }

    private void UpdateSpeed( int _scrollSpeed )
    {
        if ( scene.CurrentAction != SceneAction.Main )
             return;

        timer = 0f;
        speedText.SetText( "{0}", _scrollSpeed );
        textSeq?.Restart();

        if ( isActive )
             return;

        canvas.SetActive( true );
        canvasGroup.DOFade( 1f, .15f );

        // background
        bgImage.color = Color.clear;
        bgImage.DOColor( BGStartColor, Duration );

        background.sizeDelta = bgStartSize;
        background.DOSizeDelta( bgEndSize, Duration );

        // text
        speedText.color = new Color( 1f, 1f, 1f, 0f );
        speedText.DOFade( 1f, Duration );

        isActive = true;
        isProcessing = false;
    }

}