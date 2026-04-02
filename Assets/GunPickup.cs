public class GunPickup : BasePickup
{
    protected override void OnPickup(CharacterLogic character)
    {
        character.PickUpGun();
    }
}