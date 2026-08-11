using System;

namespace AegisRTS.Presentation.Camera
{
    /// <summary>Unity-independent RTS camera pivot, zoom, and bounds state.</summary>
    public sealed class RtsCameraRigModel
    {
        public RtsCameraRigModel(
            double pivotX = 0d, double pivotZ = 0d, double zoom = 22d,
            double minimumX = -45d, double maximumX = 45d,
            double minimumZ = -45d, double maximumZ = 45d,
            double minimumZoom = 8d, double maximumZoom = 40d)
        {
            if (minimumX > maximumX || minimumZ > maximumZ || minimumZoom <= 0d || minimumZoom > maximumZoom)
                throw new ArgumentException("Camera bounds or zoom limits are invalid.");
            MinimumX = minimumX;
            MaximumX = maximumX;
            MinimumZ = minimumZ;
            MaximumZ = maximumZ;
            MinimumZoom = minimumZoom;
            MaximumZoom = maximumZoom;
            Focus(pivotX, pivotZ);
            SetZoom(zoom);
        }

        public double PivotX { get; private set; }
        public double PivotZ { get; private set; }
        public double Zoom { get; private set; }
        public double MinimumX { get; }
        public double MaximumX { get; }
        public double MinimumZ { get; }
        public double MaximumZ { get; }
        public double MinimumZoom { get; }
        public double MaximumZoom { get; }

        public void Pan(double worldX, double worldZ)
        {
            RequireFinite(worldX, nameof(worldX));
            RequireFinite(worldZ, nameof(worldZ));
            PivotX = Clamp(PivotX + worldX, MinimumX, MaximumX);
            PivotZ = Clamp(PivotZ + worldZ, MinimumZ, MaximumZ);
        }

        public void Focus(double worldX, double worldZ)
        {
            RequireFinite(worldX, nameof(worldX));
            RequireFinite(worldZ, nameof(worldZ));
            PivotX = Clamp(worldX, MinimumX, MaximumX);
            PivotZ = Clamp(worldZ, MinimumZ, MaximumZ);
        }

        public void ZoomBy(double delta) { RequireFinite(delta, nameof(delta)); SetZoom(Zoom + delta); }
        public string GetDebugSummary() => $"Pivot=({PivotX:0.##}, {PivotZ:0.##}), Zoom={Zoom:0.##}";

        private void SetZoom(double value)
        {
            RequireFinite(value, nameof(value));
            Zoom = Clamp(value, MinimumZoom, MaximumZoom);
        }

        private static double Clamp(double value, double minimum, double maximum) => Math.Max(minimum, Math.Min(maximum, value));
        private static void RequireFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
        }
    }
}
