﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vintagestory.API.Client
{
    /// <summary>
    /// Holds arbitrary mesh data for meshes to be used in a shader
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CustomMeshDataPart<T>
    {
        bool customAllocationSize = false;
        int allocationSize;

        /// <summary>
        /// The arbitrary data to be uploaded to the graphics card
        /// </summary>
        public T[] Values;

        /// <summary>
        /// Amout of values currently added
        /// </summary>
        public int Count;

        /// <summary>
        /// Size of the Values array
        /// </summary>
        public int BufferSize { get { return Values == null ? 0 : Values.Length; } }

        /// <summary>
        /// Size of the array that should be allocated on the graphics card.
        /// </summary>
        public int AllocationSize
        {
            get { return customAllocationSize ? allocationSize : Count; }
        }

        /// <summary>
        /// Amount of variable components for variable (i.e. 2, 3 for a vec2 and a vec3), valid values are 1, 2, 3 and 4 (limited by glVertexAttribPointer)
        /// </summary>
        public int[] InterleaveSizes = null;
        /// <summary>
        /// Stride - Size in bytes of all values for one vertex
        /// </summary>
        public int InterleaveStride = 0;
        /// <summary>
        /// Offset in bytes for each in variable 
        /// </summary>
        public int[] InterleaveOffsets = null;
        /// <summary>
        /// For instanced rendering
        /// </summary>
        public bool Instanced = false;
        /// <summary>
        /// Set to false if you intend to update the buffer very often (i.e. every frame)
        /// </summary>
        public bool StaticDraw = true;
        /// <summary>
        /// Used as offset when doing a partial update on an existing buffer
        /// </summary>
        public int BaseOffset = 0;


        public CustomMeshDataPart()
        {
        }

        public CustomMeshDataPart(int arraySize) {
            Values = new T[arraySize];
        }


        public void GrowBuffer(int growAtLeastBy = 1)
        {
            if (Values == null)
            {
                Values = new T[Math.Max(growAtLeastBy, Count * 2)];
                return;
            }
            Array.Resize(ref Values, Math.Max(Values.Length + growAtLeastBy, Count * 2));
        }

        public void Add(T value)
        {
            if (Count >= BufferSize)
            {
                GrowBuffer();
            }
            Values[Count++] = value;
        }

        public void Add(T value1, T value2)
        {
            if (Count + 1 >= BufferSize)
            {
                GrowBuffer(2);
            }

            Values[Count++] = value1;
            Values[Count++] = value2;
        }

        public void Add(T value1, T value2, T value3, T value4)
        {
            if (Count + 3 >= BufferSize)
            {
                GrowBuffer(4);
            }

            Values[Count++] = value1;
            Values[Count++] = value2;
            Values[Count++] = value3;
            Values[Count++] = value4;
        }

        public void Add(T[] values)
        {
            if (Count + values.Length > BufferSize)
            {
                GrowBuffer(values.Length);
            }

            for (int i = 0; i < values.Length; i++)
            {
                Values[Count++] = values[i];
            }
        }



        /// <summary>
        /// Lets you define your a self defined size to be allocated on the graphics card.
        /// If not called the allocation size will be the count of the Elements in the Array
        /// </summary>
        public void SetAllocationSize(int size)
        {
            customAllocationSize = true;
            allocationSize = size;
        }

        /// <summary>
        /// Use the element count as the allocation size (default behavior)
        /// </summary>
        public void AutoAllocationSize()
        {
            customAllocationSize = false;
        }


        
        public void SetFrom(CustomMeshDataPart<T> meshdatapart)
        {
            customAllocationSize = meshdatapart.customAllocationSize;
            allocationSize = meshdatapart.allocationSize;
            Count = meshdatapart.Count;

            if (meshdatapart.Values != null)
            {
                Values = (T[])meshdatapart.Values.Clone();
            }
            if (meshdatapart.InterleaveSizes != null)
            {
                InterleaveSizes = (int[])meshdatapart.InterleaveSizes.Clone();
            }
            if (meshdatapart.InterleaveOffsets != null)
            {
                InterleaveOffsets = (int[])meshdatapart.InterleaveOffsets.Clone();
            }

            InterleaveStride = meshdatapart.InterleaveStride;
            Instanced = meshdatapart.Instanced;
            StaticDraw = meshdatapart.StaticDraw;
            BaseOffset = meshdatapart.BaseOffset;
        }
    }
}