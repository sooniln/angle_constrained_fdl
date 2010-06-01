using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soonil.ForceDirectedLayout
{
    public class Point : IEquatable<Point>
    {
        /// <summary>
        /// The point representing the origin of the coordinate system.
        /// </summary>
        public static readonly Point ZERO_POINT = new Point(0.0, 0.0);

        public readonly double X;
        public readonly double Y;

        #region Constructors

        /// <summary>
        /// Creates a new point at the specified x and y coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Returns a vector pointing from one point to another point.
        /// </summary>
        /// <param name="from">The point to direct the vector from.</param>
        /// <param name="to">The point to direct the vector to.</param>
        /// <returns>The vector leading from one point to another point.</returns>
        public static Vector Delta(Point from, Point to)
        {
            return Vector.FromRectangular(to.X - from.X, to.Y - from.Y);
        }

        #endregion

        #region Operators

        public static Point operator +(Point p1, Point p2)
        {
            if (p1 == ZERO_POINT)
                return p2;
            if (p2 == ZERO_POINT)
                return p1;

            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static Point operator +(Point p, Vector v)
        {
            if (v == Vector.ZERO_VECTOR)
                return p;

            return new Point(p.X + v.X, p.Y + v.Y);
        }

        public static Point operator +(Vector v, Point p)
        {
            return p + v;
        }

        public static Point operator -(Point p, Vector v)
        {
            if (v == Vector.ZERO_VECTOR)
                return p;

            return new Point(p.X - v.X, p.Y - v.Y);
        }

        public static Point operator -(Point p1, Point p2)
        {
            if (p2 == ZERO_POINT)
                return p1;

            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static Point operator *(Point p, double s)
        {
            if (s == 0)
                return ZERO_POINT;

            return new Point(p.X * s, p.Y * s);
        }

        public static Point operator *(double s, Point p)
        {
            return p * s;
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

            return Equals((Point)obj);
        }

        public bool Equals(Point p)
        {
            if (ReferenceEquals(this, p))
                return true;
            if (ReferenceEquals(null, p))
                return false;

            return X == p.X && Y == p.Y;
        }

        public static bool operator ==(Point p1, Point p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        #endregion
    }
}
