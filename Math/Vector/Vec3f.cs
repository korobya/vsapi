﻿using System;
using System.IO;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.MathTools
{
    /// <summary>
    /// Represents a vector of 3 floats. Go bug Tyron of you need more utility methods in this class.
    /// </summary>
    public class Vec3f : IVec3
    {
        public static Vec3f Zero { get { return new Vec3f(); } }

        /// <summary>
        /// The X-Component of the vector
        /// </summary>
        public float X;
        /// <summary>
        /// The Y-Component of the vector
        /// </summary>
        public float Y;
        /// <summary>
        /// The Z-Component of the vector
        /// </summary>
        public float Z;


        /// <summary>
        /// Synonum for X
        /// </summary>
        public float R { get { return X; } set { X = value; } }
        /// <summary>
        /// Synonum for Y
        /// </summary>
        public float G { get { return Y; } set { Y = value; } }
        /// <summary>
        /// Synonum for Z
        /// </summary>
        public float B { get { return Z; } set { Z = value; } }

        /// <summary>
        /// Creates a new vector with x/y/z = 0
        /// </summary>
        public Vec3f()
        {

        }

        /// <summary>
        /// Create a new vector with given coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vec3f(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Create a new vector with given coordinates
        /// </summary>
        /// <param name="vec"></param>
        public Vec3f(Vec4f vec)
        {
            this.X = vec.X;
            this.Y = vec.Y;
            this.Z = vec.Z;
        }

        /// <summary>
        /// Create a new vector with given coordinates
        /// </summary>
        /// <param name="values"></param>
        public Vec3f(float[] values)
        {
            this.X = values[0];
            this.Y = values[1];
            this.Z = values[2];
        }

        /// <summary>
        /// Returns the n-th coordinate
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public float this[int index]
        {
            get { return index == 0 ? X : (index == 1 ? Y : Z); }
            set { if (index == 0) X = value; else if (index == 1) Y = value; else Z = value; }
        }


        /// <summary>
        /// Returns the length of this vector
        /// </summary>
        /// <returns></returns>
        public float Length()
        {
            return GameMath.FastSqrt(X * X + Y * Y + Z * Z);
        }

        public void Negate()
        {
            this.X = -X;
            this.Y = -Y;
            this.Z = -Z;
        }



        /// <summary>
        /// Returns the dot product with given vector
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public float Dot(Vec3f a)
        {
            return X * a.X + Y * a.Y + Z * a.Z;
        }

        /// <summary>
        /// Returns the dot product with given vector
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public float Dot(Vec3d a)
        {
            return (float)(X * a.X + Y * a.Y + Z * a.Z);
        }


        internal double Dot(float[] pos)
        {
            return X * pos[0] + Y * pos[1] + Z * pos[2];
        }

        internal double Dot(double[] pos)
        {
            return (float)(X * pos[0] + Y * pos[1] + Z * pos[2]);
        }

        public Vec3f Cross(Vec3f vec)
        {
            Vec3f outv = new Vec3f();
            outv.X = Y * vec.Z - Z * vec.Y;
            outv.Y = Z * vec.X - X * vec.Z;
            outv.Z = X * vec.Y - Y * vec.X;
            return outv;
        }

        public double[] ToDoubleArray()
        {
            return new double[] { X, Y, Z };
        }

        public void Cross(Vec3f a, Vec3f b)
        {
            X = a.Y * b.Z - a.Z * b.Y;
            Y = a.Z * b.X - a.X * b.Z;
            Z = a.X * b.Y - a.Y * b.X;            
        }

        public void Cross(Vec3f a, Vec4f b)
        {
            X = a.Y * b.Z - a.Z * b.Y;
            Y = a.Z * b.X - a.X * b.Z;
            Z = a.X * b.Y - a.Y * b.X;
        }

        /// <summary>
        /// Adds given x/y/z coordinates to the vector
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public Vec3f Add(float x, float y, float z)
        {
            this.X += x;
            this.Y += y;
            this.Z += z;
            return this;
        }

        /// <summary>
        /// Multiplies each coordinate with given multiplier
        /// </summary>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public Vec3f Mul(float multiplier)
        {
            this.X *= multiplier;
            this.Y *= multiplier;
            this.Z *= multiplier;
            return this;
        }

        /// <summary>
        /// Creates a copy of the vetor
        /// </summary>
        /// <returns></returns>
        public Vec3f Clone()
        {
            return (Vec3f)MemberwiseClone();
        }

        /// <summary>
        /// Turns the vector into a unit vector with length 1, but only if length is non-zero
        /// </summary>
        /// <returns></returns>
        public Vec3f Normalize()
        {
            float length = Length();

            if (length > 0)
            {
                X /= length;
                Y /= length;
                Z /= length;
            }


            return this;
        }

        /// <summary>
        /// Calculates the distance the two endpoints
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public float Distance(Vec3f vec)
        {
            return GameMath.FastSqrt(
                (X - vec.X) * (X - vec.X) +
                (Y - vec.X) * (Y - vec.Y) +
                (Z - vec.X) * (Z - vec.Z)
            );
        }

        /// <summary>
        /// Adds given coordinates to a new vectors and returns it. The original calling vector remains unchanged
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public Vec3f AddCopy(float x, float y, float z)
        {
            return new Vec3f(X + x, Y + y, Z + z);
        }

        /// <summary>
        /// Adds both vectors into a new vector. Both source vectors remain unchanged.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public Vec3f AddCopy(Vec3f vec)
        {
            return new Vec3f(X + vec.X, Y + vec.Y, Z + vec.Z);
        }


        /// <summary>
        /// Substracts val from each coordinate if the coordinate if positive, otherwise it is added. If 0, the value is unchanged. The value must be a positive number
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public void ReduceBy(float val)
        {
            X = X > 0 ? Math.Max(0, X - val) : Math.Min(0, X + val);
            Y = Y > 0 ? Math.Max(0, Y - val) : Math.Min(0, Y + val);
            Z = Z > 0 ? Math.Max(0, Z - val) : Math.Min(0, Z + val);
        }

        /// <summary>
        /// Creates a new vectors that is the normalized version of this vector. 
        /// </summary>
        /// <returns></returns>
        public Vec3f NormalizedCopy()
        {
            float length = Length();
            return new Vec3f(
                  X / length,
                  Y / length,
                  Z / length
            );
        }

        /// <summary>
        /// Creates a new double precision vector with the same coordinates
        /// </summary>
        /// <returns></returns>
        public Vec3d ToVec3d()
        {
            return new Vec3d(X, Y, Z);
        }

        #region Operators
        public static Vec3f operator -(Vec3f left, Vec3f right)
        {
            return new Vec3f(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public static Vec3f operator +(Vec3f left, Vec3f right)
        {
            return new Vec3f(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }


        public static Vec3f operator -(Vec3f left, float right)
        {
            return new Vec3f(left.X - right, left.Y - right, left.Z - right);
        }


        public static Vec3f operator -(float left, Vec3f right)
        {
            return new Vec3f(left - right.X, left - right.Y, left - right.Z);
        }

        public static Vec3f operator +(Vec3f left, float right)
        {
            return new Vec3f(left.X + right, left.Y + right, left.Z + right);
        }


        public static Vec3f operator *(Vec3f left, float right)
        {
            return new Vec3f(left.X * right, left.Y * right, left.Z * right);
        }

        public static Vec3f operator *(float left, Vec3f right)
        {
            return new Vec3f(left * right.X, left * right.Y, left * right.Z);
        }

        public static float operator *(Vec3f left, Vec3f right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }

        public static Vec3f operator /(Vec3f left, float right)
        {
            return new Vec3f(left.X / right, left.Y / right, left.Z / right);
        }


        #endregion

        /// <summary>
        /// Sets the vector to this coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vec3f Set(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            return this;
        }

        /// <summary>
        /// Sets the vector to the coordinates of given vector
        /// </summary>
        /// <param name="vec"></param>
        public Vec3f Set(Vec3d vec)
        {
            this.X = (float)vec.X;
            this.Y = (float)vec.Y;
            this.Z = (float)vec.Z;
            return this;
        }

        public Vec3f Set(float[] vec)
        {
            this.X = vec[0];
            this.Y = vec[1];
            this.Z = vec[2];
            return this;
        }

        /// <summary>
        /// Sets the vector to the coordinates of given vector
        /// </summary>
        /// <param name="vec"></param>
        public void Set(Vec3f vec)
        {
            this.X = (float)vec.X;
            this.Y = (float)vec.Y;
            this.Z = (float)vec.Z;
        }

        /// <summary>
        /// Simple string represenation of the x/y/z components
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "x=" + X + ", y=" + Y + ", z=" + Z;
        }


        public void Write(BinaryWriter writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }

        public static Vec3f CreateFromBytes(BinaryReader reader)
        {
            return new Vec3f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        int IVec3.XAsInt { get { return (int)X; } }

        int IVec3.YAsInt { get { return (int)Y; } }

        int IVec3.ZAsInt { get { return (int)Z; } }

        double IVec3.XAsDouble { get { return X; } }

        double IVec3.YAsDouble { get { return Y; } }

        double IVec3.ZAsDouble { get { return Z; } }

        float IVec3.XAsFloat { get { return X; } }

        float IVec3.YAsFloat { get { return Y; } }

        float IVec3.ZAsFloat { get { return Z; } }
    }
}