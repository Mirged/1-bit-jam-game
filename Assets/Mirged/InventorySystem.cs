using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirged
{
    public class InventorySystem : MonoBehaviour
    {
        public List<Item> Items;

        [SerializeField]
        private int _maxItemCount;

        [SerializeField]
        private int _maxItemPerId;

        public void AddItem(Item item)
        {
            if (Items.Count < _maxItemCount)
            {
                Item existingItem = GetItemById(item.ID);
                if (existingItem != null)
                {
                    if (existingItem.Quantity + item.Quantity <= _maxItemPerId)
                    {
                        existingItem.Quantity += item.Quantity;
                    }
                    else
                    {
                        Debug.LogWarning("Reached max item quantity per ID.");
                    }
                }
                else
                {
                    Items.Add(item);
                }
            }
            else
            {
                Debug.LogWarning("Reached max item count.");
            }
        }

        public void RemoveItem(int itemId)
        {
            Item item = GetItemById(itemId);

            if (item != null)
            {
                Items.Remove(item);
            }
        }

        public Item GetItemById(int itemId)
        {
            return Items.Find(x => x.ID == itemId);
        }

        public int GetItemQuantity(int itemId)
        {
            Item item = GetItemById(itemId);

            if (item != null)
            {
                return item.Quantity;
            }

            return 0;
        }

        public void SetItemQuantity(int itemId, int quantity)
        {
            Item item = GetItemById(itemId);

            if (item != null)
            {
                item.Quantity = quantity;
            }
        }
    }
}
