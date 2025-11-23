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
    } 

    private void OnDestroy()
    {
        Judgement.OnHitNote     -= UpdateAccuracy;
        NowPlaying.OnInitialize -= Clear;
        NowPlaying.OnClear      -= Clear;
    }

    private void Clear()
    {
        text.text = $"100.00%";
    }

    private void UpdateAccuracy( HitData _hitData )
    {
        text.text = $"{ Judgement.CurrentResult.Accuracy:F2}%";
    }
}
