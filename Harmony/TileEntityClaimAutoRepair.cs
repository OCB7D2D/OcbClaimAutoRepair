using DMT;
using Audio;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class TileEntityClaimAutoRepair : TileEntitySecureLootContainer
{

	// how much damage repair per tick
	// Gets multiplied by Time.deltaTime!
	public const float repairSpeed = 750f;

	// The acquired block to be repaired
	public BlockValue repairBlock;

	// The position of the block being repaired
	// To check if block is still the same in the world
	public Vector3i repairPosition;

	// How much damage has already been repaired
	// To check when the block is fully repaired
	public float repairDamage;

	// Percentage of damage on acquired block
	// To calculate amount of items needed for repair
	public float damagePerc;

	// Flag only for server side code
	public bool isAccessed;

	// Copied from LandClaim code
	public Transform BoundsHelper;

	private bool isOn;

	public bool IsOn
	{
		get => this.isOn;
		set
		{
			if (this.isOn != value) {
				this.isOn = value;
				repairBlock = BlockValue.Air;
				repairPosition = ToWorldPos();
				damagePerc = 0.0f;
				repairDamage = 0.0f;
				ResetBoundHelper();
				SetModified();
			}
		}
	}

	public TileEntityClaimAutoRepair(Chunk _chunk)
		: base(_chunk)
	{
		isOn = false;
		isAccessed = false;
		repairBlock = BlockValue.Air;
		repairDamage = 0.0f;
		damagePerc = 0.0f;
	}

	public override TileEntityType GetTileEntityType() => (TileEntityType)242;

	public void ReduceItemCount(Block.SItemNameCount sitem, int count)
	{
		int having = 0;
		for (int i = 0; i < items.Length; i++)
		{
			ItemStack stack = items[i];
			if (stack.IsEmpty()) continue;
			// ToDo: how expensive is this call for `GetItem(string)`?
			if (stack.itemValue.type == ItemClass.GetItem(sitem.ItemName).type)
			{
				if (count <= stack.count)
				{
					stack.count -= count;
					UpdateSlot(i, stack);
					return;
				}
				else
				{
					count -= stack.count;
					stack.count = 0;
					UpdateSlot(i, stack);
				}
			}
		}
	}

	public int GetItemCount(Block.SItemNameCount sitem)
	{
		int having = 0;
		for (int i = 0; i < items.Length; i++)
		{
			ItemStack stack = items[i];
			if (stack.IsEmpty()) continue;
			// ToDo: how expensive is this call for `GetItem(string)`?
			if (stack.itemValue.type == ItemClass.GetItem(sitem.ItemName).type)
			{
				// Always leave at least one item in the slot
				having += stack.count;
			}
		}
		return having;
	}

	public bool CanRepairBlock(Block block)
	{
		if (block.RepairItems == null) return false;
		for (int i = 0; i < block.RepairItems.Count; i++)
		{
			int needed = block.RepairItems[i].Count;
			needed = (int)Mathf.Ceil(damagePerc * needed);
			if (GetItemCount(block.RepairItems[i]) < needed)
			{
				return false;
			}
		}

		return block.RepairItems.Count > 0;
	}

	public bool TakeRepairMaterials(Block block)
	{
		if (block.RepairItems == null) return false;
		for (int i = 0; i < block.RepairItems.Count; i++)
		{
			int needed = block.RepairItems[i].Count;
			needed = (int)Mathf.Ceil(damagePerc * needed);
			ReduceItemCount(block.RepairItems[i], needed);
		}

		return true;
	}

	public Vector3i GetRandomPos(World world, Vector3 pos, int size)
	{
		int x = 0; int y = 0; int z = 0;
		// We don't fix ourself!
		while (x == 0 && y == 0 && z == 0 && size != 0) {
			x = Random.Range(-size, size);
			y = Random.Range(-size, size);
			z = Random.Range(-size, size);
		}
		return new Vector3i(
			pos.x + x,
			pos.y + y,
			pos.z + z
		);

	}

	public void TickRepair(World world)
	{

		Vector3i worldPosI = ToWorldPos();
		Vector3 worldPos = ToWorldPos().ToVector3();

		// ToDo: probably don't need to recalculate on each tick since we reset on damage changes
		damagePerc = (float)repairBlock.damage / (float)Block.list[repairBlock.type].MaxDamage;

		// Check if we have a block for repair acquired
		if (repairBlock.type != BlockValue.Air.type)
		{

			// Get block currently at the position we try to repair
			BlockValue currentValue = world.GetBlock(repairPosition);
			// Check if any of the stats chaged after we acquired to block
			if (currentValue.type != repairBlock.type || currentValue.damage != repairBlock.damage)
			{
				// Reset the acquired block and play a sound bit
				// Play different sound according to reason of disconnect
				// Block has been switched (maybe destroyed, upgraded, etc.)
				// Block has been damaged again, abort repair on progress
				ResetAcquiredBlock(currentValue.type != repairBlock.type ?
					"weapon_jam" : "ItemNeedsRepair");
				return;
			}

			// Increase amount of repairing done
			repairDamage += Time.deltaTime * repairSpeed;

			// Check if repaired enough to fully restore
			if (repairBlock.damage <= repairDamage)
			{
				// Safety check if materials have changed
				if (!CanRepairBlock(Block.list[repairBlock.type])) {
					// Inventory seems to have changed (not repair possible)
					ResetAcquiredBlock("weapon_jam");
					return;
				}
				// Need to get the chunk first in order to alter the block?
				if (world.GetChunkFromWorldPos(repairPosition) is Chunk chunkFromWorldPos)
				{
					// Completely restore the block
					repairBlock.damage = 0;
					// Update the block at the given position (very low-level function)
					// Note: with this function we can basically install a new block at position
					world.SetBlock(chunkFromWorldPos.ClrIdx, repairPosition, repairBlock, false, false);
					// Take the repair materials from the container
					// ToDo: what if materials have gone missing?
					TakeRepairMaterials(repairBlock.Block);
					// BroadCast the changes done to the block
					world.SetBlockRPC(chunkFromWorldPos.ClrIdx, repairPosition,
						repairBlock, repairBlock.Block.Density);
					// Get material to play material specific sound
					var material = repairBlock.Block.blockMaterial.SurfaceCategory;
					world.GetGameManager().PlaySoundAtPositionServer(
						repairPosition.ToVector3(), // or at `worldPos`?
						string.Format("ImpactSurface/metalhit{0}", material),
						AudioRolloffMode.Logarithmic, 100);
					// Update clients
					SetModified();
				}
				// Reset acquired block
				ResetAcquiredBlock();
			}
			else
			{
				// Play simple click indicating we are working on something
				world.GetGameManager().PlaySoundAtPositionServer(worldPos,
					"repair_block", AudioRolloffMode.Logarithmic, 100);
			}

		}
		else
		{

			// Get size of land claim blocks to look for valid blocks to repair
			int claimSize = (GameStats.GetInt(EnumGameStats.LandClaimSize) - 1) / 2;

			// Speed up finding of blocks (for easier debugging purpose only!)
			// int n = 0; while (++n < 500 && repairBlock.type == BlockValue.Air.type)

			// Simple and crude random block acquiring
			// Repair block has slightly futher reach
			for (int i = 1; i <= claimSize + 5; i += 1)
			{

				// Get a random block and see if it need repair
				Vector3i randomPos = GetRandomPos(world, worldPos, i);
				BlockValue blockValue = world.GetBlock(randomPos);

				damagePerc = (float)(blockValue.damage) / (float)(Block.list[blockValue.type].MaxDamage);

				// Check if block needs repair and if we have the needed materials
				if (blockValue.damage > 0 && CanRepairBlock(blockValue.Block))
				{
					// Check if block is within a land claim block (don't repair stuff outside)
					// ToDo: Not sure if this is the best way to check this, but it should work
					PersistentPlayerList persistentPlayerList = world.GetGameManager().GetPersistentPlayerList();
					PersistentPlayerData playerData = persistentPlayerList.GetPlayerData(this.GetOwner());
					// int deadZone = GameStats.GetInt(EnumGameStats.LandClaimDeadZone) + claimSize;
					Chunk chunkFromWorldPos = (Chunk) world.GetChunkFromWorldPos(worldPosI);
					if (!IsBlockInsideClaim(world, chunkFromWorldPos, randomPos, playerData, claimSize, true))
					{
						continue;
					}
					// Play simple click indicating we are working on something
					world.GetGameManager().PlaySoundAtPositionServer(worldPos,
						"timer_stop", AudioRolloffMode.Logarithmic, 100);
					// Acquire the block to repair
					repairPosition = randomPos;
					repairBlock = blockValue;
					repairDamage = 0.0f;
					EnableBoundHelper();
					SetModified();
					break;
				}
			}

		}
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		this.isOn = _br.ReadBoolean();
		switch (_eStreamMode)
		{
		case TileEntity.StreamModeRead.Persistency:
			break;
		case TileEntity.StreamModeRead.FromServer:
			bool isRepairing = _br.ReadBoolean();
			this.repairPosition.x = _br.ReadInt32();
			this.repairPosition.y = _br.ReadInt32();
			this.repairPosition.z = _br.ReadInt32();
			if (isOn && isRepairing) {
				EnableBoundHelper();
			} else {
				ResetBoundHelper();
			}
			break;
		case TileEntity.StreamModeRead.FromClient:
			this.isAccessed = _br.ReadBoolean();
			if (this.isAccessed) {
				// This will provoke an update on
				// all clients to know new state.
				ResetAcquiredBlock("weapon_jam");
			}
			break;
		}
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(isOn);
		switch (_eStreamMode)
		{
		case TileEntity.StreamModeWrite.Persistency:
			break;
		case TileEntity.StreamModeWrite.ToServer:
			_bw.Write(IsUserAccessing());
			break;
		case TileEntity.StreamModeWrite.ToClient:
			_bw.Write(repairBlock.type != BlockValue.Air.type);
			_bw.Write(this.repairPosition.x);
			_bw.Write(this.repairPosition.y);
			_bw.Write(this.repairPosition.z);
			break;
		}
	}

	public void ResetAcquiredBlock(string playSound = "", bool broadcast = true)
	{
		if (repairBlock.type != BlockValue.Air.type) {
			// Play optional sound (only at the server to broadcast everywhere)
			if (playSound != "" && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) {
				GameManager.Instance.PlaySoundAtPositionServer(
					ToWorldPos().ToVector3(), playSound,
					AudioRolloffMode.Logarithmic, 100);
			}
			// Reset acquired repair block
			repairBlock = BlockValue.Air;
			repairPosition = ToWorldPos();
			damagePerc = 0.0f;
			repairDamage = 0.0f;
			ResetBoundHelper();
			if (broadcast) {
				SetModified();
			}
		}
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);

		// Check if storage is being accessed
		if (!IsOn || IsUserAccessing() || isAccessed)
		{
			ResetAcquiredBlock("weapon_jam");
		}
		else
		{
			// Call regular Tick
			TickRepair(world);
		}

	}

	public override void SetUserAccessing(bool _bUserAccessing) 
	{
		if (IsUserAccessing() != _bUserAccessing) {
			base.SetUserAccessing(_bUserAccessing);
			if (_bUserAccessing) {
				ResetAcquiredBlock("weapon_jam", false);
				SetModified(); // Force update
			}
			// SetModified is already called OnClose
		}
	}

	public void EnableBoundHelper()
	{
		if (BoundsHelper == null) return;
		BoundsHelper.localPosition = repairPosition.ToVector3() -
			Origin.position + new Vector3(0.5f, 0.5f, 0.5f);
		foreach (Renderer componentsInChild in BoundsHelper.GetComponentsInChildren<Renderer>())
			componentsInChild.material.SetColor("_Color", Color.yellow * 0.5f);
		BoundsHelper.gameObject.SetActive(this.isOn);
	}

	public void ResetBoundHelper()
	{
		if (BoundsHelper == null) return;
		BoundsHelper.localPosition = ToWorldPos().ToVector3() -
			Origin.position + new Vector3(0.5f, 0.5f, 0.5f);
		foreach (Renderer componentsInChild in BoundsHelper.GetComponentsInChildren<Renderer>())
			componentsInChild.material.SetColor("_Color", Color.gray * 0.5f);
		BoundsHelper.gameObject.SetActive(this.isOn);
	}

	private bool IsBlockInsideClaim(
		World world,
		Chunk chunk,
		Vector3i blockPos,
		PersistentPlayerData lpRelative,
		int claimSize,
		bool includeAllies)
	{
		PersistentPlayerList persistentPlayerList = world.gameManager.GetPersistentPlayerList();
		List<Vector3i> indexedBlock = chunk.IndexedBlocks["lpblock"];
		if (indexedBlock != null)
		{
			Vector3i worldPos = chunk.GetWorldPos();
			for (int index = 0; index < indexedBlock.Count; ++index)
			{
				Vector3i pos = indexedBlock[index] + worldPos;
				if (BlockLandClaim.IsPrimary(chunk.GetBlock(indexedBlock[index])))
				{
					int num1 = Mathf.Abs(pos.x - blockPos.x);
					int num2 = Mathf.Abs(pos.z - blockPos.z);
					if (num1 <= claimSize && num2 <= claimSize)
					{
						PersistentPlayerData protectionBlockOwner = persistentPlayerList.GetLandProtectionBlockOwner(pos);
						if (protectionBlockOwner != null)
						{
							bool flag = world.IsLandProtectionValidForPlayer(protectionBlockOwner);
							if (flag && lpRelative != null)
							{
								if (lpRelative == protectionBlockOwner) flag = true;
								else if (includeAllies && protectionBlockOwner.ACL != null &&
										 protectionBlockOwner.ACL.Contains(lpRelative.PlayerId)) {
									flag = true;
								}
							}
							if (flag)
								return true;
						}
					}
				}
			}
		}
		return false;
	}

}
