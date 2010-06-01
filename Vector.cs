using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Soonil.ForceDirectedLayout
{
    public class Vector : IEquatable<Vector>
    {
        /// <summary>
        /// A vector of no length or direction.
        /// </summary>
        public static readonly Vector ZERO_VECTOR = new Vector(0.0, 0.0, 0.0, 0.0);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly double x;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly double y;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private double magnitude;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private double angle;

        #region Constructors

        private Vector(double x, double y)
        { 
            this.x = x;
            this.y = y;
            magnitude = Double.NaN;
            angle = Double.NaN;
        }

        private Vector(double x, double y, double magnitude, double angle)
        {
            // adjust magnitude to always be positive
            if (magnitude < 0)
            {
                if (angle < Math.PI / 2.0)
                    angle += Math.PI / 2.0;
                else
                    angle -= Math.PI / 2.0;
                
                magnitude *= -1;
            }

            this.x = x;
            this.y = y;
            this.magnitude = magnitude;

            if (angle >= 0.0 && angle < Math.PI * 2.0)
                this.angle = angle;
        }

        /// <summary>
        /// Creates a new vector from rectangular coordinates.
        /// </summary>
        /// <param name="x">X dimension of the vector.</param>
        /// <param name="y">Y dimension of the vector.</param>
        /// <returns>A new vector of the specified dimensions.</returns>
        public static Vector FromRectangular(double x, double y)
        {
            if (x == 0.0 && y == 0.0)
                return ZERO_VECTOR;

            return new Vector(x, y);
        }

        /// <summary>
        /// Creates a new vector from polar coordinates.
        /// </summary>
        /// <param name="magnitude">The length of the vector.</param>
        /// <param name="angle">The angle of the vector.</param>
        /// <returns>A new vector of the specified dimensions.</returns>
        public static Vector FromPolar(double magnitude, double angle)
        {
            if (magnitude == 0.0)
                return ZERO_VECTOR;

            double x = Math.Cos(angle) * magnitude;
            double y = Math.Sin(angle) * magnitude;

            if (angle < 0.0 || angle >= Math.PI * 2.0)
                return new Vector(x, y);
            else
                return new Vector(x, y, magnitude, angle);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The X dimension of the vector.
        /// </summary>
        public double X { get { return x; } }

        /// <summary>
        /// The Y dimension of the vector.
        /// </summary>
        public double Y { get { return y; } }

        /// <summary>
        /// The length of the vector.
        /// </summary>
        public double Magnitude
        {
            get
            {
                if (Double.IsNaN(magnitude))
                {
                    magnitude = Math.Sqrt(x * x + y * y);
                }

                Debug.Assert(magnitude >= 0);
                return magnitude;
            }
        }

        /// <summary>
        /// The angle of the vector.
        /// </summary>
        public double Angle
        {
            get
            {
                if (Double.IsNaN(angle))
                {
                    angle = CalcAngle(x, y);
                }

                
                return angle;
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Calculate the angle in radians of a line from the origin to the point (x, y).
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>The angle in radians between the line and the x axis.</returns>
        private static double CalcAngle(double x, double y)
        {
            double angle = Math.Atan2(y, x);
            if (angle < 0.0)
                angle += 2.0 * Math.PI;
            if (angle == 2.0 * Math.PI) // necessary due to rounding error in floating point
                angle = 0.0;

            Debug.Assert(angle >= 0.0 && angle < 2.0 * Math.PI);

            return angle;
        }

        /// <summary>
        /// Calculates the angle in radians of a line between to points.
        /// </summary>
        /// <param name="from">The first point.</param>
        /// <param name="to">The second point.</param>
        /// <returns>The angle in radians between the line and the x axis.</returns>
        public static double CalcAngle(Point from, Point to)
        {
            return CalcAngle(to.X - from.X, to.Y - from.Y);
        }

        #endregion

        #region Operators

        public static Vector operator +(Vector v1, Vector v2)
        {
            if (v1 == ZERO_VECTOR)
                return v2;
            if (v2 == ZERO_VECTOR)
                return v1;

            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            if (v1 == ZERO_VECTOR)
                return v2;
            if (v2 == ZERO_VECTOR)
                return v1;

            return new Vector(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector operator -(Vector v)
        {
            if (v == ZERO_VECTOR)
                return ZERO_VECTOR;

            return -1 * v;
        }

        public static Vector operator *(double s, Vector v)
        {
            if (s == 0.0 || v == ZERO_VECTOR)
                return ZERO_VECTOR;

            if(Double.IsNaN(v.magnitude) || Double.IsNaN(v.angle))
                return new Vector(v.X * s, v.Y * s);

            return new Vector(v.X * s, v.Y * s, v.Magnitude * s, v.Angle);
        }

        public static Vector operator *(Vector v, double s)
        {
            return s * v;
        }

        public static Vector operator /(Vector v, double s)
        {
            return (1.0 / s) * v;
        }

        #endregion

        #region Value Type Overrides

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (GetType() != obj.GetType())
                return false;

            return Equals((Vector)obj);
        }

        public bool Equals(Vector v)
        {
            if (ReferenceEquals(v, null))
                return false;
            if (ReferenceEquals(this, v))
                return true;

            return X == v.X && Y == v.Y;
        }

        public static bool operator ==(Vector v1, Vector v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(Vector v1, Vector v2)
        {
            return !(v1 == v2);
        }

        #endregion
    }
}
