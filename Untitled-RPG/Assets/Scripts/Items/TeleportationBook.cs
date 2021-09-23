
public class TeleportationBook : Resource
{

    public override void Use()
    {
        TeleportManager.instance.TeleportMenu();
    }

    public override void Use(UI_InventorySlot initialSlot){}
}
