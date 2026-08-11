using System;
using System.Collections.Generic;
using AegisRTS.Gameplay.Units;

namespace AegisRTS.Gameplay.Formation
{
    public enum FormationType
    {
        Line,
        Box,
    }

    public readonly struct FormationSlot
    {
        public FormationSlot(int index, WorldPoint destination)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            Index = index;
            Destination = destination;
        }

        public int Index { get; }
        public WorldPoint Destination { get; }
    }

    /// <summary>Creates deterministic, non-overlapping group destinations without Unity types.</summary>
    public static class FormationPlanner
    {
        public static IReadOnlyList<FormationSlot> Plan(
            WorldPoint center,
            int unitCount,
            FormationType formation,
            double spacing,
            double forwardX,
            double forwardZ)
        {
            if (unitCount < 0) throw new ArgumentOutOfRangeException(nameof(unitCount));
            if (spacing <= 0d || double.IsNaN(spacing) || double.IsInfinity(spacing))
                throw new ArgumentOutOfRangeException(nameof(spacing));
            if (unitCount == 0) return Array.Empty<FormationSlot>();

            NormalizeHeading(ref forwardX, ref forwardZ);
            double rightX = forwardZ;
            double rightZ = -forwardX;
            var slots = new List<FormationSlot>(unitCount);

            int columns = formation == FormationType.Line
                ? unitCount
                : (int)Math.Ceiling(Math.Sqrt(unitCount));
            int rows = (int)Math.Ceiling((double)unitCount / columns);

            for (int index = 0; index < unitCount; index++)
            {
                int row = index / columns;
                int column = index % columns;
                int unitsInRow = Math.Min(columns, unitCount - row * columns);
                double localX = (column - (unitsInRow - 1) * 0.5d) * spacing;
                double localZ = ((rows - 1) * 0.5d - row) * spacing;
                double worldX = center.X + rightX * localX + forwardX * localZ;
                double worldZ = center.Z + rightZ * localX + forwardZ * localZ;
                slots.Add(new FormationSlot(index, new WorldPoint(worldX, center.Y, worldZ)));
            }

            return slots.AsReadOnly();
        }

        private static void NormalizeHeading(ref double x, ref double z)
        {
            if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(z) || double.IsInfinity(z))
                throw new ArgumentOutOfRangeException(nameof(x), "Formation heading must be finite.");
            double length = Math.Sqrt(x * x + z * z);
            if (length < 0.000001d)
            {
                x = 0d;
                z = 1d;
                return;
            }

            x /= length;
            z /= length;
        }
    }
}
