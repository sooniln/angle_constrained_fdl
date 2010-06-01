using System;
using System.Diagnostics;
using Soonil.GraphLib;
using System.Threading;

namespace Soonil.ForceDirectedLayout
{
    public class FDLEdge : DefaultEdge
    {
        private Vector ideal;
        private Vector actual;

        private bool dirty;

        #region Constructors

        /// <summary>
        /// Create a new FDLEdge leading from the source to the target.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public FDLEdge(FDLNode source, FDLNode target)
            : this(source, target, Point.Delta(source.Position, target.Position))
        { }

        public FDLEdge(FDLNode source, FDLNode target, Vector ideal)
            : base(source, target)
        {
            this.ideal = ideal;

            InvalidateEdge();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Source node of this edge.
        /// </summary>
        public new FDLNode Source
        {
            get { return base.Source as FDLNode; }
        }

        /// <summary>
        /// Target node of this edge.
        /// </summary>
        public new FDLNode Target
        {
            get { return base.Target as FDLNode; }
        }

        /// <summary>
        /// Actual angle of this edge from source node towards target node.
        /// </summary>
        public double Angle
        {
            get
            {
                if (dirty)
                {
                    UpdateEdge();
                }

                return actual.Angle;
            }
        }

        /// <summary>
        /// Ideal angle of this edge from source node towards target node.
        /// </summary>
        public double AngleIdeal
        {
            get { return ideal.Angle; }
        }

        /// <summary>
        /// Actual deviation from the ideal angle of this edge.
        /// </summary>
        public double AngleDelta
        {
            get
            {
                double angle_delta = AngleIdeal - Angle;
                if (angle_delta > Math.PI)
                    angle_delta -= 2 * Math.PI;
                else if (angle_delta <= -1 * Math.PI)
                    angle_delta += 2 * Math.PI;

                Debug.Assert(angle_delta <= Math.PI && angle_delta > -1 * Math.PI);

                return angle_delta;
            }
        }

        /// <summary>
        /// Actual length of this edge from source node towards target node.
        /// </summary>
        public double Length
        {
            get
            {
                if (dirty)
                {
                    UpdateEdge();
                }

                return actual.Magnitude;
            }
        }

        /// <summary>
        /// Ideal length of this edge from source node towards target node.
        /// </summary>
        public double LengthIdeal
        {
            get { return ideal.Magnitude; }
        }

        /// <summary>
        /// Actual deviation from the ideal length of this edge.
        /// </summary>
        public double LengthDelta
        {
            get
            {
                return LengthIdeal - Length;
            }
        }

        #endregion

        #region Utilities

        internal void InvalidateEdge()
        {
            dirty = true;
        }

        private void UpdateEdge()
        {
            actual = Point.Delta(Source.Position, Target.Position);
            dirty = false;
        }

        #endregion
    }
}
