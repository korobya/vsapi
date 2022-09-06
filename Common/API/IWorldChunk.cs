﻿using System.Collections.Generic;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace Vintagestory.API.Common
{
    public interface IChunkBlocks
    {
        /// <summary>
        /// Retrieves the first solid block, if that one is empty, retrieves the first fluid block
        /// </summary>
        /// <param name="index3d"></param>
        /// <returns></returns>
        int this[int index3d] { get; set; }

        int Length { get; }

        void ClearBlocks();
        /// <summary>
        /// Same as ClearBlocks but initialises the chunkdata palette, so that SetBlockUnsafe can be used  (useful in worldgen)
        /// </summary>
        void ClearBlocksAndPrepare();
        void SetBlockBulk(int index3d, int lenX, int lenZ, int value);
        /// <summary>
        /// Not threadsafe, used only in cases where we know that the chunk already has a palette (e.g. in worldgen when replacing rock with other blocks)
        /// </summary>
        /// <param name="index3d"></param>
        /// <param name="value"></param>
        void SetBlockUnsafe(int index3d, int value);
        void SetBlockAir(int index3d);
        /// <summary>
        /// Used to place blocks into the fluid layer instead of the solid blocks layer; calling code must do this
        /// </summary>
        /// <param name="index3d"></param>
        /// <param name="value"></param>
        void SetFluid(int index3d, int value);
        int GetBlockId(int index3d, int layer);
        int GetFluid(int index3d);
        /// <summary>
        /// Like get (i.e. this[]) but not threadsafe - only for use where setting and getting is guaranteed to be all on the same thread (e.g. during worldgen)
        /// </summary>
        /// <param name="index3d"></param>
        /// <param name="value"></param>
        int GetBlockIdUnsafe(int index3d);
    }

    public interface IChunkLight
    {
        int GetSunlight(int index3d);
        void SetSunlight(int index3d, int sunlevel);
        void SetSunlight_Buffered(int index3d, int sunlevel);
        int GetBlocklight(int index3d);
        void SetBlocklight(int index3d, int lightlevel);
        void SetBlocklight_Buffered(int index3d, int lightlevel);
        void ClearWithSunlight(ushort sunLight);
        void FloodWithSunlight(ushort sunLight);
        void ClearLight();
        void ClearAllSunlight();
    }

    public interface IClientChunk : IWorldChunk
    {
        /// <summary>
        /// True if fully initialized
        /// </summary>
        bool LoadedFromServer { get; }
    }


    public interface IWorldChunk
    {
        bool Empty { get; set; }
        /// <summary>
        /// Holds a reference to the current map data of this chunk column
        /// </summary>
        IMapChunk MapChunk { get; }

        /// <summary>
        /// Holds all the blockids for each coordinate, access via index: (y * chunksize + z) * chunksize + x
        /// </summary>
        IChunkBlocks Data { get; }

		/// <summary>
        /// Use <see cref="Data"/> instead
        /// </summary>
		IChunkBlocks Blocks { get; }

        /// <summary>
        /// Holds all the lighting data for each coordinate, access via index: (y * chunksize + z) * chunksize + x
        /// </summary>
        IChunkLight Lighting { get; }

        /// <summary>
        /// Faster (non-blocking) access to blocks at the cost of sometimes returning 0 instead of the real block. Use <see cref="Data"/> if you need reliable block access. Also should only be used for reading. Currently used for the particle system.
        /// </summary>
        IChunkBlocks MaybeBlocks { get; }

        /// <summary>
        /// An array holding all Entities currently residing in this chunk. This array may be larger than the amount of entities in the chunk. 
        /// </summary>
        Entity[] Entities { get; }

        /// <summary>
        /// Actual count of entities in this chunk
        /// </summary>
        int EntitiesCount { get; }

        /// <summary>
        /// An array holding block Entities currently residing in this chunk. This array may be larger than the amount of block entities in the chunk. 
        /// </summary>
        Dictionary<BlockPos, BlockEntity> BlockEntities { get; set; }

        /// <summary>
        /// Blockdata and Light might be compressed, always call this method if you want to access these
        /// </summary>
        void Unpack();

        /// <summary>
        /// Like Unpack(), except it must be used readonly: the calling code promises not to write any changes to this chunk's blocks or lighting
        /// </summary>
        bool Unpack_ReadOnly();

        /// <summary>
        /// Like Unpack_ReadOnly(), except it actually reads and returns the block ID at index<br/>
        /// (Returns 0 if the chunk was disposed)
        /// </summary>
        int UnpackAndReadBlock(int index, int layer);

        /// <summary>
        /// Like Unpack_ReadOnly(), except it actually reads and returns the Light at index<br/>
        /// (Returns 0 if the chunk was disposed)
        /// </summary>
        ushort Unpack_AndReadLight(int index);

        /// <summary>
        /// A version of Unpack_AndReadLight which also returns the lightSat<br/>
        /// (Returns 0 if the chunk was disposed)
        /// </summary>
        ushort Unpack_AndReadLight(int index, out int lightSat);

        /// <summary>
        /// Marks this chunk as modified. If called on server side it will be stored to disk on the next autosave or during shutdown, if called on client not much happens (but it will be preserved from packing for next ~8 seconds)
        /// </summary>
        void MarkModified();

        /// <summary>
        /// Marks this chunk as recently accessed. This will prevent the chunk from getting compressed by the in-memory chunk compression algorithm
        /// </summary>
        void MarkFresh();

        /// <summary>
        /// Returns a list of a in-chunk indexed positions of all light sources in this chunk
        /// </summary>
        HashSet<int> LightPositions { get; set; }
        
        /// <summary>
        /// Whether this chunk got unloaded
        /// </summary>
        bool Disposed { get; }

        /// <summary>
        /// Adds an entity to the chunk.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        void AddEntity(Entity entity);

        /// <summary>
        /// Removes an entity from the chunk.
        /// </summary>
        /// <param name="entityId">the ID for the entity</param>
        /// <returns>Whether or not the entity was removed.</returns>
        bool RemoveEntity(long entityId);


        /// <summary>
        /// Allows setting of arbitrary, permanantly stored moddata of this chunk. When set on the server before the chunk is sent to the client, the data will also be sent to the client.
        /// When set on the client the data is discarded once the chunk gets unloaded
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        void SetModdata(string key, byte[] data);

        /// <summary>
        /// Removes the permanently stored data. 
        /// </summary>
        /// <param name="key"></param>
        void RemoveModdata(string key);

        /// <summary>
        /// Retrieve arbitrary, permantly stored mod data
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        byte[] GetModdata(string key);

        /// <summary>
        /// Allows setting of arbitrary, permanantly stored moddata of this chunk. When set on the server before the chunk is sent to the client, the data will also be sent to the client.
        /// When set on the client the data is discarded once the chunk gets unloaded
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>

        void SetModdata<T>(string key, T data);
        /// <summary>
        /// Retrieve arbitrary, permantly stored mod data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T GetModdata<T>(string key, T defaultValue = default(T));


        
        /// <summary>
        /// Retrieve a block from this chunk ignoring ice/water layer, performs Unpack() and a modulo operation on the position arg to get a local position in the 0..chunksize range (it's your job to pick out the right chunk before calling this method)
        /// </summary>
        /// <param name="world"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        Block GetLocalBlockAtBlockPos(IWorldAccessor world, BlockPos position);

        Block GetLocalBlockAtBlockPos(IWorldAccessor world, int posX, int posY, int posZ, int layer);

        /// <summary>
        /// Retrieve a block entity from this chunk
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        BlockEntity GetLocalBlockEntityAtBlockPos(BlockPos pos);

        /// <summary>
        /// Sets a decor block to the side of an existing block. Use air block (id 0) to remove a decor.<br/>
        /// </summary>
        /// <param name="blockAccessor"></param>
        /// <param name="pos"></param>
        /// <param name="onFace"></param>
        /// <param name="block"></param>
        /// <returns>False if there already exists a block in this position and facing</returns>
        bool SetDecor(IBlockAccessor blockAccessor, Block block, BlockPos pos, BlockFacing onFace);

        /// <summary>
        /// Sets a decor block to a specific sub-position on the side of an existing block. Use air block (id 0) to remove a decor.<br/>
        /// </summary>
        /// <param name="blockAccessor"></param>
        /// <param name="pos"></param>
        /// <param name="onFace"></param>
        /// <param name="block"></param>
        /// <returns>False if there already exists a block in this position and facing</returns>
        bool SetDecor(IBlockAccessor blockAccessor, Block block, BlockPos pos, int faceAndSubposition);


        /// <summary>
        /// If allowed by a player action, removes all decors at given position and calls OnBrokenAsDecor() on all selected decors and drops the items that are returned from Block.GetDrops()
        /// </summary>
        /// <param name="world"></param>
        /// <param name="pos"></param>
        /// <param name="side">If null, all the decor blocks on all sides are removed</param>
        /// <param name="faceAndSubposition">If not null breaks only this part of the decor for give face. Requires side to be set.</param>
        bool BreakDecor(IWorldAccessor world, BlockPos pos, BlockFacing side = null, int? faceAndSubposition = null);


        /// <summary>
        /// Removes a decor block from given position, saves a few cpu cycles by not calculating index3d
        /// </summary>
        /// <param name="world"></param>
        /// <param name="pos"></param>
        /// <param name="index3d"></param>
        void BreakAllDecorFast(IWorldAccessor world, BlockPos pos, int index3d);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blockAccessor"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        Block[] GetDecors(IBlockAccessor blockAccessor, BlockPos pos);

        Block GetDecor(IBlockAccessor blockAccessor, BlockPos pos, int faceAndSubposition);

        /// <summary>
        /// Set entire Decors for a chunk - used in Server->Client updates
        /// </summary>
        /// <param name="newDecors"></param>
        void SetDecors(Dictionary<int, Block> newDecors);

        /// <summary>
        /// Adds extra selection boxes in case a decor block is attached at given position
        /// </summary>
        /// <param name="blockAccessor"></param>
        /// <param name="pos"></param>
        /// <param name="orig"></param>
        /// <returns></returns>
        Cuboidf[] AdjustSelectionBoxForDecor(IBlockAccessor blockAccessor, BlockPos pos, Cuboidf[] orig);

        /// <summary>
        /// Only to be implemented client side
        /// </summary>
        void FinishLightDoubleBuffering();

        /// <summary>
        /// Returns the higher light absorption between solids and fluids block layers
        /// </summary>
        /// <param name="index3d"></param>
        /// <param name="blockPos"></param>
        /// <param name="blockTypes"></param>
        /// <returns></returns>
        int GetLightAbsorptionAt(int index3d, BlockPos blockPos, IList<Block> blockTypes);
    }
}
