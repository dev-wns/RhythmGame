namespace WNS.Time.Control
{
    using System.Collections.Generic;
    using UnityEngine;

    public class SequenceManager : Singleton<SequenceManager>
    {
        [SerializeField]
        public List<Sequence> sequences = new List<Sequence>();

        public void Push( Sequence _sequence ) => sequences.Add( _sequence );

        public void Clear() => sequences.Clear();

        private void Update()
        {
            for ( int i = 0; i < sequences.Count; i++ )
            {
                if ( sequences[i].IsActive )
                    sequences[i].Process();
            }
        }
    }
}