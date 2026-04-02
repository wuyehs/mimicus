public class SmokeGrenadePickup : BasePickup
{
    protected override void OnPickup(CharacterLogic character)
    {
        character.PickUpSmokeGrenade();
    }
}