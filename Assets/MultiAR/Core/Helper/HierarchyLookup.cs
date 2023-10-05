namespace MultiAR.Core.Helper
{
    using Models;
    using Services.Interfaces;
    using UnityEngine;

    public static class HierarchyLookup
    {
        public static User FindUserInHierarchy(Component root)
        {
            var userObject = root.transform.parent.GetComponentInParent<IUserObject>();
            return userObject?.User;
        }
    }
}
