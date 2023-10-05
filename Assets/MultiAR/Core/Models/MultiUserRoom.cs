using System;
using UnityEngine;
using ExitGames.Client.Photon;
using MultiAR.Core.Services.Implementations;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MultiAR.Core.Models
{
    public class MultiUserRoom
    {
        public string Name { get; set; }

        public int UserCount { get; set; }

        public byte? UserLimit { get; set; } = null;

        public string AnchorId { get; set; } = null;

        public Vector3? Boundaries { get; set; }

        public DateTime CreationDate { get; set; }

        public Color? Color { get; set; } = null;

        public string TypeId { get; set; }


        public static Hashtable SerializeRoomProperties(MultiUserRoom room)
        {
            var properties = new Hashtable
            {
                [RoomPropertyNames.CreationDate] = room.CreationDate.ToString("o", CultureInfo.InvariantCulture)
            };

            if (!string.IsNullOrEmpty(room.AnchorId))
            {
                properties[RoomPropertyNames.AnchorId] = room.AnchorId;
            }

            if (!string.IsNullOrEmpty(room.TypeId))
            {
                properties[RoomPropertyNames.TypeId] = room.TypeId;
            }

            if (room.Color != null)
            {
                // Serialize in format R/G/B/A
                var color = room.Color.Value;
                properties[RoomPropertyNames.Color] = $"{color.r}/{color.g}/{color.b}/{color.a}";
            }

            if (room.Boundaries != null)
            {
                properties[RoomPropertyNames.Boundaries] =
                    $"{room.Boundaries?.x}:{room.Boundaries?.y}:{room.Boundaries?.z}";
            }

            return properties;
        }


        public static void DeserializeRoomProperties(IReadOnlyDictionary<object, object> properties,
            MultiUserRoom room)
        {
            if (properties.TryGetValue(RoomPropertyNames.CreationDate, out var creationDateString))
            {
                room.CreationDate = DateTime.Parse((string)creationDateString);
            }

            if (properties.TryGetValue(RoomPropertyNames.AnchorId, out var anchorId))
            {
                room.AnchorId = (string)anchorId;
            }

            if (properties.TryGetValue(RoomPropertyNames.TypeId, out var typeId))
            {
                room.TypeId = (string)typeId;
            }

            if (properties.TryGetValue(RoomPropertyNames.Color, out var colorString))
            {
                // Serialized in format R/G/B/A
                var color = ((string)colorString).Split('/').Select(float.Parse).ToArray();
                if (color.Length == 4)
                {
                    room.Color = new Color(color[0], color[1], color[2], color[3]);
                }
            }

            if (properties.TryGetValue(RoomPropertyNames.Boundaries, out var serializedBoundaries))
            {
                var boundaries = ((string)serializedBoundaries).Split(':').Select(float.Parse).ToArray();
                room.Boundaries = new Vector3(boundaries[0], boundaries[1], boundaries[2]);
            }
        }
    }
}
