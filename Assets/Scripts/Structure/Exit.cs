using UnityEngine;

public class Exit : Structure<Stage>
{
    [field:SerializeField] private Transform[] _exits;
    protected override void SetUpPoints()
    {
        throw new System.NotImplementedException();
    }
}
