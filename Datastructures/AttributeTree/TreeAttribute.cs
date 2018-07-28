﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Datastructures
{
    /// <summary>
    /// A datastructure to hold generic data for most primitives (int, string, float, etc.). But can also hold generic data using the ByteArrayAttribute + class serialization
    /// </summary>
    public class TreeAttribute : ITreeAttribute
    {
        protected int depth = 0;

        public static Dictionary<int, Type> AttributeIdMapping = new Dictionary<int, Type>();

        internal OrderedDictionary<string, IAttribute> attributes = new OrderedDictionary<string, IAttribute>();

        public IAttribute this[string key]
        {
            get
            {
                return attributes.TryGetValue(key);
            }

            set
            {
                attributes[key] = value;
            }
        }

        public int Count
        {
            get { return attributes.Count; }
        }

        public IAttribute[] Values
        {
            get { return attributes.Values.ToArray(); }
        }


        static TreeAttribute()
        {
            RegisterAttribute(1, typeof(IntAttribute));
            RegisterAttribute(2, typeof(LongAttribute));
            RegisterAttribute(3, typeof(DoubleAttribute));
            RegisterAttribute(4, typeof(FloatAttribute));
            RegisterAttribute(5, typeof(StringAttribute));
            RegisterAttribute(6, typeof(TreeAttribute));
            RegisterAttribute(7, typeof(ItemstackAttribute));
            RegisterAttribute(8, typeof(ByteArrayAttribute));
            RegisterAttribute(9, typeof(BoolAttribute));

            RegisterAttribute(10, typeof(StringArrayAttribute));
            RegisterAttribute(11, typeof(IntArrayAttribute));
            RegisterAttribute(12, typeof(FloatArrayAttribute));
            RegisterAttribute(13, typeof(DoubleArrayAttribute));
            RegisterAttribute(14, typeof(TreeArrayAttribute));
            RegisterAttribute(15, typeof(LongArrayAttribute));
            RegisterAttribute(16, typeof(BoolArrayAttribute));
        }

        public static void RegisterAttribute(int attrId, Type type)
        {
            AttributeIdMapping[attrId] = type;
        }

        public TreeAttribute()
        {

        }
        
        public virtual void FromBytes(BinaryReader stream)
        {
            if (depth > 30)
            {
                Console.WriteLine("Can't fully decode AttributeTree, beyond 30 depth limit");
                return;
            }

            attributes.Clear();

            byte attrId;
            while ((attrId = stream.ReadByte()) != 0)
            {
                string key = stream.ReadString();

                Type t = AttributeIdMapping[attrId];
                IAttribute attr = (IAttribute)Activator.CreateInstance(t);

                if (attr is TreeAttribute)
                {
                    ((TreeAttribute)attr).depth = depth + 1;
                }

                attr.FromBytes(stream);

                attributes[key] = attr;
            }
        }


        public virtual void ToBytes(BinaryWriter stream)
        {
            foreach (var val in attributes)
            {
                // attrid
                stream.Write((byte)val.Value.GetAttributeId());
                // key
                stream.Write(val.Key);
                // value
                val.Value.ToBytes(stream);
            }

            stream.Write((byte)0);
        }

        public int GetAttributeId()
        {
            return 6;
        }


        public int IndexOf(string key)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes.GetKeyAtIndex(i) == key)
                {
                    return i;
                }
            }

            return -1;
        }


        public IEnumerator<KeyValuePair<string, IAttribute>> GetEnumerator()
        {
            return attributes.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, IAttribute>> IEnumerable<KeyValuePair<string, IAttribute>>.GetEnumerator()
        {
            return attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return attributes.GetEnumerator();
        }





        public IAttribute GetAttribute(string key)
        {
            return attributes.TryGetValue(key);
        }

        public bool HasAttribute(string key)
        {
            return attributes.ContainsKey(key);
        }

        #region Quick access methods

        public virtual void RemoveAttribute(string key)
        {
            attributes.Remove(key);
        }

        public virtual void SetInt(string key, int value)
        {
            attributes[key] = new IntAttribute(value);
        }

        public virtual void SetLong(string key, long value)
        {
            attributes[key] = new LongAttribute(value);
        }

        public virtual void SetDouble(string key, double value)
        {
            attributes[key] = new DoubleAttribute(value);
        }

        public virtual void SetFloat(string key, float value)
        {
            attributes[key] = new FloatAttribute(value);
        }

        public virtual void SetString(string key, string value)
        {
            attributes[key] = new StringAttribute(value);
        }

        public virtual void SetBytes(string key, byte[] value)
        {
            attributes[key] = new ByteArrayAttribute(value);
        }

        public virtual void SetAttribute(string key, IAttribute value)
        {
            attributes[key] = value;
        }

        public void SetItemstack(string key, ItemStack itemstack)
        {
            attributes[key] = new ItemstackAttribute(itemstack);
        }



        public virtual int? TryGetInt(string key)
        {
            return ((IntAttribute)attributes.TryGetValue(key))?.value;
        }

        public virtual double? TryGetDouble(string key)
        {
            return ((DoubleAttribute)attributes.TryGetValue(key))?.value;
        }

        public virtual float? TryGetFloat(string key)
        {
            return ((FloatAttribute)attributes.TryGetValue(key))?.value;
        }


        public virtual int GetInt(string key, int defaultValue = 0)
        {
            IntAttribute attr = attributes.TryGetValue(key) as IntAttribute;
            return attr == null ? defaultValue : attr.value;
        }

        public virtual double GetDouble(string key, double defaultValue = 0)
        {
            DoubleAttribute attr = attributes.TryGetValue(key) as DoubleAttribute;
            return attr == null ? defaultValue : attr.value;
        }

        public virtual float GetFloat(string key, float defaultValue = 0)
        {
            FloatAttribute attr = attributes.TryGetValue(key) as FloatAttribute;
            return attr == null ? defaultValue : attr.value;
        }


        public virtual string GetString(string key, string defaultValue = null)
        {
            string val = (attributes.TryGetValue(key) as StringAttribute)?.value;
            return val == null ? defaultValue : val;
        }

        public virtual byte[] GetBytes(string key, byte[] defaultValue = null)
        {
            byte[] val = (attributes.TryGetValue(key) as ByteArrayAttribute)?.value;
            return val == null ? defaultValue : val;
        }

        public virtual ITreeAttribute GetTreeAttribute(string key)
        {
            return attributes.TryGetValue(key) as ITreeAttribute;
        }

        public ItemStack GetItemstack(string key, ItemStack defaultValue = null)
        {
            ItemStack stack = ((ItemstackAttribute)attributes.TryGetValue(key))?.value;
            return stack == null ? defaultValue : stack;
        }

        public virtual long GetLong(string key, long defaultValue = 0)
        {
            LongAttribute attr = (LongAttribute)attributes.TryGetValue(key);
            return attr == null ? defaultValue : attr.value;
        }

        public virtual long? TryGetLong(string key)
        {
            return ((LongAttribute)attributes.TryGetValue(key))?.value;
        }

        public virtual ModelTransform GetModelTransform(string key)
        {
            ITreeAttribute attr = GetTreeAttribute(key);
            if (attr == null) return null;

            ITreeAttribute torigin = attr.GetTreeAttribute("origin");
            ITreeAttribute trotation = attr.GetTreeAttribute("rotation");
            ITreeAttribute ttranslation = attr.GetTreeAttribute("translation");
            float scale = attr.GetFloat("scale", 1);

            Vec3f origin = new Vec3f(0.5f, 0.5f, 0.5f);
            if (torigin != null)
            {
                origin.X = torigin.GetFloat("x");
                origin.Y = torigin.GetFloat("y");
                origin.Z = torigin.GetFloat("z");
            }

            Vec3f rotation = new Vec3f();
            if (trotation != null)
            {
                rotation.X = trotation.GetFloat("x");
                rotation.Y = trotation.GetFloat("y");
                rotation.Z = trotation.GetFloat("z");
            }

            Vec3f translation = new Vec3f();
            if (ttranslation != null)
            {
                translation.X = ttranslation.GetFloat("x");
                translation.Y = ttranslation.GetFloat("y");
                translation.Z = ttranslation.GetFloat("z");
            }

            return new ModelTransform()
            {
                Scale = scale,
                Origin = origin,
                Translation = translation,
                Rotation = rotation
            };
        }




        public object GetValue()
        {
            return this;
        }


        #endregion


        


        public virtual ITreeAttribute Clone()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            ToBytes(writer);
            ms.Position = 0;

            BinaryReader reader = new BinaryReader(ms);
            SyncedTreeAttribute tree = new SyncedTreeAttribute();
            tree.FromBytes(reader);
            return tree;
        }


        /// <summary>
        /// Returns true if given tree contains all of elements of this one, but given tree may contain also more elements. Individual node values are exactly matched.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSubSetOf(IAttribute other)
        {
            if (!(other is TreeAttribute)) return false;
            TreeAttribute otherTree = (TreeAttribute)other;

            if (attributes.Count > otherTree.attributes.Count) return false;

            foreach (var val in attributes)
            {
                if (!otherTree.attributes.ContainsKey(val.Key)) return false;

                if (val.Value is TreeAttribute)
                {
                    if (!(otherTree.attributes[val.Key] as TreeAttribute).IsSubSetOf(val.Value)) return false;
                } else
                {
                    if (!otherTree.attributes[val.Key].Equals(val.Value)) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if given tree exactly matches this one
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(IAttribute other)
        {
            if (!(other is TreeAttribute)) return false;
            TreeAttribute otherTree = (TreeAttribute)other;

            if (attributes.Count != otherTree.attributes.Count) return false;

            foreach (var val in attributes)
            {
                if (!otherTree.attributes.ContainsKey(val.Key)) return false;
                if (!otherTree.attributes[val.Key].Equals(val.Value)) return false;
            }


            return true;
        }

        public bool Equals(IAttribute other, params string[] ignorePaths)
        {
            return Equals(other, "", ignorePaths);
        }

        public bool Equals(IAttribute other, string currentPath, params string[] ignorePaths)
        {
            if (!(other is TreeAttribute)) return false;
            TreeAttribute otherTree = (TreeAttribute)other;

            if (ignorePaths.Length == 0 && attributes.Count != otherTree.attributes.Count) return false;

            foreach (var val in attributes)
            {
                string curPath = currentPath + (currentPath.Length > 0 ? "/" : "") + val.Key;

                if (ignorePaths.Contains(curPath)) continue;

                if (!otherTree.attributes.ContainsKey(val.Key)) return false;

                IAttribute otherAttr = otherTree.attributes[val.Key];

                if (otherAttr is TreeAttribute)
                {
                    if (!((TreeAttribute)otherAttr).Equals(val.Value, currentPath, ignorePaths)) return false;
                } else
                {
                    if (!otherAttr.Equals(val.Value)) return false;
                }
            }


            return true;
        }

        public string ToJsonToken()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ ");
            int i = 0;

            foreach (var val in attributes)
            {
                if (i > 0) sb.Append(", ");
                i++;

                sb.Append(val.Key + ": " + val.Value.ToJsonToken());
            }

            sb.Append(" }");

            return sb.ToString();
        }
    }
}