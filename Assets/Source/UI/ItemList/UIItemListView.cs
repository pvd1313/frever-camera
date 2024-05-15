using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Frever.UI.ItemList
{
    public class UIItemListView
    {
        public event Action<int> selectionChange;
        
        public int selectedItemIndex { get; private set; }
        
        private readonly RectTransform _itemRoot;
        private readonly UIItemListConfig _config;
        private readonly List<UIListItemPrefab> _items;

        public UIItemListView(RectTransform itemRoot, UIItemListConfig config)
        {
            _itemRoot = itemRoot;
            _config = config;
            selectedItemIndex = -1;
            _items = new List<UIListItemPrefab>();
        }

        public void AddItem(string text)
        {
            UIListItemPrefab item = GameObject.Instantiate(_config.uiListItemPrefab, _itemRoot);
            item.text.text = text;
            item.backgroundImage.color = _config.defaultColor;
            int itemIndex = _items.Count;
            item.button.onClick.AddListener(() => OnItemClicked(itemIndex));

            _items.Add(item);
        }

        private void OnItemClicked(int itemIndex)
        {
            if (selectedItemIndex == itemIndex)
            {
                return;
            }
            
            SetSelectedItem(itemIndex);

            selectionChange?.Invoke(selectedItemIndex);
        }

        public void SetSelectedItem(int itemIndex)
        {
            if (selectedItemIndex == itemIndex)
            {
                return;
            }

            if (selectedItemIndex != -1)
            {
                _items[selectedItemIndex].backgroundImage.color = _config.defaultColor;
            }

            selectedItemIndex = itemIndex;

            if (selectedItemIndex != -1)
            {
                _items[selectedItemIndex].backgroundImage.color = _config.selectedColor;
            }
        }

        public string GetItem(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= _items.Count)
            {
                throw new IndexOutOfRangeException("itemIndex is out of range");
            }

            return _items[itemIndex].text.text;
        }
    }
}