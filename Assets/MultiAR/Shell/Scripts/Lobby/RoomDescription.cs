
namespace MultiAR.Shell.Scripts.Lobby
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "ExampleRoom", menuName = "MultiAR/RoomDescription", order = 1)]
    public class RoomDescription : ScriptableObject
    {
        /// <summary>
        /// A unique identifier for the room.
        /// </summary>
        public string typeId;

        /// <summary>
        /// A title for the room. This shall not be used as identifier.
        /// </summary>
        public string title;

        /// <summary>
        /// A description for the room containing more specific information what the room does.
        /// </summary>
        public string description;

        /// <summary>
        /// The name of the scene that shall be loaded when entering the room.
        /// </summary>
        public string sceneName;

        /// <summary>
        /// A model object that is used when displaying the room.
        /// </summary>
        public GameObject model;

        /// <summary>
        /// A prefab object that is used when placing the room.
        /// </summary>
        public GameObject placerPrefab;

        /// <summary>
        /// Hint that the room shall be destroyed when empty.
        /// </summary>
        public bool destroyRoomWhenEmpty;
    }
}
