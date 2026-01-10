
public static class HitRange
{
    public static double OD = 0;
    public static double Maximum { get; private set; }
    public static double Perfect { get; private set; }
    public static double Great   { get; private set; }
    public static double Good    { get; private set; }
    public static double Bad     { get; private set; }
    public static double Miss    { get; private set; }

    public static void Initialize( double _OD )
    {
        OD = _OD;

        Maximum = 16.5d;
        Perfect = 64.5d  - ( 3 * OD );
        Great   = 97.5d  - ( 3 * OD );
        Good    = 127.5d - ( 3 * OD );
        Bad     = 151.5d - ( 3 * OD );
        Miss    = 188.5d - ( 3 * OD );
    }
}

public static class HitScore
{
    public static readonly int Maximum = 320;
    public static readonly int Perfect = 300;
    public static readonly int Great   = 200;
    public static readonly int Good    = 100;
    public static readonly int Bad     =  50;
    public static readonly int Miss    =   0;
}

public static class HitBonus
{
    public static readonly int Maximum = 2;
    public static readonly int Perfect = 1;
    public static readonly int Great   = -8;
    public static readonly int Good    = -24;
    public static readonly int Bad     = -44;
    public static readonly int Miss    = -100;
}