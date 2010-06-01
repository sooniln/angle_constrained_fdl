using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Soonil.ForceDirectedLayout
{
    internal class MutableVector
    {
        public double X;
        public double Y;

        #region Constructors

        public MutableVector(Vector v)
            : this(v.X, v.Y)
        { }

        public MutableVector(double x, double y)
        { 
            X = x;
            Y = y;
        }

        #endregion

        #region Mutators

        public MutableVector Add(Vector v)
        {
            X += v.X;
            Y += v.Y;

            return this;
        }

        public MutableVector Subtract(Vector v)
        {
            X -= v.X;
            Y -= v.Y;

            return this;
        }

        public MutableVector Multiply(double s)
        {
            X *= s;
            Y *= s;

            return this;
        }

        public MutableVector Divide(double s)
        {
            X /= s;
            Y /= s;

            return this;
        }

        #endregion

        #region Casts

        public static explicit operator Vector(MutableVector v){
            return Vector.FromRectangular(v.X, v.Y);
        }

        #endregion
    }
}
