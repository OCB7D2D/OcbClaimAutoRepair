using DMT;
using Audio;
using System;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

public class BlockClaimAutoRepair : BlockSecureLoot
{

	const float BoundHelperSize = 2.59f;

	float TakeDelay = 30f;

	// Copied from vanilla BlockLandClaim code
	// public override void OnBlockEntityTransformBeforeActivated(
	// 	WorldBase _world,
	// 	Vector3i _blockPos,
	// 	int _cIdx,
	// 	BlockValue _blockValue,
	// 	BlockEntityData _ebcd)
	// {
	// 	base.OnBlockEntityTransformBeforeActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
	// }

	public override void Init()
	{
		base.Init();
		this.TakeDelay = !this.Properties.Values.ContainsKey("TakeDelay") ? 2f : StringParsers.ParseFloat(this.Properties.Values["TakeDelay"]);
	}

	// Copied from vanilla BlockLandClaim code
	public override void OnBlockLoaded(
		WorldBase _world,
		int _clrIdx,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		if (GameManager.IsDedicatedServer) return;
		if (_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityClaimAutoRepair tileEntityLandAutoRepair)
		{
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(_blockPos.ToVector3());
			if (boundsHelper != null)
			{
				boundsHelper.localScale = new Vector3(BoundHelperSize, BoundHelperSize, BoundHelperSize);
				boundsHelper.localPosition = new Vector3(_blockPos.x + 0.5f, _blockPos.y + 0.5f, _blockPos.z + 0.5f);
				tileEntityLandAutoRepair.BoundsHelper = boundsHelper;
				tileEntityLandAutoRepair.ResetBoundHelper();
			}
		}
	}

	// Copied from vanilla BlockLandClaim code
	// public override void OnBlockValueChanged(
	// 	WorldBase _world,
	// 	Chunk _chunk,
	// 	int _clrIdx,
	// 	Vector3i _blockPos,
	// 	BlockValue _oldBlockValue,
	// 	BlockValue _newBlockValue)
	// {
	// 	base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
	// }

