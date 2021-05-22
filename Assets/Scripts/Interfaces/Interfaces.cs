namespace HOMM_BM
{
    public interface ISelectable
    {
        GridUnit GetGridUnit();
    }
    public interface IItemContainer
    {
        bool CanAddItem(Item item, int amount = 1);
        bool AddItem(Item item);
        bool AddItems(Item item, int amount);
        Item RemoveItem(string itemID);
        bool RemoveItem(Item item);
        void Clear();
        int ItemCount(string itemID);
    }
}