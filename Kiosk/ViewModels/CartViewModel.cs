using Kiosk.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk.ViewModels
{
    public class CartViewModel : ViewModelBase
    {
        public ObservableCollection<CartItemVM> CartItems { get; set; }
        private int _TotalPrice;
        public int TotalPrice { get => _TotalPrice; set => SetValue(ref _TotalPrice, value); }

        public Command PayCommand { get; set; }
        public Command ClearCommand { get; set; }

        public CartViewModel()
        {
            CartItems = new ObservableCollection<CartItemVM>();
            PayCommand = new Command(Pay);
            ClearCommand = new Command(ClearItems);

            Messenger.Instance.Subscribe<Product>(MessengerEnum.SelectItem, this, AddItem);
            Messenger.Instance.Subscribe<Product>(MessengerEnum.DecreaseItem, this, DecreaseItem);
            Messenger.Instance.Subscribe<Product>(MessengerEnum.RemoveItem, this, RemoveItem);
            Messenger.Instance.Subscribe<int>(MessengerEnum.IncTotalPrice, this, IncTotalPrice);
            Messenger.Instance.Subscribe<int>(MessengerEnum.DecTotalPrice, this, DecTotalPrice);
            Messenger.Instance.Subscribe(MessengerEnum.ClearItems, this, (object obj) => { ClearItems(); });
        }

        private void AddItem(Product item)
        {
            var cartItem = CartItems.FirstOrDefault(x => x.Item == item);
            if (cartItem == null)
            {
                cartItem = new CartItemVM
                {
                    Item = item
                };
                CartItems.Add(cartItem);
            }
            cartItem.IncreaseItem();
        }

        private void DecreaseItem(Product item)
        {
            var cartItem = CartItems.FirstOrDefault(x => x.Item == item);
            if (cartItem != null)
            {
                cartItem.DecreaseItem();
            }
        }

        private void RemoveItem(Product item)
        {
            var cartItem = CartItems.FirstOrDefault(x => x.Item == item);
            if (cartItem?.Count == 0)
            {
                CartItems.Remove(cartItem);
            }
        }

        private void ClearItems()
        {
            TotalPrice = 0;
            CartItems.Clear();
            DataManager.instance.ClearSelectedOrderItems();
        }

        private void IncTotalPrice(int price)
        {
            TotalPrice += price;
            DataManager.instance.IncTotalPrice(price);
        }

        private void DecTotalPrice(int price)
        {
            TotalPrice -= price;
            DataManager.instance.DecTotalPrice(price);
        }

        private void Pay()
        {
            if (CartItems.Count == 0 || TotalPrice == 0)
                AlertPopup.Show("알림", "메뉴를 선택해주세요");
            else
                Messenger.Instance.Send<object>(MessengerEnum.ClickPay, null);
        }
    }

    public class CartItemVM : ViewModelBase
    {
        private Product _Item;
        public Product Item { get => _Item; set => SetValue(ref _Item, value); }
        private int _Count;
        public int Count { get => _Count; set => SetValue(ref _Count, value); }
        private int _Price;
        public int Price { get => _Price; set => SetValue(ref _Price, value); }

        public Command AddItemCommand { get; set; }
        public Command DelItemCommand { get; set; }

        public CartItemVM()
        {
            Item = new Product();
            AddItemCommand = new Command(IncreaseItem);
            DelItemCommand = new Command(DecreaseItem);
        }

        public void IncreaseItem()
        {
            Count++;
            Price += Item.Price;
            DataManager.instance.AddSelectedOrderItem(Item);
            Messenger.Instance.Send(MessengerEnum.IncTotalPrice, Item.Price);
        }

        public void DecreaseItem()
        {
            Count--;
            Price -= Item.Price;
            DataManager.instance.RemoveSelectedOrderItem(Item);
            Messenger.Instance.Send(MessengerEnum.DecTotalPrice, Item.Price);

            if (Count == 0)
            {
                Messenger.Instance.Send(MessengerEnum.RemoveItem, Item);
            }
        }
    }
}
