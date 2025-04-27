using UnityEngine;
using UnityEngine.Profiling;

namespace Scripts.Fire
{
    /// <summary>
    /// Represents a fire entity that can spread over time and extinguishes after a set duration.
    /// </summary>
    public class Fire : MonoBehaviour
    {
        [SerializeField] private float _duration;


        [SerializeField] private ExplosionManager _explosion;
        [field:SerializeField] public long PolyRef { get; private set; }
        [field:SerializeField] public long[] NeiRefs { get; private set; }
        [SerializeField] private float _firePropagation = 0.005f;

        private float _lifeTime;

        /// <summary>
        /// Resets the lifetime timer when the fire is activated.
        /// </summary>
        private void OnEnable()
        {
            _lifeTime = 0f;
        }

        /// <summary>
        /// Unregisters the fire from the manager when disabled.
        /// </summary>
        private void OnDisable()
        {
            _explosion.UnSetFire(this);
        }

        /// <summary>
        /// Updates the fire behavior:
        /// - Counts up lifetime.
        /// - Extinguishes after duration.
        /// - Attempts to spread to a neighbor with a small probability.
        /// </summary>
        public void UpdateOrdered()
        {
            Profiler.BeginSample("DRC Fire");

            _lifeTime += Time.deltaTime;
            if ( _lifeTime >= _duration )
                gameObject.SetActive(false);

            // Attempt to spread to a random neighboring polygon
            if ( _explosion.Rand.Range(0f, 1f) < _firePropagation )
                _explosion.SetFire( NeiRefs[ _explosion.Rand.Range(0, NeiRefs.Length) ] );
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Editor-only method to bake in references for fire position and neighbors.
        /// </summary>
        /// <param name="explosion"> ExplosionManager controlling fire behavior. </param>
        /// <param name="selfRef"> Polygon ID the fire is located at. </param>
        /// <param name="neiRefs"> List of neighboring polygon IDs. </param>
        public void SetRefs(ExplosionManager explosion, long selfRef, long[] neiRefs)
        {
            _explosion = explosion;
           PolyRef = selfRef;
           NeiRefs = neiRefs;
        }
        #endif
    }
}