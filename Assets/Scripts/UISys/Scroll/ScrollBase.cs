using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollBase : MonoBehaviour
{
    public bool IsDuplicate { get; private set; }
    public bool IsLoop { get; set; } = false;

    public int currentIndex  { get; protected set; }
    public int previousIndex { get; protected set; }
    public int Length  { get; protected set; }

    protected virtual void Select( int _pos )
    {
        if ( Length <= 0 ) return;

        currentIndex = _pos;
    }

    public virtual void PrevMove()
    {
        if ( currentIndex == 0 )
        {
            if ( IsLoop )
            {
                currentIndex = Length - 1;
                return;
            }

            IsDuplicate = true;
            return;
        }

        previousIndex = currentIndex--;
        IsDuplicate = false;
    }

    public virtual void NextMove()
    {
        if ( currentIndex == Length - 1 )
        {
            if ( IsLoop )
            {
                currentIndex = 0;
                return;
            }

            IsDuplicate = true;
            return;
        }

        previousIndex = currentIndex++;
        IsDuplicate = false;
    }
}
