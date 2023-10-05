using UnityEngine;

namespace MultiAR.Core.Helper
{
    public static class ColorHelper
    {
        private static readonly string[] ColorCodes =
        {
            "#1F44E7", // Dark Blue
            "#C600ED", // Solid Purple
            "#17F106", // Toxic Green
            "#F20C1A", // Solid Red
            "#FFF000", // Vibrant Yellow
            "#FF7FB0", // Friendly Rose
            "#FFA800", // Solid Orange
            "#840091", // Darker Purple
            "#49A617", // Wood Green
            "#6AC3FF", // Sky Blue
            "#82502C", // Solid Brown
            "#26DA85", // Tealish Green
            "#BA0E0E", // Wine Red
            "#BA0E0E", // Blood Red
            "#FCD358", // Sandy Orange
            "#1A9636", // Basic Green
            "#01F6DE", // Bright Blue
        };

        private static Color[] _colorCache = null;

        private static Color[] Colors
        {
            get
            {
                if (_colorCache == null)
                {
                    _colorCache = new Color[ColorCodes.Length];
                    for (int i = 0; i < ColorCodes.Length; ++i)
                    {
                        _colorCache[i] = FromHex(ColorCodes[i]);
                    }
                }

                return _colorCache;
            }
        }

        public static Color GetColorForId(int id)
        {
            return Colors[(id - 1) % Colors.Length];
        }

        public static Color RandomFlatColor()
        {
            return Colors[Random.Range(0, Colors.Length)];
        }

        private static Color FromHex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var colorResult))
            {
                return colorResult;
            }

            Debug.LogWarning("Failed to parse hex color: " + hex);
            return Color.magenta;
        }
    }
}
