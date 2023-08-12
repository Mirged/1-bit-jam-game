namespace Mirged
{
    [System.Serializable]
    public class Item
    {
        public int id;
        public string name;
        public string description;
        public int quantity;

        public Item(int id, string name, string description, int quantity)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.quantity = quantity;
        }
    }
}
