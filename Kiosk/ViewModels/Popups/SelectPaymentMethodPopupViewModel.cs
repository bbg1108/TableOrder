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
    public class SelectPaymentMethodPopupViewModel : PopupViewModelBase
    {
        public List<PaymentMethodButtonVM> Buttons { get; set; }

        public Command InitCommand { get; set; }

        public SelectPaymentMethodPopupViewModel() : base("결제수단 선택", PopupButtonStyleEnum.CancelPrev)
        {
            Buttons = new List<PaymentMethodButtonVM>();
            InitButtons();

            InitCommand = new Command(Init);
            Messenger.Instance.Subscribe(MessengerEnum.SelectPaymentMethod, this, (PaymentMethodEnum method) =>
            {
                NextPopup = new PayPopupViewModel(method) { PrevPopup = this };
                OnNext();
            });
        }

        private void Init()
        {
            DataManager.instance.SetCurrentScreen(KioskScreenEnum.SelectPaymentMethodPopup);
            TextToSpeech.Instance.Speak("결제수단을 선택하세요.", "결제수단을 말해주세요.");
        }

        private void InitButtons()
        {
            int enumCnt = Enum.GetValues(typeof(PaymentMethodEnum)).Length;
            for (int i = 0; i < enumCnt; i++)
            {
                var type = (PaymentMethodEnum)i;
                var attr = typeof(PaymentMethodEnum).GetField(type.ToString()).GetCustomAttributes(false);
                ImageInfoAttribute imageInfo = attr[1] as ImageInfoAttribute;

                Buttons.Add(new PaymentMethodButtonVM
                {
                    PaymentType = type,
                    Text = type.GetDescription(),
                    ImagePath = new BitmapImage(new Uri(CommonPath.ImageDir + imageInfo.Source, UriKind.Absolute)),
                    ImageWidth = imageInfo.Width,
                    ImageHeight = imageInfo.Height
                });
            }
        }
    }

    public class PaymentMethodButtonVM
    {
        public PaymentMethodEnum PaymentType { get; set; }
        public string Text { get; set; }
        public ImageSource ImagePath { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        public Command PaymentMethodCommand { get; set; }

        public PaymentMethodButtonVM()
        {
            PaymentMethodCommand = new Command(ClickButton);
        }

        private void ClickButton()
        {
            Messenger.Instance.Send(MessengerEnum.SelectPaymentMethod, PaymentType);
        }
    }
}
