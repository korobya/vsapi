﻿using System.IO;
using System.Text;

namespace Vintagestory.API.Datastructures
{
    public class DoubleArrayAttribute : ArrayAttribute<double>, IAttribute
    {
        public DoubleArrayAttribute()
        {

        }

        public DoubleArrayAttribute(double[] value)
        {
            this.value = value;
        }

        public void ToBytes(BinaryWriter stream)
        {
            stream.Write(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                stream.Write(value[i]);
            }

        }

        public void FromBytes(BinaryReader stream)
        {
            int quantity = stream.ReadInt32();
            value = new double[quantity];
            for (int i = 0; i < quantity; i++)
            {
                value[i] = stream.ReadDouble();
            }

        }

        public int GetAttributeId()
        {
            return 13;
        }
        

    }
}