public class Item
{
    public int id;
    public ItemParameter param;
    public ItemInfo info;

    public Item(int id)
    {
        this.id = id;
        param = ItemParameterList.GetItemStat(id);
        info = info ?? new ItemInfo(); // ✔️ 기본만 만들어두고, 실제 표시정보는 Factory가 채움
    }

    public virtual void Use() { /* ... */ }
}
