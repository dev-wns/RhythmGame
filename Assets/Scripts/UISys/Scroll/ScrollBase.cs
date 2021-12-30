using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollBase : MonoBehaviour
{
    public List<GameObject> contents;

    public bool IsDuplicate { get; private set; }
    public bool IsLoop { get; protected set; } = false;

    protected GameObject curOption { get; private set; }
    protected int curIndex;

    protected virtual void Awake()
    {
        SelectPosition( 0 );
        CreateContents();
    }

    protected virtual void CreateContents() { }

    protected GameObject GetContent( int _index )
    {
        if ( contents.Count <= _index ) return null;


        return contents[_index];
    }

    protected GameObject GetContent( string _name )
    {
        for ( int i = 0; i < contents.Count; i++ )
        {
            if ( Equals( contents[i].name, _name ) )
                return contents[i];
        }

        return null;
    }

    protected void SelectPosition( int _pos )
    {
        if ( contents.Count <= 0 ) return;

        curIndex = 0;
        curOption = contents[0];
    }

    public virtual void PrevMove()
    {

        if ( curIndex == 0 )
        {
            if ( IsLoop )
            {
                curIndex = contents.Count - 1;
                curOption = contents[curIndex];
                return;
            }

            IsDuplicate = true;
            return;
        }

        curOption = contents[--curIndex];
        IsDuplicate = false;
    }

    public virtual void NextMove()
    {
        if ( curIndex == contents.Count - 1 )
        {
            if ( IsLoop )
            {
                curIndex = 0;
                curOption = contents[curIndex];
                return;
            }

            IsDuplicate = true;
            return;
        }

        curOption = contents[++curIndex];
        IsDuplicate = false;
    }
}
