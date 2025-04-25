using UnityEngine;
using UnityEngine.Profiling;

namespace Scripts.Fire
{
    public class Fire : MonoBehaviour
    {
        [SerializeField] private float _duration;

        [field:SerializeField] public long PolyRef { get; private set; }
        [field:SerializeField] public long[] NeiRefs { get; private set; }
        [SerializeField] private float _firePropagation = 0.005f;

        private float _lifeTime;
        public bool ON { get; private set; } = false;

        private void OnEnable()
        {
            _lifeTime = 0f;
            ON = true;
        }

        private void OnDisable()
        {
            ON = false;
        }

        public void UpdateOrdered()
        {
            Profiler.BeginSample("DRC Fire");

            _lifeTime += Time.deltaTime;
            if ( _lifeTime >= _duration )
                gameObject.SetActive(false);

            if ( ExplosionManager.Rand.Range(0f, 1f) < _firePropagation )
                ExplosionManager.SetFire( NeiRefs[ ExplosionManager.Rand.Range(0, NeiRefs.Length) ] );
        }

        #if UNITY_EDITOR
        public void SetRefs(long selfRef, long[] neiRefs)
        {
           PolyRef = selfRef;
           NeiRefs = neiRefs;
        }
        #endif
    }
}