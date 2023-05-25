using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class IconController : MonoBehaviour
{
    private RectTransform rt;
    private Image image;
    private Sequence sequence;
    private Vector2 sizeDelta;

    public Color Color { set { image.color = value; } }

    private void Awake()
    {
        rt = transform as RectTransform;
        image = GetComponent<Image>();
    }

    private void Start()
    {
        sizeDelta = rt.sizeDelta;
        sequence = DOTween.Sequence().Pause().SetAutoKill( false );
        sequence.Append( rt.DOSizeDelta( sizeDelta * .75f, .125f ) ).
                 Append( rt.DOSizeDelta( sizeDelta, .125f ) );
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }

    public void Play() => sequence.Restart();
}