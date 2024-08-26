// Simple class to test repair speed in game
// When placing this block it well be damaged
public class BlockOcbLcbRepairTest : Block
{
    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        _result.blockValue.damage = 5000;
        base.PlaceBlock(_world, _result, _ea);
    }
}
