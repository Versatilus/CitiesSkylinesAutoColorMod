﻿using System.Linq;
using UnityEngine;

namespace AutoLineColor.Coloring
{
    internal static class ColorSelector
    {
        public static IColorSelector DifferenceThreshold { get; } = new DifferenceThresholdSelector();
        public static IColorSelector LeastUsed { get; } = new LeastUsedSelector();

        private class DifferenceThresholdSelector : IColorSelector
        {
            public Color32 SelectColor(in TransportLine transportLine, IColorSet colorSet, IUsedColors usedColors, IColorDistanceMetric metric)
            {
                var colors = colorSet.GetColors();
                var threshold = Configuration.Instance.MinColorDiffPercentage / 100f;

                for (var i = 0; i < Configuration.Instance.MaxDiffColorPickAttempt; i++)
                {
                    var candidate = colors[Random.Range(0, colors.Count - 1)];

                    if (usedColors.MeasureNovelty(candidate, metric) >= threshold)
                    {
                        return candidate;
                    }
                }

                // nothing was above the threshold
                return colors.DefaultIfEmpty(Color.black)
                    .MaxBy(candidate => usedColors.MeasureNovelty(candidate, metric));
            }
        }

        private class LeastUsedSelector : IColorSelector
        {
            public Color32 SelectColor(in TransportLine transportLine, IColorSet colorSet, IUsedColors usedColors,
                IColorDistanceMetric metric)
            {
                var colors = colorSet.GetColors();
                return colors.DefaultIfEmpty(Color.black)
                    .MinBy(usedColors.CountPreviousUses);
            }
        }
    }
}
