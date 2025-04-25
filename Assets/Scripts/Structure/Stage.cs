using UnityEngine;

public class Stage : Structure<Stage>
{
    [field:SerializeField] private Renderer _area;
    protected override void SetUpPoints()
    {
        throw new System.NotImplementedException();
    }
}
