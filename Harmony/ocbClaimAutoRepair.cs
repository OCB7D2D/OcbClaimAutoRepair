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
				__result = (TileEntity) new TileEntityClaimAutoRepair(_chunk);
				return false;
			}
			return true;
		}
	}

}
