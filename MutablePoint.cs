using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Soonil.ForceDirectedLayout
{
    internal class MutablePoint
    {
        public double X;
        public double Y;

        #region Constructors

        public MutablePoint(Point v)
            : this(v.X, v.Y)
        { }

        public MutablePoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        #endregion

        #region Mutators

        public MutablePoint Add(Vector v)
        {
            X += v.X;
            Y += v.Y;

            return this;
        }

        public MutablePoint Add(Point p)
        {
            X += p.X;
            Y += p.Y;

            return this;
        }

        public MutablePoint Subtract(Vector v)
        {
            X -= v.X;
            Y -= v.Y;

            return this;
        }

        public MutablePoint Subtract(Point p)
        {
            X -= p.X;
            Y -= p.Y;

            return this;
        }

        public MutablePoint Multiply(double s)
        {
            X *= s;
            Y *= s;

            return this;
        }

        public MutablePoint Divide(double s)
        {
            X /= s;
            Y /= s;

            return this;
        }

        #endregion

        #region Casts

        public static explicit operator Point(MutablePoint v)
        {
            return new Point(v.X, v.Y);
        }

        #endregion
    }
}
