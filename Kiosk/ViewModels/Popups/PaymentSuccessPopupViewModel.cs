using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk.ViewModels
{
    public class PaymentSuccessPopupViewModel : PopupViewModelBase
    {
        private int _TimeCount;
        public int TimeCount { get => _TimeCount; set => SetValue(ref _TimeCount, value); }

        public PaymentSuccessPopupViewModel() : base("결제 성공")
        {
            DataManager.instance.SetCurrentScreen(KioskScreenEnum.PaymentSuccessPopup);
            TimeCount = 5;
            _ = GoHomeAsync();
        }

        private async Task GoHomeAsync()
        {
            try
            {
                while (TimeCount > 0)
                {
                    await Task.Delay(1000);
                    TimeCount--;
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log(ex);
            }
            finally
            {
                Messenger.Instance.Send<object>(MessengerEnum.ClickHome, null);
            }
        }
    }
}
