using UnityEngine;

public class Results
{
    public int Maximum  { get; set; }
    public int Perfect  { get; set; }
    public int Great    { get; set; }
    public int Good     { get; set; }
    public int Bad      { get; set; }
    public int Miss     { get; set; }
    public int Fast     { get; set; }
    public int Slow     { get; set; }
    public int Combo    { get; set; }
    public int MaxCombo { get; set; }
    public double Score { get; set; }

    public double Accuracy
    {
        get
        {
            double current  = ( HitScore.Perfect * ( Maximum + Perfect ) ) + ( HitScore.Great * Great ) + ( HitScore.Good * Good ) + ( HitScore.Bad * Bad );
            double maxValue = HitScore.Perfect * Count;
            return Global.Math.Abs( maxValue ) <= double.Epsilon ? 0d : ( current / maxValue ) * 100d;
        }
    }

    public int Count => Maximum + Perfect + Great + Good + Bad + Miss;

    public void Clear()
    {
        Maximum  = 0;
        Perfect  = 0;
        Great    = 0;
        Good     = 0;
        Bad      = 0;
        Miss     = 0;
        Fast     = 0;
        Slow     = 0;
        Combo    = 0;
        MaxCombo = 0;
        Score    = 0d;
    }
}
