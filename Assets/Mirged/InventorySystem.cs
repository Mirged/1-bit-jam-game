using System.Collections
using System.Collections.Generic;
using UnityEngine;

namespace Mirged
{
    public class InventorySystem : MonoBehaviour
    {
        [SerializeField]
        private int maxItemCount;

        [SerializeField]
        private int maxItemPerId;

        public List<Item> Items;

        public void AddItem(Item item)
        {
            if (Items.Count < maxItemCount)
            {
                Item existingItem = GetItemById(item.id);
                if (existingItem != null)
                {
                    if (existingItem.quantity + item.quantity <= maxItemPerId)
                    {
                        existingItem.quantity += item.quantity;
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
            return Items.Find(x => x.id == itemId);
        }

        public int GetItemQuantity(int itemId)
        {
            Item item = GetItemById(itemId);

            if (item != null)
            {
                return item.quantity;
            }

            return 0;
        }

        public void SetItemQuantity(int itemId, int quantity)
        {
            Item item = GetItemById(itemId);

            if (item != null)
            {
                item.quantity = quantity;
            }
        }
    }
}
