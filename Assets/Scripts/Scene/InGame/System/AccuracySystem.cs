using TMPro;
using UnityEngine;

public class AccuracySystem : MonoBehaviour
{
    public TextMeshProUGUI text;

    private void Awake()
    {
        Judgement.OnHitNote     += UpdateAccuracy;
        NowPlaying.OnInitialize += Clear;
        NowPlaying.OnClear      += Clear;

        text.SetText( "{0:2}%", 100.00f );
    } 

    private void OnDestroy()
    {
        Judgement.OnHitNote     -= UpdateAccuracy;
        NowPlaying.OnInitialize -= Clear;
        NowPlaying.OnClear      -= Clear;
    }

    private void Clear()
    {
        text.SetText( "{0:2}%", 100.00f );
    }

    private void UpdateAccuracy( HitData _hitData )
    {
        text.SetText( "{0:2}%", ( float )Judgement.CurrentResult.Accuracy );
    }
}
