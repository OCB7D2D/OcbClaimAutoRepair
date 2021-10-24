using DMT;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

public class OcbClaimAutoRepair
{

	// Entry class for Harmony patching
	public class OcbClaimAutoRepair_Init : IHarmony	
	{
		public void Start()
		{
			Debug.Log("Loading OCB Claim Auto Repair Patch: " + GetType().ToString());
			var harmony = new Harmony(GetType().ToString());
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}

	[HarmonyPatch(typeof(TileEntity))]
	[HarmonyPatch("Instantiate")]
	public class TileEntity_Instantiate
	{
		public static bool Prefix(TileEntityType type, Chunk _chunk, ref TileEntity __result)
		{
			if (type == (TileEntityType)242) {
				__result = (TileEntity) new TileEntityClaimAutoRepairContainer(_chunk);
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(BlockSecureLoot))]
	[HarmonyPatch("OnBlockAdded")]
	public class BlockSecureLoot_OnBlockAdded
	{
		public static bool Prefix(
			BlockSecureLoot __instance,
			WorldBase world,
			Chunk _chunk,
			Vector3i _blockPos,
			BlockValue _blockValue,
			int ___lootList)
		{
			if (__instance.Properties.Values.ContainsKey("CustomIcon")) {
				var icon = __instance.Properties.Values["CustomIcon"];
				if (icon == "ClaimAutoRepair") {

					if (_blockValue.ischild)
						return true;
 
					__instance.shape.OnBlockAdded(world, _chunk, _blockPos, _blockValue);

					if (__instance.isMultiBlock)
						__instance.multiBlockPos.AddChilds(world, _chunk, _chunk.ClrIdx, _blockPos, _blockValue);

					TileEntitySecureLootContainer secureLootContainer = new TileEntityClaimAutoRepairContainer(_chunk);
					secureLootContainer.localChunkPos = World.toBlock(_blockPos);
					secureLootContainer.lootListIndex = (int)(ushort)___lootList;
					secureLootContainer.SetContainerSize(LootContainer.lootList[___lootList].size);
					_chunk.AddTileEntity((TileEntity)secureLootContainer);
					return false;
				}
			}
			return true;
		}
	}


}
