using DMT;
using Audio;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

public class BlockClaimAutoRepair : BlockSecureLoot
{

	const float BoundHelperSize = 2.59f;

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

	// Copied from vanilla BlockLandClaim code
	public override void OnBlockLoaded(
		WorldBase _world,
		int _clrIdx,
		Vector3i _blockPos,
		BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		if (_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityClaimAutoRepair tileEntityLandAutoRepair)
		{
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(_blockPos.ToVector3());
			if (boundsHelper != null)
			{
				boundsHelper.localScale = new Vector3(BoundHelperSize, BoundHelperSize, BoundHelperSize);
				boundsHelper.localPosition = new Vector3(_blockPos.x + 0.5f, _blockPos.y + 0.5f, _blockPos.z + 0.5f);
				tileEntityLandAutoRepair.BoundsHelper = boundsHelper;
				tileEntityLandAutoRepair.DisableBoundHelper();
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
		if (_world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityClaimAutoRepair tileEntityLandAutoRepair)
		{
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(_blockPos.ToVector3());
			if (boundsHelper != null)
			{
				boundsHelper.localScale = new Vector3(BoundHelperSize, BoundHelperSize, BoundHelperSize);
				boundsHelper.localPosition = new Vector3(_blockPos.x + 0.5f, _blockPos.y + 0.5f, _blockPos.z + 0.5f);
				tileEntityLandAutoRepair.BoundsHelper = boundsHelper;
				tileEntityLandAutoRepair.DisableBoundHelper();
			}
		}	}

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

}
