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
    
    [HarmonyPatch(typeof(TileEntityLandClaim))]
    [HarmonyPatch("UpdateTick")]
    public class TileEntityLandClaim_UpdateTick
    {
        static void Postfix(TileEntityLandClaim __instance)
        {
			World world = GameManager.Instance.World;
			// object[] data = (object[]) timerData.Data;
			// int _clrIdx = (int) data[0];
			// BlockValue blockValue = (BlockValue) data[1];
			// Vector3i vector3i = (Vector3i) data[2];
			// BlockValue block = world.GetBlock(vector3i);

			var worldPos = __instance.ToWorldPos().ToVector3();

			for (int i = 0; i < 250; i ++) {

			var Pos = worldPos - Origin.position;// + new Vector3(0.5f, 0.0f, 0.5f);

				Pos += new Vector3(
					Random.Range(-3, 3),
					Random.Range(-3, 3),
					Random.Range(-3, 3)
				);

				BlockValue blockValue = world.GetBlock(new Vector3i(Pos));
				// var asd = blockValue.block;
				// TileEntity titty =_world.GetTileEntity(_cIdx, _blockPos);
				// int blockType = blockValue.type;


				if (blockValue.damage > 0) {

Block ls = blockValue.Block;

					Log.Out("UpdateTick Land-Claim " + worldPos + " vs " + Origin.position + " vs " + Pos);
					// Log.Out("Block is " + blockValue + " vs " + blockType + " other " + blockValue.RepairItems);
					Log.Out("Needs repair " + blockValue.Block);
					break;
				}
        	}
		}

    }

}
