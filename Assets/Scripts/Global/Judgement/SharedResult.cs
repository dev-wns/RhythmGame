
public enum HitResult : int { None = -2, Miss, Maximum, Perfect, Great, Good, Bad }

public struct HitData
{
    public int       lane;
    public double    time; // 키 입력 시간
    public double    diff;
    public HitResult hitResult;
    public KeyState  keyState;

    public HitData( int _lane, double _time, double _diff, HitResult _hitResult, KeyState _keyState )
    {
        lane = _lane;
        time = _time;
        diff = _diff;
        hitResult = _hitResult;
        keyState  = _keyState;
    }

    public HitData( HitResult _hitResult )
    {
        lane = 0;
        time = 0d;
        diff = 0d;
        hitResult = _hitResult;
        keyState  = KeyState.None;
    }
}

//public struct ResultData
//{
//    public int Maximum;
//    public int Perfect;
//    public int Great;
//    public int Good;
//    public int Bad;
//    public int Miss;
//    public int Fast;
//    public int Slow;
//    public int Combo;
//    public int MaxCombo;
//    public double Score;

//    public double Accuracy
//    {
//        get
//        {
//            double current  = ( HitScore.Perfect * ( Maximum + Perfect ) ) + ( HitScore.Great * Great ) + ( HitScore.Good * Good ) + ( HitScore.Bad * Bad );
//            double maxValue = HitScore.Perfect * Count;
//            return Global.Math.Abs( maxValue ) <= double.Epsilon ? 0d : ( current / maxValue ) * 100d;
//        }
//    }
//    public int Count => Maximum + Perfect + Great + Good + Bad + Miss;
//}