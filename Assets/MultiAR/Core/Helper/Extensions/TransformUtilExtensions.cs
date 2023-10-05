namespace MultiAR.Core.Helper.Extensions
{
    using UnityEngine;

    public static class TransformUtilExtensions
    {
        public static void DestroyAllChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                Object.Destroy(child.gameObject);
            }
        }
     }
}
