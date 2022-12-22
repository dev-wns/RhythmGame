namespace WNS.Time.Control
{
    using UnityEngine;

    public class SequenceInterval : BaseSequence
    {
        private float time;

        public SequenceInterval( float _t ) : base( _t ) { }

        public override bool Process()
        {
            time += Time.deltaTime;
            return duration < time;
        }

        public override void Restart()
        {
            time = 0f;
        }
    }
}