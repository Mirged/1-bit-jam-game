using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirged
{
    [System.Serializable]
    public class Item
    {
        public int ID;
        public string Name;
        public string Description;
        public int Quantity;

        public Item(int id, string name, string description, int quantity)
        {
            this.ID = id;
            this.Name = name;
            this.Description = description;
            this.Quantity = quantity;
        }
    }
}
