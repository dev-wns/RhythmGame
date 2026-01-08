using TMPro;
using UnityEngine;

public class AccuracySystem : MonoBehaviour
{
    [SerializeField] private InGame scene;
    public TextMeshProUGUI text;

    private void Awake()
    {
        Judgement.Inst.OnHitNote += UpdateAccuracy;
        scene.OnClear            += Clear;

        Clear();
    } 

    private void OnDestroy()
    {
        Judgement.Inst.OnHitNote -= UpdateAccuracy;
        scene.OnClear            -= Clear;
    }

    private void Clear()
    {
        text.SetText( "{0:2}%", 100.00f );
    }

    private void UpdateAccuracy( in HitData _hitData )
    {
        text.SetText( "{0:2}%", ( float )Judgement.Inst.Accuracy );
    }
}
