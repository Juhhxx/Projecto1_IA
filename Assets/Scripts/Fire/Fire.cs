using UnityEngine;
using UnityEngine.Profiling;

namespace Scripts.Fire
{
    public class Fire : MonoBehaviour
    {
        [SerializeField] private float _duration;


        [SerializeField] private ExplosionManager _explosion;
        [field:SerializeField] public long PolyRef { get; private set; }
        [field:SerializeField] public long[] NeiRefs { get; private set; }
        [SerializeField] private float _firePropagation = 0.005f;

        private float _lifeTime;

        private void OnEnable()
        {
            _lifeTime = 0f;
        }

        private void OnDisable()
        {
            _explosion.UnSetFire(this);
        }

        public void UpdateOrdered()
        {
            Profiler.BeginSample("DRC Fire");

            _lifeTime += Time.deltaTime;
            if ( _lifeTime >= _duration )
                gameObject.SetActive(false);

            if ( _explosion.Rand.Range(0f, 1f) < _firePropagation )
                _explosion.SetFire( NeiRefs[ _explosion.Rand.Range(0, NeiRefs.Length) ] );
        }

        #if UNITY_EDITOR
        public void SetRefs(ExplosionManager explosion, long selfRef, long[] neiRefs)
        {
            _explosion = explosion;
           PolyRef = selfRef;
           NeiRefs = neiRefs;
        }
        #endif
    }
}