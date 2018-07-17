﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace AutoLineColor.Coloring
{
    internal class ColorSetLoader : IColorSetLoader
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [NotNull]
        public string Name { get; }

        [NotNull] private readonly string _filename;
        [NotNull] private readonly string _defaultContent;

        public ColorSetLoader([NotNull] string name, [NotNull] string filename, [NotNull] string defaultContent)
        {
            Name = name;
            _filename = filename;
            _defaultContent = defaultContent;
        }

        public IColorSet LoadColorSet()
        {
            var logger = Console.Instance;

            // we need to load the color list
            var fullPath = Configuration.GetModFileName(_filename);
            var unparsedColors = _defaultContent;

            try
            {
                if (File.Exists(fullPath))
                {
                    unparsedColors = File.ReadAllText(fullPath);
                }
                else
                {
                    logger.Message("No colors found, writing default values to  " + fullPath);
                    File.WriteAllText(fullPath, unparsedColors);
                }
            }
            catch (Exception ex)
            {
                logger.Error("error reading colors from disk " + ex);
            }

            return ParseColorSet(unparsedColors);
        }

        private static IColorSet ParseColorSet(string unparsedColors)
        {
            // split on new lines, commas and semi-colons
            var colorHexValues = unparsedColors.Split(new[] { "\n", "\r", ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            var colorList = new List<Color32>(colorHexValues.Length);
            foreach (var colorHexValue in colorHexValues)
            {
                if (TryHexToColor(colorHexValue, out var color))
                {
                    colorList.Add(color);
                }
            }
            return new ColorSet(colorList);
        }

        private static bool TryHexToColor(string hex, out Color32 color)
        {
            try
            {
                hex = hex.Replace("0x", ""); //in case the string is formatted 0xFFFFFF
                hex = hex.Replace("#", ""); //in case the string is formatted #FFFFFF
                hex = hex.Trim();

                byte alpha = 255; //assume fully visible unless specified in hex

                var red = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                var green = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                var blue = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

                //Only use alpha if the string has enough characters
                if (hex.Length == 8)
                {
                    alpha = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
                }

                color = new Color32(red, green, blue, alpha);
                return true;
            }
            catch (Exception)
            {
                color = new Color32(0, 0, 0, 255);
                return false;
            }
        }
    }
}
