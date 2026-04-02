using UnityEngine;

public class PhantomStaffPickup : BasePickup
{
    protected override void OnPickup(CharacterLogic character)
    {
        character.PickUpPhantomStaff();
    }
}