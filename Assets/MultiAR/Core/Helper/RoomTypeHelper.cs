namespace MultiAR.Core.Helper
{
    using System;

    public enum RoomType
    {
        Cubes,
        Medical,
        Anatomy
    }

    public static class RoomTypeHelper
    {
        public static string GetSceneNameForType(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.Cubes: return "CubesRoom";
                case RoomType.Medical: return "MedicalRoom";
                case RoomType.Anatomy: return "AnatomyRoom";
            }

            throw new NotImplementedException();
        }
    }
}
