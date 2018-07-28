﻿using Cairo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Vintagestory.API.Common
{
    /// <summary>
    /// Contains all properties shared by Blocks and Items
    /// </summary>
    public abstract class CollectibleObject
    {
        /// <summary>
        /// The block or item id
        /// </summary>
        public abstract int Id { get; }

        /// <summary>
        /// Block or Item?
        /// </summary>
        public abstract EnumItemClass ItemClass { get; }

        /// <summary>
        /// A unique domain + code of the collectible. Must be globally unique for all items / all blocks.
        /// </summary>
        public AssetLocation Code = null;

        /// <summary>
        /// Max amount of collectible that one default inventory slot can hold
        /// </summary>
        public int MaxStackSize = 64;

        /// <summary>
        /// How many uses does this collectible has when being used. Item disappears at durability 0
        /// </summary>
        public int Durability = 1;

        /// <summary>
        /// When true, liquids become selectable to the player when being held in hands
        /// </summary>
        public bool LiquidSelectable;

        /// <summary>
        /// How much damage this collectible deals when used as a weapon
        /// </summary>
        public float AttackPower = 0.5f;

        /// <summary>
        /// Until how for away can you attack entities using this collectibe
        /// </summary>
        public float AttackRange = GlobalConstants.DefaultAttackRange;

        /// <summary>
        /// From which damage sources does the item takes durability damage
        /// </summary>
        public EnumItemDamageSource[] DamagedBy;

        /// <summary>
        /// Modifies how fast the player can break a block when holding this item
        /// </summary>
        public Dictionary<EnumBlockMaterial, float> MiningSpeed;

        /// <summary>
        /// What tier this block can mine when held in hands
        /// </summary>
        public int MiningTier;

        /// <summary>
        /// List of creative tabs in which this collectible should appear in
        /// </summary>
        public string[] CreativeInventoryTabs;

        /// <summary>
        /// If you want to add itemstacks with custom attributes to the creative inventory, add them to this list
        /// </summary>
        public CreativeTabAndStackList[] CreativeInventoryStacks;

        /// <summary>
        /// Alpha test value for rendering in gui, fp hand, tp hand or on the ground
        /// </summary>
        public float RenderAlphaTest = 0.01f;

        /// <summary>
        /// Used for scaling, rotation or offseting the block when rendered in guis
        /// </summary>
        public ModelTransform GuiTransform;

        /// <summary>
        /// Used for scaling, rotation or offseting the block when rendered in the first person mode hand
        /// </summary>
        public ModelTransform FpHandTransform;

        /// <summary>
        /// Used for scaling, rotation or offseting the block when rendered in the third person mode hand
        /// </summary>
        public ModelTransform TpHandTransform;

        /// <summary>
        /// Used for scaling, rotation or offseting the rendered as a dropped item on the ground
        /// </summary>
        public ModelTransform GroundTransform;

        /// <summary>
        /// Custom Attributes that's always assiociated with this item
        /// </summary>
        public JsonObject Attributes;

        /// <summary>
        /// Information about the blocks burnable states
        /// </summary>
        public CombustibleProperties CombustibleProps = null;

        /// <summary>
        /// Information about the blocks nutrition states
        /// </summary>
        public FoodNutritionProperties NutritionProps = null;

        /// <summary>
        /// Information about the blocks grinding properties
        /// </summary>
        public GrindingProperties GrindingProps = null;


        /// <summary>
        /// If set, this item will be classified as given tool
        /// </summary>
        public EnumTool? Tool;

        /// <summary>
        /// Determines in which kind of bags the item can be stored in
        /// </summary>
        public EnumItemStorageFlags StorageFlags = EnumItemStorageFlags.General;

        /// <summary>
        /// Determines on whether an object floats on liquids or not. Water has a density of 1000. Not yet implemented
        /// </summary>
        public int MaterialDensity = 2000;

        /// <summary>
        /// The animation to play in 3rd person mod when hitting with this collectible
        /// </summary>
        public string HeldTpHitAnimation = "breakhand";

        /// <summary>
        /// The animation to play in 3rd person mod when holding this collectible
        /// </summary>
        public string HeldTpIdleAnimation;

        /// <summary>
        /// The animation to play in 3rd person mod when using this collectible
        /// </summary>
        public string HeldTpUseAnimation = "placeblock";


        static int[] ItemDamageColor;

        static CollectibleObject()
        {
            int[] colors = new int[]
            {
                ColorUtil.Hex2Int("#A7251F"),
                ColorUtil.Hex2Int("#F01700"),
                ColorUtil.Hex2Int("#F04900"),
                ColorUtil.Hex2Int("#F07100"),
                ColorUtil.Hex2Int("#F0D100"),
                ColorUtil.Hex2Int("#F0ED00"),
                ColorUtil.Hex2Int("#E2F000"),
                ColorUtil.Hex2Int("#AAF000"),
                ColorUtil.Hex2Int("#71F000"),
                ColorUtil.Hex2Int("#33F000"),
                ColorUtil.Hex2Int("#00F06B"),
            };

            ItemDamageColor = new int[100];
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    ItemDamageColor[10 * i + j] = ColorUtil.ColorOverlay(colors[i], colors[i + 1], j / 10f);
                }
            }
        }



        /// <summary>
        /// Server Side: Called once the collectible has been registered
        /// Client Side: Called once the collectible has been loaded from server packet
        /// </summary>
        public virtual void OnLoaded(ICoreAPI api)
        {
            
        }

        /// <summary>
        /// Called when the client/server is shutting down
        /// </summary>
        /// <param name="api"></param>
        public virtual void OnUnloaded(ICoreAPI api)
        {

        }

        /// <summary>
        /// Should return in which storage containers this item can be placed in
        /// </summary>
        /// <param name="itemstack"></param>
        /// <returns></returns>
        public virtual EnumItemStorageFlags GetStorageFlags(ItemStack itemstack)
        {
            // We clear the backpack flag if the backpack is empty
            if ((StorageFlags & EnumItemStorageFlags.Backpack) > 0 && IsEmptyBackPack(itemstack)) return EnumItemStorageFlags.General | EnumItemStorageFlags.Backpack;

            return StorageFlags;
        }

        /// <summary>
        /// Returns a hardcoded rgb color (green->yellow->red) that is representative for its remaining durability vs total durability
        /// </summary>
        /// <param name="itemstack"></param>
        /// <returns></returns>
        public virtual int GetItemDamageColor(ItemStack itemstack)
        {
            int p = GameMath.Clamp((100 * itemstack.Attributes.GetInt("durability")) / Durability, 0, 99);

            return ItemDamageColor[p];
        }

        /// <summary>
        /// Return true if remaining durability != total durability
        /// </summary>
        /// <param name="itemstack"></param>
        /// <returns></returns>
        public virtual bool ShouldDisplayItemDamage(IItemStack itemstack)
        {
            return Durability != itemstack.Attributes.GetInt("durability", Durability);
        }



        /// <summary>
        /// This method is called before rendering the item stack into GUI, first person hand, third person hand and/or on the ground
        /// The renderinfo object is pre-filled with default values. 
        /// </summary>
        /// <param name="capi"></param>
        /// <param name="itemstack"></param>
        /// <param name="target"></param>
        /// <param name="renderinfo"></param>
        public virtual void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            
        }


        /// <summary>
        /// Returns the items remaining durability
        /// </summary>
        /// <param name="itemstack"></param>
        /// <returns></returns>
        public virtual float GetDurability(IItemStack itemstack)
        {
            return Durability;
        }

        /// <summary>
        /// The amount of damage dealt when used as a weapon
        /// </summary>
        /// <param name="withItemStack"></param>
        /// <returns></returns>
        public virtual float GetAttackPower(IItemStack withItemStack)
        {
            return AttackPower;
        }

        /// <summary>
        /// The the attack range when used as a weapon
        /// </summary>
        /// <param name="withItemStack"></param>
        /// <returns></returns>
        public virtual float GetAttackRange(IItemStack withItemStack)
        {
            return AttackRange;
        }


        /// <summary>
        /// Player is holding this collectible and breaks the targeted block
        /// </summary>
        /// <param name="player"></param>
        /// <param name="blockSel"></param>
        /// <param name="itemslot"></param>
        /// <param name="remainingResistance"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public virtual float OnBlockBreaking(IPlayer player, BlockSelection blockSel, IItemSlot itemslot, float remainingResistance, float dt)
        {
            Block block = player.Entity.World.BlockAccessor.GetBlock(blockSel.Position);

            if (block.RequiredMiningTier > 0 && (itemslot.Itemstack.Collectible.MiningTier < block.RequiredMiningTier || !MiningSpeed.ContainsKey(block.BlockMaterial)))
            {
                return remainingResistance;
            }

            return remainingResistance - GetMiningSpeed(itemslot.Itemstack, block) * dt;
        }


        /// <summary>
        /// Player has broken a block while holding this collectible. Return false if you want to cancel the block break event.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="byEntity"></param>
        /// <param name="itemslot"></param>
        /// <param name="blockSel"></param>
        /// <returns></returns>
        public virtual bool OnBlockBrokenWith(IWorldAccessor world, IEntity byEntity, IItemSlot itemslot, BlockSelection blockSel)
        {
            IPlayer byPlayer = null;
            if (byEntity is IEntityPlayer) byPlayer = world.PlayerByUid(((IEntityPlayer)byEntity).PlayerUID);

            Block block = world.BlockAccessor.GetBlock(blockSel.Position);
            block.OnBlockBroken(world, blockSel.Position, byPlayer);

            if (DamagedBy != null && DamagedBy.Contains(EnumItemDamageSource.BlockBreaking))
            {
                DamageItem(world, byEntity, itemslot);
            }

            return true;
        }



        /// <summary>
        /// Called every game tick when the player breaks a block with this item in his hands. Returns the mining speed for given block.
        /// </summary>
        /// <param name="itemstack"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public virtual float GetMiningSpeed(IItemStack itemstack, Block block)
        {
            if (MiningSpeed == null || !MiningSpeed.ContainsKey(block.BlockMaterial)) return 1f;

            return MiningSpeed[block.BlockMaterial];
        }


        /// <summary>
        /// Not implemented yet
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <returns></returns>
        public virtual ModelTransformKeyFrame[] GeldHeldFpHitAnimation(IItemSlot slot, IEntity byEntity)
        {
            return null;
        }

        /// <summary>
        /// Called when an entity uses this item to hit something in 3rd person mode
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <returns></returns>
        public virtual string GetHeldTpHitAnimation(IItemSlot slot, IEntity byEntity)
        {
            return HeldTpHitAnimation;
        }

        /// <summary>
        /// Called when an entity holds this item in hands in 3rd person mode
        /// </summary>
        /// <param name="activeHotbarSlot"></param>
        /// <param name="byEntity"></param>
        /// <returns></returns>
        public virtual string GetHeldTpIdleAnimation(IItemSlot activeHotbarSlot, IEntity byEntity)
        {
            return HeldTpIdleAnimation;
        }

        /// <summary>
        /// Called when an entity holds this item in hands in 3rd person mode
        /// </summary>
        /// <param name="activeHotbarSlot"></param>
        /// <param name="byEntity"></param>
        /// <returns></returns>
        public virtual string GetHeldTpUseAnimation(IItemSlot activeHotbarSlot, IEntity byEntity)
        {
            if (this.NutritionProps != null) return null;

            return HeldTpUseAnimation;
        }

        /// <summary>
        /// An entity used this collectibe to attack something
        /// </summary>
        /// <param name="world"></param>
        /// <param name="byEntity"></param>
        /// <param name="itemslot"></param>
        public virtual void OnAttackingWith(IWorldAccessor world, IEntity byEntity, IItemSlot itemslot)
        {
            if (DamagedBy != null && DamagedBy.Contains(EnumItemDamageSource.Attacking))
            {
                DamageItem(world, byEntity, itemslot);
            }
        }


        /// <summary>
        /// Called when this collectible is being used as part of a crafting recipe and should get consumed now
        /// </summary>
        /// <param name="stackInSlot"></param>
        /// <param name="fromIngredient"></param>
        /// <param name="byPlayer"></param>
        public virtual void OnConsumedByCrafting(IItemSlot[] allInputSlots, IItemSlot stackInSlot, CraftingRecipeIngredient fromIngredient, IPlayer byPlayer, int quantity)
        {
            if (fromIngredient.IsTool)
            {
                stackInSlot.Itemstack.Collectible.DamageItem(byPlayer.Entity.World, byPlayer.Entity, stackInSlot);
            }
            else
            {
                stackInSlot.Itemstack.StackSize -= quantity;

                if (stackInSlot.Itemstack.StackSize <= 0)
                {
                    stackInSlot.Itemstack = null;
                    stackInSlot.MarkDirty();
                }

                if (fromIngredient.ReturnedStack != null)
                {
                    byPlayer.InventoryManager.TryGiveItemstack(fromIngredient.ReturnedStack.ResolvedItemstack.Clone(), true);
                }
            }
        }



        /// <summary>
        /// Causes the item to be damaged. Will play a breaking sound and removes the itemstack if no more durability is left
        /// </summary>
        /// <param name="world"></param>
        /// <param name="byEntity"></param>
        /// <param name="itemslot"></param>
        public virtual void DamageItem(IWorldAccessor world, IEntity byEntity, IItemSlot itemslot)
        {
            IItemStack itemstack = itemslot.Itemstack;

            int leftDurability = itemstack.Attributes.GetInt("durability", Durability);
            leftDurability--;
            itemstack.Attributes.SetInt("durability", leftDurability);

            if (leftDurability <= 0)
            {
                itemslot.Itemstack = null;

                if (byEntity is IEntityPlayer)
                {
                    IPlayer player = world.PlayerByUid(((IEntityPlayer)byEntity).PlayerUID);
                    world.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), player, player);
                } else
                {
                    world.PlaySoundAt(new AssetLocation("sounds/effect/toolbreak"), byEntity.Pos.X, byEntity.Pos.Y, byEntity.Pos.Z);
                }

                
            }

            itemslot.MarkDirty();
        }

        /// <summary>
        /// Should return the amount of tool modes this collectible has
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="byPlayer"></param>
        /// <param name="blockSelection"></param>
        /// <returns></returns>
        public virtual int GetQuantityToolModes(IItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection)
        {
            return 0;
        }

        /// <summary>
        /// Should draw given tool mode icon
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="byPlayer"></param>
        /// <param name="blockSelection"></param>
        /// <param name="cr"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="toolMode"></param>
        /// <param name="color"></param>
        public virtual void DrawToolModeIcon(IItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection, Context cr, int x, int y, int width, int height, int toolMode, int color)
        {

        }

        /// <summary>
        /// Should return the current items tool mode.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="byPlayer"></param>
        /// <param name="blockSelection"></param>
        /// <returns></returns>
        public virtual int GetToolMode(IItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection)
        {
            return 0;
        }

        /// <summary>
        /// Should set given toolmode
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="byPlayer"></param>
        /// <param name="blockSelection"></param>
        /// <param name="toolMode"></param>
        public virtual void SetToolMode(IItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection, int toolMode)
        {
            
        }

        /// <summary>
        /// This method is called during the opaque render pass when this item or block is being held in hands
        /// </summary>
        /// <param name="inSlot"></param>
        /// <param name="byPlayer"></param>
        public virtual void OnHeldRenderOpaque(IItemSlot inSlot, IClientPlayer byPlayer)
        {

        }

        /// <summary>
        /// This method is called during the order independent transparency render pass when this item or block is being held in hands
        /// </summary>
        /// <param name="inSlot"></param>
        /// <param name="byPlayer"></param>
        public virtual void OnHeldRenderOit(IItemSlot inSlot, IClientPlayer byPlayer)
        {

        }

        /// <summary>
        /// This method is called during the ortho (for 2D GUIs) render pass when this item or block is being held in hands
        /// </summary>
        /// <param name="inSlot"></param>
        /// <param name="byPlayer"></param>
        public virtual void OnHeldRenderOrtho(IItemSlot inSlot, IClientPlayer byPlayer)
        {

        }



        /// <summary>
        /// Called every frame when the player is holding this collectible in his hands. Is not called during OnUsing() or OnAttacking()
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        public virtual void OnHeldIdle(IItemSlot slot, IEntityAgent byEntity)
        {

        }

        /// <summary>
        /// Called every game tick when this collectible lies dropped on the ground
        /// </summary>
        /// <param name="entityItem"></param>
        public virtual void OnGroundIdle(EntityItem entityItem)
        {
            
        }

        /// <summary>
        /// Called every frame when this item is being displayed in the gui
        /// </summary>
        /// <param name="world"></param>
        /// <param name="stack"></param>
        public virtual void InGuiIdle(IWorldAccessor world, ItemStack stack)
        {

        }



        /// <summary>
        /// General begin use access. Override OnHeldAttackStart or OnHeldInteractStart to alter the behavior.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <param name="blockSel"></param>
        /// <param name="entitySel"></param>
        /// <param name="useType"></param>
        /// <returns></returns>
        public EnumHandInteract OnHeldUseStart(IItemSlot slot, IEntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumHandInteract useType)
        {
            bool shouldBegin = useType == EnumHandInteract.HeldItemAttack ? OnHeldAttackStart(slot, byEntity, blockSel, entitySel) : OnHeldInteractStart(slot, byEntity, blockSel, entitySel);

            return shouldBegin ? useType : EnumHandInteract.None;
        }

        /// <summary>
        /// General cancel use access. Override OnHeldAttackCancel or OnHeldInteractCancel to alter the behavior.
        /// </summary>
        /// <param name="secondsPassed"></param>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <param name="blockSel"></param>
        /// <param name="entitySel"></param>
        /// <param name="cancelReason"></param>
        /// <returns></returns>
        public EnumHandInteract OnHeldUseCancel(float secondsPassed, IItemSlot slot, IEntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            EnumHandInteract useType = byEntity.Controls.HandUse;

            bool allowCancel = useType == EnumHandInteract.HeldItemAttack ? OnHeldAttackCancel(secondsPassed, slot, byEntity, blockSel, entitySel, cancelReason) : OnHeldInteractCancel(secondsPassed, slot, byEntity, blockSel, entitySel, cancelReason);
            return allowCancel ? EnumHandInteract.None : useType;
        }

        /// <summary>
        /// General using access. Override OnHeldAttackStep or OnHeldInteractStep to alter the behavior.
        /// </summary>
        /// <param name="secondsPassed"></param>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <param name="blockSel"></param>
        /// <param name="entitySel"></param>
        /// <returns></returns>
        public EnumHandInteract OnHeldUseStep(float secondsPassed, IItemSlot slot, IEntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            EnumHandInteract useType = byEntity.Controls.HandUse;

            bool shouldContinueUse = useType == EnumHandInteract.HeldItemAttack ? OnHeldAttackStep(secondsPassed, slot, byEntity, blockSel, entitySel) : OnHeldInteractStep(secondsPassed, slot, byEntity, blockSel, entitySel);

            return shouldContinueUse ? useType : EnumHandInteract.None;
        }

        /// <summary>
        /// General use over access. Override OnHeldAttackStop or OnHeldInteractStop to alter the behavior.
        /// </summary>
        /// <param name="secondsPassed"></param>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <param name="blockSel"></param>
        /// <param name="entitySel"></param>
        /// <param name="useType"></param>
        public void OnHeldUseStop(float secondsPassed, IItemSlot slot, IEntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumHandInteract useType)
        {
            if (useType == EnumHandInteract.HeldItemAttack)
            {
                OnHeldAttackStop(secondsPassed, slot, byEntity, blockSel, entitySel);
            } else
            {
                OnHeldInteractStop(secondsPassed, slot, byEntity, blockSel, entitySel);
            }
        }


        /// <summary>
        /// When the player has begun using this item for attacking (left mouse click). Return true to play a custom action.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <param name="blockSel"></param>
        /// <param name="entitySel"></param>
        /// <returns></returns>
        public virtual bool OnHeldAttackStart(IItemSlot slot, IEntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            return false;
        }

        /// <summary>
        /// When the player has canceled a custom attack action.
        /// </summary>
        /// <param name="secondsPassed"></param>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <param name="blockSelection"></param>
        /// <param name="entitySel"></param>
        /// <param name="cancelReason"></param>
        /// <returns></returns>
        public virtual bool OnHeldAttackCancel(float secondsPassed, IItemSlot slot, IEntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            return false;
        }

        /// <summary>
        /// Called continously when a custom attack action is playing. Return false to stop the action.
        /// </summary>
        /// <param name="secondsPassed"></param>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <param name="blockSelection"></param>
        /// <param name="entitySel"></param>
        /// <returns></returns>
        public virtual bool OnHeldAttackStep(float secondsPassed, IItemSlot slot, IEntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel)
        {
            return false;
        }

        /// <summary>
        /// Called when a custom attack action is finished
        /// </summary>
        /// <param name="secondsPassed"></param>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <param name="blockSelection"></param>
        /// <param name="entitySel"></param>
        public virtual void OnHeldAttackStop(float secondsPassed, IItemSlot slot, IEntityAgent byEntity, BlockSelection blockSelection, EntitySelection entitySel)
        {
            
        }


        /// <summary>
        /// Called when the player right clicks while holding this block/item in his hands
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <param name="blockSel"></param>
        /// <param name="entitySel"></param>
        /// <returns>True if an interaction should happen (makes it sync to the server), false if no sync to server is required</returns>
        public virtual bool OnHeldInteractStart(IItemSlot slot, IEntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (NutritionProps != null)
            {
                byEntity.World.RegisterCallback((dt) =>
                {
                    if (byEntity.Controls.HandUse == EnumHandInteract.HeldItemInteract)
                    {
                        IPlayer player = null;
                        if (byEntity is IEntityPlayer) player = byEntity.World.PlayerByUid(((IEntityPlayer)byEntity).PlayerUID);

                        byEntity.PlayEntitySound("eat", player);
                    }
                }, 500);

                byEntity.StartAnimation("eat");

                return true;
            }

            return false;
        }


        /// <summary>
        /// Called every frame while the player is using this collectible. Return false to stop the interaction.
        /// </summary>
        /// <param name="secondsUsed"></param>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <param name="blockSel"></param>
        /// <param name="entitySel"></param>
        /// <returns>False if the interaction should be stopped. True if the interaction should continue</returns>
        public virtual bool OnHeldInteractStep(float secondsUsed, IItemSlot slot, IEntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (NutritionProps == null) return false;

            if (byEntity.World is IClientWorldAccessor)
            {
                ModelTransform tf = new ModelTransform();
                tf.Origin.Set(1.1f, 0.5f, 0.5f);
                tf.EnsureDefaultValues();

                if (ItemClass == EnumItemClass.Item)
                {
                    if (secondsUsed > 0.5f)
                    {
                        tf.Translation.X = GameMath.Sin(30 * secondsUsed) / 10;
                    }

                    tf.Translation.Z += -Math.Min(1.2f, secondsUsed * 4 * 1.57f);
                    tf.Translation.Y += Math.Min(0.5f, secondsUsed * 2);

                    tf.Rotation.Y -= Math.Min(85f, secondsUsed * 350 * 1.5f);
                    tf.Rotation.X += Math.Min(40f, secondsUsed * 350 * 0.75f);
                    tf.Rotation.Z += Math.Min(40f, secondsUsed * 350 * 0.75f);
                }
                else
                {
                    tf.Translation.X -= Math.Min(1.7f, secondsUsed * 4 * 1.8f);
                    tf.Scale = 1 + Math.Min(0.5f, secondsUsed * 4 * 1.8f);
                    tf.Rotation.X += Math.Min(40f, secondsUsed * 350 * 0.75f);

                    if (secondsUsed > 0.5f)
                    {
                        tf.Translation.Y = GameMath.Sin(30 * secondsUsed) / 10;
                    }

                }

                byEntity.Controls.UsingHeldItemTransform = tf;

                Vec3d pos = byEntity.Pos.AheadCopy(0.4f).XYZ;
                pos.Y += byEntity.EyeHeight() - 0.4f;

                if (secondsUsed > 0.5f && (int)(30 * secondsUsed) % 7 == 1)
                {
                    if (slot.Itemstack.Class == EnumItemClass.Block)
                    {
                        byEntity.World.SpawnBlockVoxelParticles(pos, slot.Itemstack.Block, 0.3f, 4, 0.5f);
                    }
                    else
                    {
                        byEntity.World.SpawnItemVoxelParticles(pos, slot.Itemstack.Item, 0.3f, 4, 0.5f);
                    }
                }

                return secondsUsed <= 1f;
            }

            // Let the client decide when he is done eating
            return true;
        }

        /// <summary>
        /// Called when the player successfully completed the using action, always called once an interaction is over
        /// </summary>
        /// <param name="secondsUsed"></param>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <param name="blockSel"></param>
        /// <param name="entitySel"></param>
        public virtual void OnHeldInteractStop(float secondsUsed, IItemSlot slot, IEntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (byEntity.World is IServerWorldAccessor && NutritionProps != null && secondsUsed >= 0.95f)
            {
                byEntity.ReceiveSaturation(NutritionProps.Saturation);

                if (NutritionProps.EatenStack != null)
                {
                    IPlayer player = null;
                    if (byEntity is IEntityPlayer) player = byEntity.World.PlayerByUid(((IEntityPlayer)byEntity).PlayerUID);

                    if (player == null || !player.InventoryManager.TryGiveItemstack(NutritionProps.EatenStack.ResolvedItemstack.Clone(), true))
                    {
                        byEntity.World.SpawnItemEntity(NutritionProps.EatenStack.ResolvedItemstack.Clone(), byEntity.LocalPos.XYZ);
                    }
                }

                slot.Itemstack.StackSize--;
                
                if (NutritionProps.Health != 0)
                {
                    byEntity.ReceiveDamage(new DamageSource() { source = EnumDamageSource.Internal, type = NutritionProps.Health > 0 ? EnumDamageType.Heal : EnumDamageType.Poison }, Math.Abs(NutritionProps.Health));
                }

                slot.MarkDirty();
            }
        }

        /// <summary>
        /// When the player released the right mouse button. Return false to deny the cancellation (= will keep using the item until OnHeldInteractStep returns false).
        /// </summary>
        /// <param name="secondsUsed"></param>
        /// <param name="slot"></param>
        /// <param name="byEntity"></param>
        /// <param name="blockSel"></param>
        /// <param name="entitySel"></param>
        /// <param name="cancelReason"></param>
        /// <returns></returns>
        public virtual bool OnHeldInteractCancel(float secondsUsed, IItemSlot slot, IEntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
        {
            return true;
        }


        /// <summary>
        /// Callback when the player dropped this item from his inventory. You can set handling to PreventDefault to prevent dropping this item.
        /// You can also check if the entityplayer of this player is dead to check if dropping of this item was due the players death
        /// </summary>
        /// <param name="world"></param>
        /// <param name="byPlayer"></param>
        /// <param name="slot"></param>
        /// <param name="quantity">Amount of items the player wants to drop</param>
        /// <param name="handling"></param>
        public virtual void OnHeldDropped(IWorldAccessor world, IPlayer byPlayer, IItemSlot slot, int quantity, ref EnumHandling handling)
        {
            
        }




        /// <summary>
        /// Called by the inventory system when you hover over an item stack. This is the text that is getting displayed.
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="dsc"></param>
        /// <param name="world"></param>
        /// <param name="withDebugInfo"></param>
        public virtual void GetHeldItemInfo(ItemStack stack, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            string descLangCode = Code?.Domain + AssetLocation.LocationSeparator + ItemClass.ToString().ToLowerInvariant() + "desc-" + Code?.Path;
            string descText = Lang.GetMatching(descLangCode);
            if (descText == descLangCode) descText = "";
            else descText = descText + "\n";

            dsc.Append((withDebugInfo ? "Id: " + Id + "\n" : ""));
            dsc.Append((withDebugInfo ? "Code: " + Code + "\n" : ""));

            float temp = GetTemperature(world, stack);
            if (temp > 20)
            {
                dsc.AppendLine(Lang.Get("Temperature: {0}°C", (int)temp));
            }

            if (Durability > 1)
            {
                dsc.AppendLine(Lang.Get("Durability: {0} / {1}", stack.Attributes.GetInt("durability", Durability), Durability));
            }
            

            if (MiningSpeed != null && MiningSpeed.Count > 0)
            {
                dsc.AppendLine(Lang.Get("Tool Tier: {0}", MiningTier));

                dsc.Append(Lang.Get("Mining speed: "));
                int i = 0;
                foreach (var val in MiningSpeed)
                {
                    if (val.Value < 1.1) continue;

                    if (i > 0) dsc.Append(", ");
                    dsc.Append(val.Key + " " + val.Value.ToString("#.#") + "x");
                    i++;
                }

                dsc.Append("\n");
                
            }

            if (IsBackPack(stack))
            {
                dsc.AppendLine(Lang.Get("Quantity Slots: {0}", QuantityBackPackSlots(stack)));
                ITreeAttribute backPackTree = stack.Attributes.GetTreeAttribute("backpack");
                if (backPackTree != null)
                {
                    dsc.AppendLine(Lang.Get("Contents: "));

                    ITreeAttribute slotsTree = backPackTree.GetTreeAttribute("slots");

                    foreach (var val in slotsTree)
                    {
                        IItemStack cstack = (IItemStack)val.Value?.GetValue();
                        if (cstack != null && cstack.StackSize > 0)
                        {
                            dsc.AppendLine("- " + cstack.StackSize + "x " + cstack.GetName());
                        }
                    }

                }


            }

            if (NutritionProps != null)
            {
                if (Math.Abs(NutritionProps.Health) > 0.001f)
                {
                    dsc.Append(Lang.Get("When eaten: {0} sat, {1} hp\n", NutritionProps.Saturation, NutritionProps.Health));
                }
                else
                {
                    dsc.Append(Lang.Get("When eaten: {0} sat\n", NutritionProps.Saturation));
                }
            }

            if (GrindingProps != null)
            {
                dsc.Append(Lang.Get("When ground: Turns into {0}x {1}", GrindingProps.GrindedStack.ResolvedItemstack.StackSize, GrindingProps.GrindedStack.ResolvedItemstack.GetName()));
            }

            if (GetAttackPower(stack) > 0.5f)
            {
                dsc.Append(Lang.Get("Attack power: -{0} hp\n", GetAttackPower(stack).ToString("0.#")));
            }

            if (GetAttackRange(stack) > GlobalConstants.DefaultAttackRange)
            {
                dsc.Append(Lang.Get("Attack range: {0} m\n", GetAttackRange(stack).ToString("0.#")));
            }

            if (CombustibleProps != null)
            {
                if (CombustibleProps.BurnTemperature > 0)
                {
                    dsc.AppendLine(Lang.Get("Burn temperature: {0}°C", CombustibleProps.BurnTemperature));
                    dsc.Append(Lang.Get("Burn duration: {0}s", CombustibleProps.BurnDuration));
                }
                

                string smelttype = CombustibleProps.SmeltingType.ToString().ToLowerInvariant();
                if (CombustibleProps.MeltingPoint > 0)
                {
                    dsc.AppendLine(Lang.Get("game:smeltpoint-" + smelttype, CombustibleProps.MeltingPoint));
                }

                if (CombustibleProps.SmeltedStack != null)
                {
                    int instacksize = CombustibleProps.SmeltedRatio;
                    int outstacksize = CombustibleProps.SmeltedStack.ResolvedItemstack.StackSize;
                    

                    string str = instacksize == 1 ?
                        Lang.Get("game:smeltdesc-"+ smelttype+"-singular", outstacksize, CombustibleProps.SmeltedStack.ResolvedItemstack.GetName()) :
                        Lang.Get("game:smeltdesc-" + smelttype+"-plural", instacksize, outstacksize, CombustibleProps.SmeltedStack.ResolvedItemstack.GetName())
                    ;
                    
                    dsc.AppendLine(str);
                }

                
            }

            if (descText.Length > 0 && dsc.Length > 0) dsc.Append("\n");
            dsc.Append(descText);
        }


        /// <summary>
        /// Should return true if the stack can be placed into given slot
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public virtual bool CanBePlacedInto(ItemStack stack, IItemSlot slot)
        {
            return slot.StorageType == 0 || (slot.StorageType & GetStorageFlags(stack)) > 0;
        }

        /// <summary>
        /// Should return the max. number of items that can be placed from sourceStack into the sinkStack
        /// </summary>
        /// <param name="sinkStack"></param>
        /// <param name="sourceStack"></param>
        /// <returns></returns>
        public virtual int GetMergableQuantity(ItemStack sinkStack, ItemStack sourceStack)
        {
            if (Equals(sinkStack, sourceStack, GlobalConstants.IgnoredStackAttributes) && sinkStack.StackSize < MaxStackSize)
            {
                return Math.Min(MaxStackSize - sinkStack.StackSize, sourceStack.StackSize);
            }

            return 0;
        }

        /// <summary>
        /// Is always called on the sink slots item
        /// </summary>
        /// <param name="op"></param>
        public virtual void TryMergeStacks(ItemStackMergeOperation op)
        {
            op.MovableQuantity = GetMergableQuantity(op.SinkSlot.Itemstack, op.SourceSlot.Itemstack);
            if (op.MovableQuantity == 0) return;
            if (!op.SinkSlot.CanTakeFrom(op.SourceSlot)) return;

            op.MovedQuantity = Math.Min(op.MovableQuantity, op.RequestedQuantity);

            if (HasTemperature(op.SinkSlot.Itemstack) || HasTemperature(op.SourceSlot.Itemstack))
            {
                if (op.CurrentPriority < EnumMergePriority.DirectMerge)
                {
                    float tempDiff = Math.Abs(GetTemperature(op.World, op.SinkSlot.Itemstack) - GetTemperature(op.World, op.SourceSlot.Itemstack));
                    if (tempDiff > 10)
                    {
                        op.MovedQuantity = 0;
                        op.MovableQuantity = 0;
                        op.RequiredPriority = EnumMergePriority.ConfirmedMerge;
                        return;
                    }
                }
                
                SetTemperature(
                    op.World,
                    op.SinkSlot.Itemstack,
                    (op.SinkSlot.StackSize * GetTemperature(op.World, op.SinkSlot.Itemstack) + op.MovedQuantity * GetTemperature(op.World, op.SourceSlot.Itemstack)) / (op.SinkSlot.StackSize + op.MovedQuantity)
                );
            }


            op.SinkSlot.Itemstack.StackSize += op.MovedQuantity;
            op.SourceSlot.Itemstack.StackSize -= op.MovedQuantity;

            if (op.SourceSlot.Itemstack.StackSize <= 0)
            {
                op.SourceSlot.Itemstack = null;
            }
        }


        /// <summary>
        /// If the item is smeltable, this is the time it takes to smelt at smelting point
        /// </summary>
        /// <param name="world"></param>
        /// <param name="cookingSlotsProvider"></param>
        /// <param name="inputSlot"></param>
        /// <returns></returns>
        public virtual float GetMeltingDuration(IWorldAccessor world, ISlotProvider cookingSlotsProvider, IItemSlot inputSlot)
        {
            return CombustibleProps == null ? 0 : CombustibleProps.MeltingDuration;
        }

        /// <summary>
        /// If the item is smeltable, this is its melting point
        /// </summary>
        /// <param name="world"></param>
        /// <param name="cookingSlotsProvider"></param>
        /// <param name="inputSlot"></param>
        /// <returns></returns>
        public virtual float GetMeltingPoint(IWorldAccessor world, ISlotProvider cookingSlotsProvider, IItemSlot inputSlot)
        {
            return CombustibleProps == null ? 0 : CombustibleProps.MeltingPoint;
        }


        /// <summary>
        /// Should return true if this collectible is smeltable
        /// </summary>
        /// <param name="world"></param>
        /// <param name="cookingSlotsProvider"></param>
        /// <param name="inputStack"></param>
        /// <param name="outputStack"></param>
        /// <returns></returns>
        public virtual bool CanSmelt(IWorldAccessor world, ISlotProvider cookingSlotsProvider, ItemStack inputStack, ItemStack outputStack)
        {
            ItemStack smeltedStack = CombustibleProps?.SmeltedStack?.ResolvedItemstack;

            return
                smeltedStack != null
                && inputStack.StackSize >= CombustibleProps.SmeltedRatio
                && CombustibleProps.MeltingPoint > 0
                && (outputStack == null || outputStack.Collectible.GetMergableQuantity(outputStack, smeltedStack) >= smeltedStack.StackSize)
            ;
        }

        /// <summary>
        /// Transform the item to it's smelted variant
        /// </summary>
        /// <param name="world"></param>
        /// <param name="cookingSlotsProvider"></param>
        /// <param name="inputSlot"></param>
        /// <param name="outputSlot"></param>
        public virtual void DoSmelt(IWorldAccessor world, ISlotProvider cookingSlotsProvider, IItemSlot inputSlot, IItemSlot outputSlot)
        {
            if (!CanSmelt(world, cookingSlotsProvider, inputSlot.Itemstack, outputSlot.Itemstack)) return;

            ItemStack smeltedStack = CombustibleProps.SmeltedStack.ResolvedItemstack;

            int batchSize = 1;

            if (outputSlot.Itemstack == null)
            {
                outputSlot.Itemstack = smeltedStack.Clone();
                outputSlot.Itemstack.StackSize = batchSize * smeltedStack.StackSize;
            }
            else
            {
                outputSlot.Itemstack.StackSize += batchSize * smeltedStack.StackSize;
            }

            inputSlot.Itemstack.StackSize -= batchSize * CombustibleProps.SmeltedRatio;

            if (inputSlot.Itemstack.StackSize <= 0)
            {
                inputSlot.Itemstack = null;
            }

            outputSlot.MarkDirty();
        }



        /// <summary>
        /// Returns true if the stack has a temperature attribute
        /// </summary>
        /// <param name="itemstack"></param>
        /// <returns></returns>
        public virtual bool HasTemperature(IItemStack itemstack)
        {
            if (itemstack == null || itemstack.Attributes == null) return false;
            return itemstack.Attributes.HasAttribute("temperature");
        }

        /// <summary>
        /// Returns the stacks item temperature in degree celsius
        /// </summary>
        /// <param name="world"></param>
        /// <param name="itemstack"></param>
        /// <returns></returns>
        public virtual float GetTemperature(IWorldAccessor world, ItemStack itemstack)
        {
            if (
                itemstack == null
                || itemstack.Attributes == null
                || itemstack.Attributes["temperature"] == null
                || !(itemstack.Attributes["temperature"] is ITreeAttribute)
            )
            {
                return 20;
            }

            ITreeAttribute attr = ((ITreeAttribute)itemstack.Attributes["temperature"]);

            double nowHours = world.Calendar.TotalHours;
            double lastUpdateHours = attr.GetDouble("temperatureLastUpdate");

            double hourDiff = nowHours - lastUpdateHours;

            // 1.5 deg per irl second
            // 1 game hour = irl 60 seconds
            if (hourDiff > 1/85f)
            {
                float temp = Math.Max(0, attr.GetFloat("temperature", 20) - Math.Max(0, (float)(nowHours - lastUpdateHours) * attr.GetFloat("cooldownSpeed", 90)));
                SetTemperature(world, itemstack, temp);
                return temp;
            }

            return attr.GetFloat("temperature", 20);
        }

        /// <summary>
        /// Sets the stacks item temperature in degree celsius
        /// </summary>
        /// <param name="world"></param>
        /// <param name="itemstack"></param>
        /// <param name="temperature"></param>
        /// <param name="delayCooldown"></param>
        public virtual void SetTemperature(IWorldAccessor world, ItemStack itemstack, float temperature, bool delayCooldown = true)
        {
            if (itemstack == null) return;

            ITreeAttribute attr = ((ITreeAttribute)itemstack.Attributes["temperature"]);

            if (attr == null)
            {
                itemstack.Attributes["temperature"] = attr = new TreeAttribute();
            }

            double nowHours = world.Calendar.TotalHours;
            // If the colletible gets heated, retain the heat for 1.5 ingame hours
            if (delayCooldown && attr.GetFloat("temperature") < temperature) nowHours += 1.5f;

            attr.SetDouble("temperatureLastUpdate", nowHours);
            attr.SetFloat("temperature", temperature);
        }

        /// <summary>
        /// Returns true if this stack is an empty backpack
        /// </summary>
        /// <param name="itemstack"></param>
        /// <returns></returns>
        public static bool IsEmptyBackPack(IItemStack itemstack)
        {
            if (!IsBackPack(itemstack)) return false;

            ITreeAttribute backPackTree = itemstack.Attributes.GetTreeAttribute("backpack");
            if (backPackTree == null) return true;
            ITreeAttribute slotsTree = backPackTree.GetTreeAttribute("slots");

            foreach (var val in slotsTree)
            {
                IItemStack stack = (IItemStack)val.Value?.GetValue();
                if (stack != null && stack.StackSize > 0) return false;
            }
            return true;
        }


        /// <summary>
        /// Returns true if this stack is a backpack that can hold other items/blocks
        /// </summary>
        /// <param name="itemstack"></param>
        /// <returns></returns>
        public static bool IsBackPack(IItemStack itemstack)
        {
            if (itemstack == null || itemstack.Collectible.Attributes == null) return false;
            return itemstack.Collectible.Attributes["backpack"]["quantitySlots"].AsInt() > 0;
        }

        /// <summary>
        /// If the stack is a backpack, this returns the amount of slots it has
        /// </summary>
        /// <param name="itemstack"></param>
        /// <returns></returns>
        public static int QuantityBackPackSlots(IItemStack itemstack)
        {
            if (itemstack == null || itemstack.Collectible.Attributes == null) return 0;
            return itemstack.Collectible.Attributes["backpack"]["quantitySlots"].AsInt();
        }

        /// <summary>
        /// Returns a new assetlocation with an equal domain and the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public AssetLocation CodeWithPath(string path)
        {
            return Code.CopyWithPath(path);
        }

        /// <summary>
        /// Removes componentsToRemove parts from the blocks code end by splitting it up at every occurence of a dash ('-'). Right to left.
        /// </summary>
        /// <param name="componentsToRemove"></param>
        /// <returns></returns>
        public string CodeWithoutParts(int componentsToRemove)
        {
            int i = Code.Path.Length;
            int index = 0;
            while (i-- > 0 && componentsToRemove > 0)
            {
                if (Code.Path[i] == '-')
                {
                    index = i;
                    componentsToRemove--;
                }
            }

            return Code.Path.Substring(0, index);
        }


        /// <summary>
        /// Removes componentsToRemove parts from the blocks code beginning by splitting it up at every occurence of a dash ('-'). Left to Right
        /// </summary>
        /// <param name="componentsToRemove"></param>
        /// <returns></returns>
        public string CodeEndWithoutParts(int componentsToRemove)
        {
            int i = 0;
            int index = 0;
            while (i++ < Code.Path.Length && componentsToRemove > 0)
            {
                if (Code.Path[i] == '-')
                {
                    index = i + 1;
                    componentsToRemove--;
                }
            }

            return Code.Path.Substring(index, Code.Path.Length - index);
        }


        /// <summary>
        /// Replaces the last parts from the blocks code and replaces it with components by splitting it up at every occurence of a dash ('-')
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        public AssetLocation CodeWithParts(params string[] components)
        {
            if (Code == null) return null;

            AssetLocation newCode = Code.CopyWithPath(CodeWithoutParts(components.Length));
            for (int i = 0; i < components.Length; i++) newCode.Path += "-" + components[i];
            return newCode;
        }

        /// <summary>
        /// Replaces the last parts from the blocks code and replaces it with components by splitting it up at every occurence of a dash ('-')
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        public AssetLocation CodeWithPart(string part, int atPosition = 0)
        {
            if (Code == null) return null;

            AssetLocation newCode = Code.Clone();
            string[] parts = newCode.Path.Split('-');
            parts[atPosition] = part;
            newCode.Path = String.Join("-", parts);

            return newCode;
        }


        /// <summary>
        /// Returns the n-th code part in inverse order. If the code contains no dash ('-') the whole code is returned. Returns null if posFromRight is too high.
        /// </summary>
        /// <param name="posFromRight"></param>
        /// <returns></returns>
        public string LastCodePart(int posFromRight = 0)
        {
            if (Code == null) return null;
            if (posFromRight == 0 && !Code.Path.Contains('-')) return Code.Path;

            string[] parts = Code.Path.Split('-');
            return parts.Length - 1 - posFromRight >= 0 ? parts[parts.Length - 1 - posFromRight] : null;
        }

        /// <summary>
        /// Returns the n-th code part. If the code contains no dash ('-') the whole code is returned. Returns null if posFromLeft is too high.
        /// </summary>
        /// <param name="posFromLeft"></param>
        /// <returns></returns>
        public string FirstCodePart(int posFromLeft = 0)
        {
            if (Code == null) return null;
            if (posFromLeft == 0 && !Code.Path.Contains('-')) return Code.Path;

            string[] parts = Code.Path.Split('-');
            return posFromLeft <= parts.Length - 1 ? parts[posFromLeft] : null;
        }


        /// <summary>
        /// Should return true if given stacks are equal, ignoring their stack size.
        /// </summary>
        /// <param name="thisStack"></param>
        /// <param name="otherStack"></param>
        /// <param name="ignoreAttributeSubTrees"></param>
        /// <returns></returns>
        public virtual bool Equals(ItemStack thisStack, ItemStack otherStack, params string[] ignoreAttributeSubTrees)
        {
            return 
                thisStack.Class == otherStack.Class &&
                thisStack.Id == otherStack.Id &&
                thisStack.Attributes.Equals(otherStack.Attributes, ignoreAttributeSubTrees)
            ;
        }

        /// <summary>
        /// Should return true if thisStack is a satisfactory replacement of otherStack. It's bascially an Equals() test, but it ignores any additional attributes that exist in otherStack
        /// </summary>
        /// <param name="thisStack"></param>
        /// <param name="otherStack"></param>
        /// <returns></returns>
        public virtual bool Satisfies(ItemStack thisStack, ItemStack otherStack)
        {
            return
                thisStack.Class == otherStack.Class &&
                thisStack.Id == otherStack.Id &&
                thisStack.Attributes.IsSubSetOf(otherStack.Attributes)
            ;
        }

        /// <summary>
        /// This method is for example called by chests when they are being exported as part of a block schematic. Has to store all the currents world id mappings so it can be correctly imported again
        /// </summary>
        /// <param name="blockIdMapping"></param>
        /// <param name="itemIdMapping"></param>
        public virtual void OnStoreCollectibleMappings(IWorldAccessor world, ItemSlot inSlot, Dictionary<int, AssetLocation> blockIdMapping, Dictionary<int, AssetLocation> itemIdMapping)
        {
            if (this is Item)
            {
                itemIdMapping[Id] = Code;
            }
            else
            {
                blockIdMapping[Id] = Code;
            }
        }

        /// <summary>
        /// Returns true if the block has given behavior
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool HasBehavior(Type type, bool withInheritance)
        {
            return false;
        }

        /// <summary>
        /// Returns true if the block has given behavior
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasBehavior(Type type)
        {
            return HasBehavior(type, false);
        }

        /// <summary>
        /// Returns true if the block has given behavior
        /// </summary>
        /// <param name="type"></param>
        /// <param name="classRegistry"></param>
        /// <returns></returns>
        public virtual bool HasBehavior(string type, IClassRegistryAPI classRegistry)
        {
            return false;
        }


        /// <summary>
        /// Returns true if any given wildcard matches the blocks code. E.g. water-* will match all water blocks
        /// </summary>
        /// <param name="wildcards"></param>
        /// <returns></returns>
        public bool WildCardMatch(AssetLocation[] wildcards)
        {
            foreach (AssetLocation wildcard in wildcards)
            {
                if (WildCardMatch(wildcard)) return true;
            }

            return false;
        }
        
        /// <summary>
        /// Returns true if given wildcard matches the blocks code. E.g. water-* will match all water blocks
        /// </summary>
        /// <param name="wildCard"></param>
        /// <returns></returns>
        public bool WildCardMatch(AssetLocation wildCard)
        {
            if (wildCard == Code) return true;

            if (Code == null || wildCard.Domain != Code.Domain) return false;

            string pattern = Regex.Escape(wildCard.Path).Replace(@"\*", @"(.*)");

            return Regex.IsMatch(Code.Path, @"^" + pattern + @"$");
        }
    }
}