using HarmonyLib;
using UnityEngine;
using System.Reflection;

public class OcbClaimAutoRepair : IModApi
{

	// Entry class for A20 patching
	public void InitMod(Mod mod)
	{
		Log.Out("OCB Harmony Patch: " + GetType().ToString());
		Harmony harmony = new Harmony(GetType().ToString());
		harmony.PatchAll(Assembly.GetExecutingAssembly());
	}

	[HarmonyPatch(typeof(TileEntity))]
	[HarmonyPatch("Instantiate")]
	public class TileEntity_Instantiate
	{
		public static bool Prefix(TileEntityType type, Chunk _chunk, ref TileEntity __result)
		{
			if (type == (TileEntityType)242) {
				__result = new TileEntityClaimAutoRepair(_chunk);
				return false;
			}
			return true;
		}
	}

}
