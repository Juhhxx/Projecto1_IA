using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField] private float _duration;
    private float _lifeTime;
    public long PolyRef;

    [field:SerializeField] public long[] neiRefs { get; private set; }

    private void OnEnable()
    {
        _lifeTime = 0f;
    }

    private void Update()
    {
        _lifeTime += Time.deltaTime;
        if ( _lifeTime >= _duration )
            gameObject.SetActive(false);

        if ( Random.Range(0f, 1f) < 0.005f )
            ExplosionManager.SetFire( neiRefs[ Random.Range(0, neiRefs.Length-1) ] );
    }

    public void SetNeighborReferences(long[] refs)
    {
        neiRefs = refs;
    }
}
