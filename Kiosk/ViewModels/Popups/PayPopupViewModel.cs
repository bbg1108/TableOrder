using Kiosk.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Kiosk.CommonEnum;

namespace Kiosk.ViewModels
{
    public class PayPopupViewModel : PopupViewModelBase
    {
        public string PaymentMethodText { get; set; }
        public ImageSource PayImageSource { get; set; }

        public Command PayCommand { get; set; }
        public Command InitCommand { get; set; }

        public PayPopupViewModel(PaymentMethodEnum method) : base("결제 진행", PopupButtonStyleEnum.Prev, 700, 450)
        {
            InitPayImageSource(method);
            InitPaymentMethodText(method);

            PayCommand = new Command(Pay);
            InitCommand = new Command(Init);
        }

        private void Init()
        {
            DataManager.instance.SetCurrentScreen(KioskScreenEnum.PayPopup);
        }

        private void InitPayImageSource(PaymentMethodEnum method)
        {
            switch (method)
            {
                case PaymentMethodEnum.Card:
                    PayImageSource = new BitmapImage(new Uri(CommonPath.ImageDir + "Card_insert.jpg", UriKind.Absolute));
                    break;
                case PaymentMethodEnum.Barcode:
                    PayImageSource = new BitmapImage(new Uri(CommonPath.ImageDir + "Barcode2.png", UriKind.Absolute));
                    break;
            }
        }

        private void InitPaymentMethodText(PaymentMethodEnum method)
        {
            switch (method)
            {
                case PaymentMethodEnum.Card:
                    PaymentMethodText = "카드를 투입구에 넣어주세요";
                    break;
                case PaymentMethodEnum.Barcode:
                    PaymentMethodText = "바코드를 찍어주세요";
                    break;
            }
        }

        private async void Pay()
        {
            // 결제시 장바구니 목록을 tcp로 전송
            try
            {
                var selectedProducts = DataManager.instance.GetSelectedOrderItems();
                int totalCnt = selectedProducts.Count;
                OrderItem[] orderItemArray = new OrderItem[totalCnt];
                int totalPrice = 0;
                int index = 0;

                foreach (var product in selectedProducts)
                {
                    orderItemArray[index] = new OrderItem
                    {
                        Name = product.Name,
                        Count = product.Count,
                        Price = product.Price
                    };
                    totalPrice += orderItemArray[index++].Price;
                }

                var orderDetails = new OrderDetails
                {
                    TableNo = DataManager.instance.TableNo,
                    ItemsCount = totalCnt,
                    Items = orderItemArray,
                    TotalPrice = totalPrice,
                    OrderTime = DateTimeOffset.UtcNow
                };

                bool sendResult = await TcpComm.Instance.SendAsync(orderDetails);
                if (sendResult)
                {
                    // 전송 성공
                    NextPopup = new PaymentSuccessPopupViewModel();
                    OnNext();
                }
            }
            catch (Exception ex)
            {
                AlertPopup.Show("결제 실패", "결제를 실패했습니다. 다시 시도해주세요.");
                FileLogger.Log(ex);
            }
        }
    }
}
