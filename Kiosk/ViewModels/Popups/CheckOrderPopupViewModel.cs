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
    public class CartItem
    {
        public OrderItem Item { get; set; }
        public string ImagePath { get; set; }
    }

    public class CheckOrderPopupViewModel : PopupViewModelBase
    {
        public ObservableCollection<CartItem> Cart { get; set; }
        private int _TotalPrice;
        public int TotalPrice { get => _TotalPrice; set => SetValue(ref _TotalPrice, value); }

        public Command InitCommand { get; set; }

        public CheckOrderPopupViewModel() : base("주문내역 확인", PopupButtonStyleEnum.CancelNext)
        {
            Cart = new ObservableCollection<CartItem>();
            UpdateCart();

            NextPopup = new SelectPaymentMethodPopupViewModel
            {
                PrevPopup = this
            };

            InitCommand = new Command(Init);
        }

        private void Init()
        {
            DataManager.instance.SetCurrentScreen(KioskScreenEnum.CheckOrderPopup);
            TextToSpeech.Instance.Speak("주문내역을 확인해주세요.", "주문내역이 맞으시면 다음이라고 말해주세요.");
        }

        private void UpdateCart()
        {
            Cart.Clear();
            foreach (var item in DataManager.instance.GetSelectedOrderItems())
            {
                CartItem cartItem = new CartItem
                {
                    Item = item,
                    ImagePath = DataManager.instance.GetProductImagePath(item.Name),
                };
                Cart.Add(cartItem);
            }
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            TotalPrice = 0;
            Cart.ToList().ForEach(x => TotalPrice += x.Item.Price);
        }
    }
}
