﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace Vintagestory.API.Common
{
    /// <summary>
    /// A crafting recipe ingredient
    /// </summary>
    public class CraftingRecipeIngredient
    {
        /// <summary>
        /// Item or Block
        /// </summary>
        public EnumItemClass Type = EnumItemClass.Block;
        /// <summary>
        /// Code of the item or block
        /// </summary>
        public AssetLocation Code;
        /// <summary>
        /// Name of the class, used for filling placeholders in the output stack
        /// </summary>
        public string Name;
        /// <summary>
        /// How much input items are required
        /// </summary>
        public int Quantity = 1;
        /// <summary>
        /// What attributes this itemstack must have
        /// </summary>
        [JsonProperty, JsonConverter(typeof(JsonAttributesConverter))]
        public JsonObject Attributes;
        /// <summary>
        /// Whether this crafting recipe ingredient should be regarded as a tool required to build this item.
        /// If true, the recipe will not consume the item but reduce its durability.
        /// </summary>
        public bool IsTool;
        /// <summary>
        /// When using a wildcrad in the item/block code, setting this field will limit the allowed variants
        /// </summary>
        public string[] AllowedVariants;
        /// <summary>
        /// If set, the crafting recipe will give back the consumed stack to be player upon crafting
        /// </summary>
        public JsonItemStack ReturnedStack;

        /// <summary>
        /// The itemstack made from Code, Quantity and Attributes, populated by the engine
        /// </summary>
        public ItemStack ResolvedItemstack;

        /// <summary>
        /// Whether this recipe contains a wildcard, populated by the engine
        /// </summary>
        public bool IsWildCard = false;

        /// <summary>
        /// Turns Type, Code and Attributes into an IItemStack
        /// </summary>
        /// <param name="resolver"></param>
        public bool Resolve(IWorldAccessor resolver)
        {
            if (ReturnedStack != null)
            {
                ReturnedStack.Resolve(resolver, "ConsumedStack of crafting recipe with output " + Code);
            }

            if (Code.Path.Contains("*"))
            {
                IsWildCard = true;
                return true;
            }

            if (Type == EnumItemClass.Block)
            {
                Block block = resolver.GetBlock(Code);
                if (block == null) return false;

                ResolvedItemstack = new ItemStack(block, Quantity);
            }
            else
            {
                Item item = resolver.GetItem(Code);
                if (item == null) return false;
                ResolvedItemstack = new ItemStack(item, Quantity);
            }

            if (Attributes != null)
            {
                IAttribute attributes = Attributes.ToAttribute();
                if (attributes is ITreeAttribute)
                {
                    ResolvedItemstack.Attributes = (ITreeAttribute)attributes;
                }
            }

            return true;
        }


        public bool SatisfiesAsIngredient(ItemStack inputStack)
        {
            if (inputStack == null) return false;

            if (IsWildCard)
            {
                if (Type != inputStack.Class) return false;
                if (!WildCardMatch(Code, inputStack.Collectible.Code, AllowedVariants)) return false;
                if (inputStack.StackSize < Quantity) return false;
            }
            else
            {
                if (!ResolvedItemstack.Satisfies(inputStack)) return false;
                if (inputStack.StackSize < ResolvedItemstack.StackSize) return false;
            }

            return true;
        }


        public static bool WildCardMatch(AssetLocation wildCard, AssetLocation blockCode, string[] allowedVariants)
        {
            if (blockCode == null || !wildCard.Domain.Equals(blockCode.Domain)) return false;
            if (wildCard.Equals(blockCode)) return true;

            string pattern = Regex.Escape(wildCard.Path).Replace(@"\*", @"(.*)");

            if (!Regex.IsMatch(blockCode.Path, @"^" + pattern + @"$")) return false;

            if (allowedVariants != null)
            {
                int wildcardStartLen = wildCard.Path.IndexOf("*");
                int wildcardEndLen = wildCard.Path.Length - wildcardStartLen - 1;
                string code = blockCode.Path.Substring(wildcardStartLen);
                string codepart = code.Substring(0, code.Length - wildcardEndLen);
                if (!allowedVariants.Contains(codepart)) return false;
            }

            return true;
        }



        internal CraftingRecipeIngredient Clone()
        {
            CraftingRecipeIngredient stack = new CraftingRecipeIngredient()
            {
                Code = Code.Clone(),
                Type = Type,
                Name = Name,
                Quantity = Quantity,
                IsWildCard = IsWildCard,
                IsTool = IsTool,
                AllowedVariants = AllowedVariants == null ? null : (string[])AllowedVariants.Clone(),
                ResolvedItemstack = ResolvedItemstack?.Clone()
            };

            if (Attributes != null) stack.Attributes = Attributes.Clone();

            return stack;
        }


        public override string ToString()
        {
            return Type + " code " + Code;
        }
        


        public void FillPlaceHolder(string key, string value)
        {
            Code = Code.CopyWithPath(Code.Path.Replace("{" + key + "}", value));
            Attributes?.FillPlaceHolder(key, value);
        }

        internal void ToBytes(BinaryWriter writer)
        {
            writer.Write(IsWildCard);
            writer.Write((int)Type);
            writer.Write(Code.ToShortString());
            writer.Write(Quantity);
            if (!IsWildCard)
            {
                ResolvedItemstack.ToBytes(writer);
            }

            writer.Write(IsTool);

            writer.Write(AllowedVariants != null);
            if (AllowedVariants != null)
            {
                writer.Write(AllowedVariants.Length);
                for (int i = 0; i < AllowedVariants.Length; i++)
                {
                    writer.Write(AllowedVariants[i]);
                }
            }

            writer.Write(ReturnedStack?.ResolvedItemstack != null);
            if (ReturnedStack?.ResolvedItemstack != null)
            {
                ReturnedStack.ToBytes(writer);
            }
        }

        internal void FromBytes(BinaryReader reader, IWorldAccessor resolver)
        {
            IsWildCard = reader.ReadBoolean();
            Type = (EnumItemClass)reader.ReadInt32();
            Code = new AssetLocation(reader.ReadString());
            Quantity = reader.ReadInt32();
            if (!IsWildCard)
            {
                ResolvedItemstack = new ItemStack(reader, resolver);
            }

            IsTool = reader.ReadBoolean();

            bool haveVariants = reader.ReadBoolean();
            if (haveVariants)
            {
                AllowedVariants = new string[reader.ReadInt32()];
                for (int i = 0; i < AllowedVariants.Length; i++)
                {
                    AllowedVariants[i] = reader.ReadString();
                }
            }

            bool haveConsumedStack = reader.ReadBoolean();
            if (haveConsumedStack)
            {
                ReturnedStack = new JsonItemStack();
                ReturnedStack.FromBytes(reader, resolver.ClassRegistry);
                ReturnedStack.ResolvedItemstack.ResolveBlockOrItem(resolver);
            }
        }
    }
}