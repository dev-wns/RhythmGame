namespace WNS.Time.Control
{
    using UnityEngine;

    public class RendererFade : BaseRenderer
    {
        public RendererFade( SpriteRenderer _rdr, float _end, float _duration )
                             : base( _rdr, _end, _duration )
                             => Restart();

        public override bool Process()
        {
            if ( isDuplicateValue )
                return true;

            elapsed += ( offset * Time.deltaTime ) / duration;
            Color newColor = rdr.color;
            newColor.a = elapsed;
            rdr.color = newColor;

            NextSequence?.Process();
            if ( isFadeIn ? elapsed > end :
                            elapsed < end )
            {
                newColor.a = end;
                rdr.color = newColor;
                OnCompleted?.Invoke();
                return true;
            }
            return false;
        }

        public override void Restart()
        {
            float alpha = rdr.color.a;
            offset = end - alpha;
            isFadeIn = alpha < end;
            elapsed = alpha;
            isDuplicateValue = Global.Math.Abs( alpha - end ) < float.Epsilon;
        }
    }
}