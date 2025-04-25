using UnityEngine;

public abstract class Manager : MonoBehaviour
{
    internal protected abstract void AwakeOrdered();
    internal protected abstract void StartOrdered();
    internal protected abstract void Bake();
    internal protected abstract void UpdateOrdered();
}
