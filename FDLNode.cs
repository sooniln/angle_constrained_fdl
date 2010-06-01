using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Soonil.GraphLib;
using System.Diagnostics;
using System.Threading;

namespace Soonil.ForceDirectedLayout
{
    public class FDLNode : DefaultNode
    {
        public const double DEFAULT_MASS = 1.0;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly double mass;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Point position;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Point prev_position;

        #region Constructors

        /// <summary>
        /// Creates a new node with the default mass and zeroed position and velocity. 
        /// </summary>
        public FDLNode() : this(DEFAULT_MASS) { }

        /// <summary>
        /// Create a new node with the specified mass and zeroed position and velocity.
        /// </summary>
        /// <param name="mass">The mass of the node.</param>
        public FDLNode(double mass)
        {
            this.mass = mass;
            this.position = Point.ZERO_POINT;
            this.prev_position = Point.ZERO_POINT;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The mass of this node.
        /// </summary>
        public double Mass
        {
            get { return mass; }
        }

        /// <summary>
        /// The absolute position of this node.
        /// </summary>
        public Point Position
        {
            get { return position; }
            set
            {
                PreviousPosition = position;

                if (value == position)
                    return;

                position = value;

                foreach (FDLEdge edge in OutEdges)
                    edge.InvalidateEdge();
                foreach (FDLEdge edge in InEdges)
                    edge.InvalidateEdge();
            }
        }

        /// <summary>
        /// The position of this node before the current position.
        /// Updated whenever Position is set.
        /// </summary>
        public Point PreviousPosition
        {
            get { return prev_position; }
            private set { prev_position = value; }
        }

        #endregion
    }
}
