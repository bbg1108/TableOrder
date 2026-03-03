using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk.ViewModels
{
    public class SideBarViewModel : ViewModelBase
    {
        public Command HomeButtonCommand { get; set; }

        public SideBarViewModel()
        {
            HomeButtonCommand = new Command(ClickHomeButton);
        }

        private void ClickHomeButton()
        {
            var popup = AlertPopup.Show("알림", "첫화면으로 이동하겠습니까?", PopupButtonStyleEnum.CancelOK);
            if (popup.ClickButtonValue == PopupButtonStyleEnum.OK)
                Messenger.Instance.Send<object>(MessengerEnum.ClickHome, null);
        }
    }
}
