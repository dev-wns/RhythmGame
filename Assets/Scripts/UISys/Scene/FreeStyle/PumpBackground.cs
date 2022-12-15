using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PumpBackground : MonoBehaviour
{
    public AudioVisualizer visualizer;
    private Image background;

    [Range(0f, 10f)]
    public float minAmount = 0f;

    [Range(0f, 1f)]
    public float decrease;
    [Range(0f, 50f)]
    public float decreasePower;
    public float increasePower;

    private float amountCache;

    private void Awake()
    {
        background = GetComponent<Image>();
        visualizer.OnUpdateBass += ColorUpdate;
    }

    private void ColorUpdate( float _amount )
    {
        float amount = Global.Math.Clamp(_amount - minAmount, 0f, 1f );
        if ( amountCache < amount ) amountCache = amount;
        else                        amountCache -= decrease * Global.Math.Abs( amountCache - _amount );

        //float diffAbs = Global.Math.Abs( amountCache - amount );
        //float value  = Global.Math.Lerp( 0f, 1f, diffAbs * decreasePower * Time.deltaTime );
        //amountCache += amountCache < amount ? value * increasePower : -value;

        background.color = new Color( 1, 1, 1,amountCache );
    }
}