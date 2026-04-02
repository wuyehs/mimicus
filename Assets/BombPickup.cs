public class BombPickup : BasePickup
{
    protected override void OnPickup(CharacterLogic character)
    {
        character.PickUpBomb();   // 你需要在 CharacterLogic 中实现该方法
    }
}