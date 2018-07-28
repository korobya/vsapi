﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Common
{
    /// <summary>
    /// Abstract class used for BlockVoxelParticles and ItemVoxelParticles
    /// </summary>
    public abstract class CollectibleParticleProperties : IParticlePropertiesProvider
    {
        public Random rand = new Random();

        public virtual bool DieInLiquid() { return false; }
        public virtual bool DieInAir() { return false; }
        public abstract float GetQuantity();
        public abstract Vec3d GetPos();

        public abstract Vec3f GetVelocity(Vec3d pos);
        public abstract byte[] GetRgbaColor();
        public abstract byte GetGlowLevel();
        public abstract EnumParticleModel ParticleModel();

        public ICoreAPI api;

        public virtual bool SelfPropelled()
        {
            return false;
        }

        public virtual bool TerrainCollision() { return true; }

        public virtual float GetSize()
        {
            return 1f;
        }

        public virtual float GetGravityEffect()
        {
            return 1f;
        }

        public virtual float GetLifeLength()
        {
            return 1.5f;
        }

        public virtual bool UseLighting()
        {
            return true;
        }



        public static byte[] RandomItemPixel(ICoreClientAPI api, Item item, BlockFacing facing, BlockPos pos)
        {
            int textureSubId = item.FirstTexture.Baked.TextureSubId;
            int color = ColorUtil.ReverseColorBytes(api.GetRandomItemPixel(item.ItemId, textureSubId));
            color = (Math.Min(255, (color >> 24) + 50) << 24) | (color & 0x00ffffff);
            return ColorUtil.ToBGRABytes(color);
        }


        public static byte[] RandomBlockPixel(ICoreClientAPI api, Block block, BlockFacing facing, BlockPos pos)
        {
            int tintIndex = block.TintIndex;

            int textureSubId = block.TextureSubIdForRandomBlockPixel(api.World, pos, facing, ref tintIndex);

            int color = ColorUtil.ReverseColorBytes(api.GetRandomBlockPixel(block.BlockId, textureSubId));

            // TODO: FIXME
            // We probably need to pre-generate like a list of 50 random colors per block face 
            // not by probing the first texture but by probing all elmenent faces that are facing that block face
            // Has to include the info if that pixel has to be biome tinted or not and then tinted as below
            // Result:
            // - No more white block pixels
            // - Properly biome tinted
            if (tintIndex > 0)
            {
                color = api.ApplyColorTint(tintIndex, color, pos.X, pos.Y, pos.Z);
            }

            color = (Math.Min(255, (color >> 24) + 50) << 24) | (color & 0x00ffffff);

            return ColorUtil.ToBGRABytes(color);
        }

        public Vec3d RandomBlockPos(IBlockAccessor blockAccess, BlockPos pos, Block block, BlockFacing facing = null)
        {
            if (facing == null)
            {
                Cuboidf[] selectionBoxes = block.GetSelectionBoxes(blockAccess, pos);
                Cuboidf box = (selectionBoxes != null && selectionBoxes.Length > 0) ? selectionBoxes[0] : Block.DefaultCollisionBox;

                return new Vec3d(
                    pos.X + box.X1 + rand.NextDouble() * (box.X2 - box.X1),
                    pos.Y + box.Y1 + rand.NextDouble() * (box.Y2 - box.Y1),
                    pos.Z + box.Z1 + rand.NextDouble() * (box.Z2 - box.Z1)
                );
            }
            else
            {
                Vec3i face = facing.Normali;

                Cuboidf[] boxes = block.GetCollisionBoxes(blockAccess, pos);
                if (boxes == null || boxes.Length == 0) boxes = block.GetSelectionBoxes(blockAccess, pos);
                

                bool haveCollisionBox = boxes != null && boxes.Length > 0;

                Vec3d basepos = new Vec3d(
                    pos.X + 0.5f + face.X / 1.95f + (haveCollisionBox && facing.Axis == EnumAxis.X ? (face.X > 0 ? boxes[0].X2 - 1 : boxes[0].X1) : 0),
                    pos.Y + 0.5f + face.Y / 1.95f + (haveCollisionBox && facing.Axis == EnumAxis.Y ? (face.Y > 0 ? boxes[0].Y2 - 1 : boxes[0].Y1) : 0),
                    pos.Z + 0.5f + face.Z / 1.95f + (haveCollisionBox && facing.Axis == EnumAxis.Z ? (face.Z > 0 ? boxes[0].Z2 - 1 : boxes[0].Z1) : 0)
                );

                Vec3d posVariance = new Vec3d(
                    1f * (1 - face.X),
                    1f * (1 - face.Y),
                    1f * (1 - face.Z)
                );

                return new Vec3d(
                    basepos.X + (rand.NextDouble() - 0.5) * posVariance.X,
                    basepos.Y + (rand.NextDouble() - 0.5) * posVariance.Y,
                    basepos.Z + (rand.NextDouble() - 0.5) * posVariance.Z
                );
            }
        }

        public EvolvingNatFloat GetOpacityEvolve()
        {
            return null;
        }

        public EvolvingNatFloat GetRedEvolve() { return null; }
        public EvolvingNatFloat GetGreenEvolve() { return null; }
        public EvolvingNatFloat GetBlueEvolve() { return null; }


        public EvolvingNatFloat GetSizeEvolve()
        {
            return null;
        }

        public Block ColorByBlock()
        {
            return null;
        }

        public virtual void ToBytes(BinaryWriter writer)
        {

        }

        public virtual void FromBytes(BinaryReader reader, IWorldAccessor resolver)
        {

        }

        public void BeginParticle() { }

        public virtual EvolvingNatFloat[] GetVelocityEvolve()
        {
            return null;
        }

        public virtual IParticlePropertiesProvider[] GetSecondaryParticles()
        {
            return null;
        }

        public virtual float GetSecondarySpawnInterval()
        {
            return 0.0f;
        }

        public virtual void PrepareForSecondarySpawn(IParticleInstance particleInstance)
        {
        }

        public virtual void Init(ICoreAPI api)
        {
            this.api = api;
        }
    }
}