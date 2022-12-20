using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollBase : MonoBehaviour
{
    public bool IsDuplicate   { get; private set; }
    public bool IsLoop        { get; set; } = false;
    public int  Length        { get; set; }
    public int  CurrentIndex  { get; set; }
    public int  PreviousIndex { get; set; }

    protected virtual void Select( int _pos )
    {
        if ( Length <= 0 ) return;

        CurrentIndex = _pos;
    }

    public virtual void PrevMove()
    {
        if ( CurrentIndex == 0 )
        {
            if ( IsLoop )
            {
                CurrentIndex = Length - 1;
                return;
            }

            IsDuplicate = true;
            return;
        }

        PreviousIndex = CurrentIndex--;
        IsDuplicate = false;
    }

    public virtual void NextMove()
    {
        if ( CurrentIndex == Length - 1 )
        {
            if ( IsLoop )
            {
                CurrentIndex = 0;
                return;
            }

            IsDuplicate = true;
            return;
        }

        PreviousIndex = CurrentIndex++;
        IsDuplicate = false;
    }
}
