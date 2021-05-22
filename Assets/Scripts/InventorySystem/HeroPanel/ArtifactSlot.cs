namespace HOMM_BM
{
    public class ArtifactSlot : ItemSlot
    {
        public ArtifactType ArtifactType;

        protected override void OnValidate()
        {
            base.OnValidate();
            gameObject.name = ArtifactType.ToString() + " Slot";
        }

        public override bool CanReceiveItem(Item item)
        {
            if (item == null)
                return true;

            EquippableItem equippableItem = item as EquippableItem;
            return equippableItem != null && equippableItem.ArtifactType == ArtifactType;
        }
    }
}