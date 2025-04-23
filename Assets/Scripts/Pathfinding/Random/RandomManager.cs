using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomManager : MonoBehaviour
{
    [field:SerializeField] public int BaseSeed { get; private set; } = 12345;

    [SerializeReference] private List<ISeedRandom> _streams;

    public static RandomManager Instance { get; private set; }

    private void Awake()
    {
        _streams = new List<ISeedRandom>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterStream(ISeedRandom stream)
    {
        if (!_streams.Contains(stream))
        {
            // int ID = stream.Owner.transform.hierarchyCapacity ^ stream.Owner.transform.GetSiblingIndex();
            int ID = stream.Owner.transform.hierarchyCapacity;

            Transform t = stream.Owner.transform;
            string path = t.name;

            while ( t.parent != null )
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            foreach (char c in path)
                ID = ID * 31 + c;

            stream.ID = ID;

            stream.Random = new System.Random(BaseSeed ^ stream.ID);
            _streams.Add(stream);
        }

        Debug.Log("Registered new stream: " + stream);
    }
}