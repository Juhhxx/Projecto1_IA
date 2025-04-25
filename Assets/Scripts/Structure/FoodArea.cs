using UnityEngine;

public class FoodTable : Structure<Stage>
{
    [field:SerializeField] private Transform[] _tables;
    protected override void SetUpPoints()
    {
        throw new System.NotImplementedException();
    }
}
