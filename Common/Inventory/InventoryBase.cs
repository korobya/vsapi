﻿using System;
using System.Collections.Generic;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Vintagestory.API.Common
{
    public delegate void OnInventoryOpened(IPlayer player);
    public delegate void OnInventoryClosed(IPlayer player);

    public abstract class InventoryBase : IInventory
    {
        /// <summary>
        /// The world in which the inventory is operating in. Gives inventories access to block types, item types and the ability to drop items on the ground.
        /// </summary>
        public ICoreAPI Api;

        /// <summary>
        /// Is this inventory generally better suited to hold items? (e.g. set to 3 for armor in armor inventory, 2 for any item in hotbar inventory, 1 for any item in normal inventory)
        /// </summary>
        protected float baseWeight = 0;

        protected string className;
        protected string instanceID;

        public long lastChangedSinceServerStart;
        public HashSet<string> openedByPlayerGUIds;

        public IInventoryNetworkUtil InvNetworkUtil;

        /// <summary>
        /// Slots that have been recently modified. This list is used on the server to update the clients (then cleared) and on the client to redraw itemstacks in guis (then cleared)
        /// </summary>
        public HashSet<int> dirtySlots = new HashSet<int>();

        public string InventoryID { get { return className + "-" + instanceID; } }
        public string ClassName { get { return className; } }
        public long LastChanged { get { return lastChangedSinceServerStart; } }

        public abstract int QuantitySlots { get; }
        
        public virtual bool IsDirty { get { return dirtySlots.Count > 0; } }

        public HashSet<int> DirtySlots { get { return dirtySlots; } }

        public virtual bool TakeLocked { get; set; }
        public virtual bool PutLocked { get; set; }

        public event API.Common.Action<int> SlotModified;
        public event API.Common.Action<int> SlotNotified;
        public event OnInventoryOpened OnInventoryOpened;
        public event OnInventoryClosed OnInventoryClosed;

        public InventoryBase(string className, string instanceID, ICoreAPI api)
        {
            openedByPlayerGUIds = new HashSet<string>();

            this.instanceID = instanceID;
            this.className = className;
            this.Api = api;
            if (api != null)
            {
                InvNetworkUtil = api.ClassRegistry.CreateInvNetworkUtil(this, api);
            }
        }

        public InventoryBase(string inventoryID, ICoreAPI api)
        {
            openedByPlayerGUIds = new HashSet<string>();

            if (inventoryID != null)
            {
                string[] elems = inventoryID.Split(new char[] { '-' }, 2);
                className = elems[0];
                instanceID = elems[1];
            }

            this.Api = api;
            if (api != null)
            {
                InvNetworkUtil = api.ClassRegistry.CreateInvNetworkUtil(this, api);
            }
        }

        /*public void AddSlotModifiedListener(API.Common.Action<int> handler)
        {
            slotModifiedListeners.Add(handler);
        }*/

        public virtual void LateInitialize(string inventoryID, ICoreAPI api)
        {
            this.Api = api;
            string[] elems = inventoryID.Split(new char[] { '-' }, 2);
            className = elems[0];
            instanceID = elems[1];

            if (InvNetworkUtil == null)
            {
                InvNetworkUtil = api.ClassRegistry.CreateInvNetworkUtil(this, api);
            } else
            {
                InvNetworkUtil.Api = api;
            }
            
        }

        public virtual void AfterBlocksLoaded(IWorldAccessor world)
        {
            ResolveBlocksOrItems();
        }



        public abstract ItemSlot GetSlot(int slotId);

        ItemSlot IInventory.GetSlot(int slotId)
        {
            return GetSlot(slotId);
        }

        public virtual void ResolveBlocksOrItems()
        {
            for (int i = 0; i < QuantitySlots; i++)
            {
                ItemSlot slot = GetSlot(i);
                if (slot.Itemstack != null)
                {
                    if (!slot.Itemstack.ResolveBlockOrItem(Api.World))
                    {
                        slot.Itemstack = null;
                    }
                }
            }
        }

        public virtual int GetSlotId(IItemSlot slot)
        {
            for (int i = 0; i < QuantitySlots; i++)
            {
                if (GetSlot(i) == slot)
                {
                    return i;
                }
            }

            return -1;
        }


        public virtual WeightedSlot GetBestSuitedSlot(IPlayer actingPlayer, ItemSlot sourceSlot, List<IItemSlot> skipSlots = null)
        {
            WeightedSlot bestWSlot = new WeightedSlot();

            // Useless to put the item into the same inventory
            if (sourceSlot.Inventory == this) return bestWSlot;

            // 1. Prefer already filled slots
            for (int i = 0; i < QuantitySlots; i++)
            {
                ItemSlot slot = GetSlot(i);
                if (skipSlots.Contains(slot)) continue;

                if (slot.Itemstack != null && slot.CanTakeFrom(sourceSlot))
                {
                    float curWeight = GetSuitability(actingPlayer, sourceSlot, slot, true);

                    if (bestWSlot.slot == null || bestWSlot.weight < curWeight)
                    {
                        bestWSlot.slot = slot;
                        bestWSlot.weight = curWeight;
                    }
                }
            }

            // 2. Otherwise use empty slots
            for (int i = 0; i < QuantitySlots; i++)
            {
                ItemSlot slot = GetSlot(i);
                if (skipSlots.Contains(slot)) continue;

                if (slot.Itemstack == null && slot.CanTakeFrom(sourceSlot))
                {
                    float curWeight = GetSuitability(actingPlayer, sourceSlot, slot, false);

                    if (bestWSlot.slot == null || bestWSlot.weight < curWeight)
                    {
                        bestWSlot.slot = slot;
                        bestWSlot.weight = curWeight;
                    }
                }
            }

            return bestWSlot;
        }

        /// <summary>
        /// How well a stack fits into this inventory. 
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="targetSlot"></param>
        /// <param name="isMerge"></param>
        /// <returns></returns>
        public virtual float GetSuitability(IPlayer actingPlayer, ItemSlot sourceSlot, ItemSlot targetSlot, bool isMerge)
        {
            return isMerge ? (baseWeight + 3) : (baseWeight + 1);
        }


        public object TryFlipItems(int targetSlotId, IItemSlot itemSlot)
        {
            IItemSlot targetSlot = GetSlot(targetSlotId);
            if (targetSlot != null && targetSlot.TryFlipWith(itemSlot))
            {
                return InvNetworkUtil.GetFlipSlotsPacket(itemSlot.Inventory, itemSlot.Inventory.GetSlotId(itemSlot), targetSlotId);
            }
            return null;
        }


        public virtual bool CanPlayerAccess(IPlayer player, EntityPos position)
        {
            return true;
        }

        public virtual bool CanPlayerModify(IPlayer player, EntityPos position)
        {
            return CanPlayerAccess(player, position) && HasOpened(player);
        }


        public virtual void OnSearchTerm(string text) { }

        /// <summary>
        /// Call when a player has clicked on this slot. The source slot is the mouse cursor slot. This handles the logic of either taking, putting or exchanging items.
        /// </summary>
        /// <param name="slotId"></param>
        /// <param name="sourceSlot"></param>
        /// <param name="op"></param>
        /// <returns>The appropriate packet needed to reflect the changes on the opposing side</returns>
        public virtual object ActivateSlot(int slotId, IItemSlot sourceSlot, ref ItemStackMoveOperation op)
        {
            object packet = InvNetworkUtil.GetActivateSlotPacket(slotId, op);

            GetSlot(slotId).ActivateSlot((ItemSlot)sourceSlot, ref op);

            return packet;
        }


        /// <summary>
        /// Called when one of the containing slots has been modified
        /// </summary>
        /// <param name="slot"></param>
        public virtual void OnItemSlotModified(IItemSlot slot)
        {

        }


        /// <summary>
        /// Called when one of the containing slots has been modified
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="extractedStack">If non null the itemstack that was taken out</param>
        public virtual void DidModifyItemSlot(IItemSlot slot, IItemStack extractedStack = null)
        {
            int slotId = GetSlotId(slot);
            MarkSlotDirty(slotId);
            OnItemSlotModified(slot);
            SlotModified?.Invoke(slotId);
        }

        /// <summary>
        /// Called when one of the containing slot was notified via NotifySlot
        /// </summary>
        /// <param name="slotId"></param>
        public virtual void DidNotifySlot(int slotId)
        {
            ItemSlot slot = GetSlot(slotId);
            if (slot == null || slot.Inventory != this) return;

            SlotNotified?.Invoke(slotId);
        }



        /// <summary>
        /// Called when the game is loaded or loaded from server
        /// </summary>
        /// <param name="tree"></param>
        public abstract void FromTreeAttributes(ITreeAttribute tree);

        /// <summary>
        /// Called when the game is saved or sent to client
        /// </summary>
        /// <returns></returns>
        public abstract void ToTreeAttributes(ITreeAttribute tree);


        public virtual bool TryFlipItemStack(IPlayer owningPlayer, string[] invIds, int[] slotIds, long[] lastChanged)
        {
            // 0 = source slot
            // 1 = target slot
            IItemSlot[] slots = getSlotsIfExists(owningPlayer, invIds, slotIds);

            if (slots[0] == null || slots[1] == null) return false;

            InventoryBase targetInv = (InventoryBase)owningPlayer.InventoryManager.GetInventory(invIds[1]);

            // 4. Try to move the item stack
            return targetInv.TryFlipItems(slotIds[1], slots[0]) != null;
        }


        public virtual bool TryMoveItemStack(IPlayer player, string[] invIds, int[] slotIds, ref ItemStackMoveOperation op)
        {
            // 0 = source slot
            // 1 = target slot
            IItemSlot[] slots = getSlotsIfExists(player, invIds, slotIds);

            if (slots[0] == null || slots[1] == null) return false;

            InventoryBase targetInv = (InventoryBase)player.InventoryManager.GetInventory(invIds[1]);

            // 4. Try to move the item stack
            slots[0].TryPutInto(slots[1], ref op);

            return op.MovedQuantity == op.RequestedQuantity;
        }

        public virtual IItemSlot[] getSlotsIfExists(IPlayer player, string[] invIds, int[] slotIds)
        {
            IItemSlot[] slots = new IItemSlot[2];
           
            // 1. Both inventories must exist and be modifiable
            InventoryBase sourceInv = (InventoryBase)player.InventoryManager.GetInventory(invIds[0]);
            InventoryBase targetInv = (InventoryBase)player.InventoryManager.GetInventory(invIds[1]);

            if (sourceInv == null || targetInv == null) return slots;

            if (!sourceInv.CanPlayerModify(player, player.Entity.Pos) || !targetInv.CanPlayerModify(player, player.Entity.Pos))
            {
                return slots;
            }

            // 3. Source and Dest slot must exist
            slots[0] = sourceInv.GetSlot(slotIds[0]);
            slots[1] = targetInv.GetSlot(slotIds[1]);

            return slots;
        }


        public ItemSlot[] SlotsFromTreeAttributes(ITreeAttribute tree, ItemSlot[] slots = null, List<ItemSlot> modifiedSlots = null)
        {
            if (slots == null)
            {
                slots = new ItemSlot[tree.GetInt("qslots")];
                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i] = NewSlot(i);
                }
            }

            for (int slotId = 0; slotId < slots.Length; slotId++)
            {
                ItemStack newstack = tree.GetTreeAttribute("slots")?.GetItemstack("" + slotId);
                ItemStack oldstack = slots[slotId].Itemstack;

                if (Api?.World == null)
                {
                    slots[slotId].Itemstack = newstack;
                    continue;
                }

                newstack?.ResolveBlockOrItem(Api.World);

                bool didModify =
                    (newstack != null && !newstack.Equals(oldstack)) ||
                    (oldstack != null && !oldstack.Equals(newstack))
                ;

                slots[slotId].Itemstack = newstack;

                if (didModify && modifiedSlots != null)
                {
                    modifiedSlots.Add(slots[slotId]);
                }
            }

            

            return slots;
        }


        public void SlotsToTreeAttributes(ItemSlot[] slots, ITreeAttribute tree)
        {
            tree.SetInt("qslots", slots.Length);

            TreeAttribute slotsTree = new TreeAttribute();

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Itemstack == null) continue;
                slotsTree.SetItemstack(i + "", slots[i].Itemstack.Clone());
            }

            tree["slots"] = slotsTree;
        }
        


        public ItemSlot[] GenEmptySlots(int quantity)
        {
            ItemSlot[] slots = new ItemSlot[quantity];
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = NewSlot(i);
            }
            return slots;
        }

        protected virtual ItemSlot NewSlot(int i)
        {
            return new ItemSlot(this);
        }

        public virtual void MarkSlotDirty(int slotId)
        {
            if (slotId < 0) throw new Exception("Negative slotid?!");
            dirtySlots.Add(slotId);

            /*if (this is InventorySmelting)
            {
                Console.WriteLine("slot {0} dirty", slotId);
                Console.WriteLine(Environment.StackTrace);
            }*/
            
        }


        public virtual void DiscardAll()
        {
            for (int i = 0; i < QuantitySlots; i++)
            {
                if (GetSlot(i).Itemstack != null)
                {
                    dirtySlots.Add(i);
                }

                GetSlot(i).Itemstack = null;
            }
        }

        public virtual void DropSlots(Vec3d pos, params int[] slotsIds)
        {
            foreach (int slotId in slotsIds)
            {
                if (GetSlot(slotId).Itemstack != null)
                {
                    if (slotId < 0) throw new Exception("Negative slotid?!");
                    dirtySlots.Add(slotId);
                    Api.World.SpawnItemEntity(GetSlot(slotId).Itemstack, pos);
                    GetSlot(slotId).Itemstack = null;
                }
            }
        }

        public virtual void DropAll(Vec3d pos)
        {
            for (int i = 0; i < QuantitySlots; i++)
            {
                if (GetSlot(i).Itemstack != null)
                {
                    dirtySlots.Add(i);
                    Api.World.SpawnItemEntity(GetSlot(i).Itemstack, pos);
                    GetSlot(i).Itemstack = null;
                }
            }
        }
    
        public virtual void OnOwningEntityDeath(Vec3d pos)
        {
            DropAll(pos);
        }


        public virtual object Open(IPlayer player)
        {
            object packet = InvNetworkUtil.DidOpen(player);
            openedByPlayerGUIds.Add(player.PlayerUID);

            OnInventoryOpened?.Invoke(player);
            return packet;
        }

        public virtual object Close(IPlayer player)
        {
            object packet = InvNetworkUtil.DidClose(player);
            openedByPlayerGUIds.Remove(player.PlayerUID);

            OnInventoryClosed?.Invoke(player);

            return packet;
        }

        public virtual bool HasOpened(IPlayer player)
        {
            return openedByPlayerGUIds.Contains(player.PlayerUID);
        }

    }
}