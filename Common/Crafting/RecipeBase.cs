﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vintagestory.API.Common
{
    public abstract class RecipeBase<T>
    {
        public CraftingRecipeIngredient Ingredient;
        public JsonItemStack Output;
        public AssetLocation Name;
        public bool Enabled = true;

        public abstract Dictionary<string, string[]> GetNameToCodeMapping(IWorldAccessor world);

        public abstract bool Resolve(IWorldAccessor world, string sourceForErrorLogging);

        public abstract T Clone();
    }
}