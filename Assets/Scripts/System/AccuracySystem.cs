using TMPro;
using UnityEngine;

public class AccuracySystem : MonoBehaviour
{
    [Header("RateSystem")]
    private InGame scene;
    private Judgement judge;

    public TextMeshProUGUI text;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<InGame>();
        scene.OnReLoad += OnReLoad;
        //scene.OnResult += OnResult;

        InputManager.OnHitNote += UpdateAccuracy;
    }

    private void OnDestroy()
    {
        InputManager.OnHitNote -= UpdateAccuracy;
    }

    //private void OnResult()
    //{
    //    DataStorage.Inst.UpdateResult( HitResult.Accuracy, ( int )( Accuracy * 100f ) );
    //}

    private void OnReLoad()
    {
        text.text = $"100.00%";
    }

    private void UpdateAccuracy( HitData _hitData )
    {
        text.text = $"{ Judgement.CurrentResult.Accuracy:F2}%";
    }
}
