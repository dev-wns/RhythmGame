using UnityEngine;
using UnityEngine.UI;

public class LoopAlpha : MonoBehaviour
{
    [Range(0f, 1f)]
    public float minAlpha = 0f;
    public float speed = 1;
    private float time;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        time += Time.deltaTime;

        float alpha = minAlpha + ( ( 1f + Mathf.Cos( time * speed ) ) * .5f * ( 1f - minAlpha ) );
        image.color = new Color( 1f, 1f, 1f, alpha );
    }
}