	// Copied from vanilla BlockLandClaim code
	public override void OnBlockEntityTransformAfterActivated(
		WorldBase _world,
		Vector3i _blockPos,
		int _cIdx,
		BlockValue _blockValue,
		BlockEntityData _ebcd)
	{
		if (_ebcd == null) return;
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityClaimAutoRepair)) {
			Chunk chunkFromWorldPos = (Chunk) _world.GetChunkFromWorldPos(_blockPos);
			TileEntityClaimAutoRepair tileEntity = new TileEntityClaimAutoRepair(chunkFromWorldPos);
			tileEntity.localChunkPos = World.toBlock(_blockPos);
			tileEntity.lootListIndex = (int) (ushort) this.lootList;
			tileEntity.SetContainerSize(LootContainer.lootList[this.lootList].size);
			chunkFromWorldPos.AddTileEntity((TileEntity) tileEntity);
		}

		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
	}

	// Copied from vanilla BlockLandClaim code
	public override void OnBlockAdded(
		WorldBase _world,
		Chunk _chunk,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		if (_blockValue.ischild || _world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityClaimAutoRepair)
			return;

		// Overload TileEntity creation (base method should still recognize this)
		TileEntityClaimAutoRepair tileEntity = new TileEntityClaimAutoRepair(_chunk);
		tileEntity.localChunkPos = World.toBlock(_blockPos);
		tileEntity.lootListIndex = (int) (ushort) this.lootList;
		tileEntity.SetContainerSize(LootContainer.lootList[this.lootList].size);
		_chunk.AddTileEntity((TileEntity) tileEntity);

		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		if (GameManager.IsDedicatedServer) return;
		if (_world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityClaimAutoRepair tileEntityLandAutoRepair)
		{
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(_blockPos.ToVector3());
			if (boundsHelper != null)
			{
				boundsHelper.localScale = new Vector3(BoundHelperSize, BoundHelperSize, BoundHelperSize);
				boundsHelper.localPosition = new Vector3(_blockPos.x + 0.5f, _blockPos.y + 0.5f, _blockPos.z + 0.5f);
				tileEntityLandAutoRepair.BoundsHelper = boundsHelper;
				tileEntityLandAutoRepair.ResetBoundHelper();
			}
		}
	}

	// Copied from vanilla BlockLandClaim code
	public override void OnBlockRemoved(
		WorldBase _world,
		Chunk _chunk,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		if (_world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityClaimAutoRepair tileEntityLandAutoRepair)
		{
			LandClaimBoundsHelper.RemoveBoundsHelper(_blockPos.ToVector3());
		}
	}

	// Copied from vanilla BlockLandClaim code
	public override void OnBlockUnloaded(
		WorldBase _world,
		int _clrIdx,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
		if (_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityClaimAutoRepair tileEntityLandAutoRepair)
		{
			LandClaimBoundsHelper.RemoveBoundsHelper(_blockPos.ToVector3());
		}
	}

	// Copied from vanilla BlockLandClaim code
	// public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
	// {
	// 	base.PlaceBlock(_world, _result, _ea);
	// }

	public override BlockActivationCommand[] GetBlockActivationCommands(
		WorldBase _world,
		BlockValue _blockValue,
		int _clrIdx,
		Vector3i _blockPos,
		EntityAlive _entityFocusing)
	{
		TileEntityClaimAutoRepair tileEntity = _world.GetTileEntity(_clrIdx, _blockPos) as TileEntityClaimAutoRepair;
		BlockActivationCommand[] cmds = base.GetBlockActivationCommands(_world, _blockValue, _clrIdx, _blockPos, _entityFocusing);
		Array.Resize(ref cmds, cmds.Length + 2);
		cmds[cmds.Length - 2] = new BlockActivationCommand("take", "hand", false);

		string cmd = tileEntity.IsOn ? "turn_claimautorep_off" : "turn_claimautorep_on";
		cmds[cmds.Length - 1] = new BlockActivationCommand(cmd, "electric_switch", true);
		if (this.CanPickup)
			cmds[cmds.Length - 2].enabled = true;
		else if ((double) EffectManager.GetValue(PassiveEffects.BlockPickup, _entity: _entityFocusing, tags: _blockValue.Block.Tags) > 0.0)
			cmds[cmds.Length - 2].enabled = true;
		else 
			cmds[cmds.Length - 2].enabled = false;
		return cmds;
	}

	public override bool OnBlockActivated(
		int _indexInBlockActivationCommands,
		WorldBase _world,
		int _cIdx,
		Vector3i _blockPos,
		BlockValue _blockValue,
		EntityAlive _player)
	{
		if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityClaimAutoRepair tileEntity)) return false;
		if (_indexInBlockActivationCommands == 5)
		{
			// Copied from vanilla Block::OnBlockActivated
			bool flag = this.CanPickup;
			if ((double) EffectManager.GetValue(PassiveEffects.BlockPickup, _entity: _player, tags: _blockValue.Block.Tags) > 0.0)
			flag = true;
			if (!flag) return false;
			if (!_world.CanPickupBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()))
			{
				_player.PlayOneShot("keystone_impact_overlay");
				return false;
			}
			if (_blockValue.damage > 0)
			{
				GameManager.ShowTooltipWithAlert(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup"), "ui_denied");
				return false;
			}
			ItemStack itemStack = Block.list[_blockValue.type].OnBlockPickedUp(_world, _cIdx, _blockPos, _blockValue, _player.entityId);
			if (!_player.inventory.CanTakeItem(itemStack) && !_player.bag.CanTakeItem(itemStack))
			{
				GameManager.ShowTooltipWithAlert(_player as EntityPlayerLocal, Localization.Get("xuiInventoryFullForPickup"), "ui_denied");
				return false;
			}
			TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
			return false;

		}
		else if (_indexInBlockActivationCommands == 6)
		{
			tileEntity.IsOn = !tileEntity.IsOn;
			return true;
		}
		else {
			return base.OnBlockActivated(_indexInBlockActivationCommands, _world, _cIdx, _blockPos, _blockValue, _player);
		}
	}

	public override string GetActivationText(
		WorldBase _world,
		BlockValue _blockValue,
		int _clrIdx,
		Vector3i _blockPos,
		EntityAlive _entityFocusing)
	{
		return base.GetActivationText(_world, _blockValue, _clrIdx, _blockPos, _entityFocusing);
	}

	public void TakeItemWithTimer(
		int _cIdx,
		Vector3i _blockPos,
		BlockValue _blockValue,
		EntityAlive _player)
	{
		if (_blockValue.damage > 0)
		{
			GameManager.ShowTooltipWithAlert(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup"), "ui_denied");
		}
		else
		{
			LocalPlayerUI playerUi = (_player as EntityPlayerLocal).PlayerUI;
			playerUi.windowManager.Open("timer", true);
			XUiC_Timer childByType = playerUi.xui.GetChildByType<XUiC_Timer>();
			TimerEventData _eventData = new TimerEventData();
			_eventData.Data = (object) new object[4]
			{
				(object) _cIdx,
				(object) _blockValue,
				(object) _blockPos,
				(object) _player
			};
			_eventData.Event += new TimerEventHandler(this.EventData_Event);
			childByType.SetTimer(this.TakeDelay, _eventData);
		}
	}

	private void EventData_Event(TimerEventData timerData)
	{
		World world = GameManager.Instance.World;
		object[] data = (object[]) timerData.Data;
		int _clrIdx = (int) data[0];
		BlockValue blockValue = (BlockValue) data[1];
		Vector3i vector3i = (Vector3i) data[2];
		BlockValue block = world.GetBlock(vector3i);
		EntityPlayerLocal entityPlayerLocal = data[3] as EntityPlayerLocal;
		if (block.damage > 0)
		{
			GameManager.ShowTooltipWithAlert(entityPlayerLocal, Localization.Get("ttRepairBeforePickup"), "ui_denied");
		}
		else if (block.type != blockValue.type)
		{
			GameManager.ShowTooltipWithAlert(entityPlayerLocal, Localization.Get("ttBlockMissingPickup"), "ui_denied");
		}
		else
		{
			TileEntityClaimAutoRepair tileEntity = world.GetTileEntity(_clrIdx, vector3i) as TileEntityClaimAutoRepair;
			if (tileEntity.IsUserAccessing())
			{
				GameManager.ShowTooltipWithAlert(entityPlayerLocal, Localization.Get("ttCantPickupInUse"), "ui_denied");
			}
			else
			{
				LocalPlayerUI uiForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
				this.HandleTakeInternalItems(tileEntity, uiForPlayer);
				ItemStack itemStack = new ItemStack(block.ToItemValue(), 1);
				if (!uiForPlayer.xui.PlayerInventory.AddItem(itemStack))
				uiForPlayer.xui.PlayerInventory.DropItem(itemStack);
				world.SetBlockRPC(_clrIdx, vector3i, BlockValue.Air);
			}
		}
	}

	protected virtual void HandleTakeInternalItems(TileEntityClaimAutoRepair te, LocalPlayerUI playerUI)
	{
		ItemStack[] items = te.items;
		for (int index = 0; index < items.Length; ++index)
		{
		if (!items[index].IsEmpty() && !playerUI.xui.PlayerInventory.AddItem(items[index]))
			playerUI.xui.PlayerInventory.DropItem(items[index]);
		}
	}

}
