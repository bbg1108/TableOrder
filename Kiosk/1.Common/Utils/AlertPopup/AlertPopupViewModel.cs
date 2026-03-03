using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk
{
    public class AlertPopupViewModel : PopupViewModelBase, IAlertPopupViewModel
    {
        public string Message { get; private set; }
        public PopupButtonStyleEnum ClickButtonValue { get; set; }
        public event Action ClosePopupRequested;

        public AlertPopupViewModel(string title, string message, PopupButtonStyleEnum button) : base(title, button, 500, 300)
        {
            Message = message;
            Messenger.Instance.Subscribe(MessengerEnum.Cancel, this, (KioskScreenEnum state) =>
            {
                if (state == KioskScreenEnum.AlertPopup)
                    OnCancel();
            });
            Messenger.Instance.Subscribe(MessengerEnum.Confirm, this, (KioskScreenEnum state) =>
            {
                if (state == KioskScreenEnum.AlertPopup)
                    OnOK();
            });
        }

        public void RequestClosePopup()
        {
            ClosePopupRequested.Invoke();
        }

        protected override void OnCancel()
        {
            if (ShowCancel)
            {
                base.OnCancel();
                ClickButtonValue = PopupButtonStyleEnum.Cancel;
            }
        }

        protected override void OnOK()
        {
            if (ShowOK)
            {
                base.OnOK();
                ClickButtonValue = PopupButtonStyleEnum.OK;
            }
        }

        protected override void OnNext()
        {
            base.OnNext();
            ClickButtonValue = PopupButtonStyleEnum.Next;
        }

        protected override void OnPrev()
        {
            base.OnPrev();
            ClickButtonValue = PopupButtonStyleEnum.Prev;
        }
    }
}
