public static class Global
{
    public static int BoolToInt ( bool _val )
    {
         if ( _val ) 
            return 1;
         else 
            return 0;
    }

    public static bool IntToBool (int _val )
    {
        if ( _val != 0 )
            return true;
        else 
            return false;
    }
}
