using UnityEngine;

namespace Scripts
{
    /// <summary>
    /// Manager abstract class defines methods that will update in a set order for Manager subclasses.
    /// </summary>
    public abstract class Manager : MonoBehaviour
    {
        internal protected abstract void AwakeOrdered();
        internal protected abstract void StartOrdered();
        internal protected abstract void Bake();
        internal protected abstract void UpdateOrdered();
    }
}