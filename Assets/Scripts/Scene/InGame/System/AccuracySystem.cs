using TMPro;
using UnityEngine;

public class AccuracySystem : MonoBehaviour
{
    public TextMeshProUGUI text;

    private void Awake()
    {
        InputManager.OnHitNote += UpdateAccuracy;
        NowPlaying.OnPreInit   += Clear;
        NowPlaying.OnClear     += Clear;
    }

    private void OnDestroy()
    {
        InputManager.OnHitNote -= UpdateAccuracy;
        NowPlaying.OnPreInit   -= Clear;
        NowPlaying.OnClear     -= Clear;
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
