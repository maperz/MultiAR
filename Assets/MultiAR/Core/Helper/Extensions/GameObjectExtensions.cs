namespace MultiAR.Core.Helper.Extensions
{
    using UnityEngine;

    public static class GameObjectExtensions
    {
        public static bool DestroyComponent<T>(this GameObject gameObject)
        {
            Component component = gameObject.GetComponent(typeof(T));
            if (component != null)
            {
                Object.DestroyImmediate(component);
                return true;
            }

            return false;
        }
    }
}
