using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField] private float _duration;

    [field:SerializeField] public long PolyRef { get; private set; }
    [field:SerializeField] public long[] NeiRefs { get; private set; }

    private float _lifeTime;

    private void OnEnable()
    {
        _lifeTime = 0f;
    }

    private void Update()
    {
        _lifeTime += Time.deltaTime;
        if ( _lifeTime >= _duration )
            gameObject.SetActive(false);

        if ( ExplosionManager.Rand.Range(0f, 1f) < 0.005f )
            ExplosionManager.SetFire( NeiRefs[ Random.Range(0, NeiRefs.Length) ] );
    }

    #if UNITY_EDITOR
    public void SetRefs(long selfRef, long[] neiRefs)
    {
        PolyRef = selfRef;
        NeiRefs = neiRefs;
    }
    #endif
}
