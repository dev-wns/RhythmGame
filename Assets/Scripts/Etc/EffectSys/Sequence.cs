namespace WNS.Time.Control
{
    using System.Collections.Generic;

    public class Sequence
    {
        private List<BaseSequence> actions = new List<BaseSequence>();
        public int Count => actions.Count;
        public int CurrentIndex { get; private set; }
        public bool IsActive { get; private set; }

        public Sequence()
        {
            SequenceManager.Inst.Push( this );
        }

        public void Process()
        {
            if ( !IsActive || actions.Count == 0 )
                return;

            if ( actions[CurrentIndex].Process() )
            {
                if ( CurrentIndex + 1 < actions.Count ) CurrentIndex++;
                else IsActive = false;
            }
        }

        public void Start() => IsActive = true;
        public void Stop() => IsActive = false;

        public void Clear() => actions.Clear();

        public void Restart()
        {
            IsActive = true;
            for ( int i = 0; i <= CurrentIndex; i++ )
            {
                actions[i].Restart();
            }
            CurrentIndex = 0;
        }

        public Sequence Add( BaseSequence _info )
        {
            if ( Count == 0 ) actions.Add( _info );
            else actions[Count - 1].NextSequence = _info;

            return this;
        }

        public Sequence Append( BaseSequence _info )
        {
            actions.Add( _info );
            return this;
        }

        public Sequence AppendInterval( float _t )
        {
            actions.Add( new SequenceInterval( _t ) );
            return this;
        }

        public Sequence OnCompleted( System.Action _action )
        {
            if ( Count > 0 )
                actions[Count - 1].OnCompleted = _action;

            return this;
        }
    }
}