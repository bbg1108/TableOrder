using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk.ViewModels
{
    public class FirstViewModel : ViewModelBase
    {
        public Command NormalOrderCommand { get; set; }
        public Command VoiceOrderCommand { get; set; }

        public FirstViewModel()
        {
            DataManager.instance.SetCurrentScreen(KioskScreenEnum.First);

            NormalOrderCommand = new Command(NormalOrder);
            VoiceOrderCommand = new Command(VoiceOrder);
        }

        private void NormalOrder()
        {
            Messenger.Instance.Send<object>(MessengerEnum.SelectNormalOrder, null);
        }

        private void VoiceOrder()
        {
            Messenger.Instance.Send<object>(MessengerEnum.SelectVoiceOrder, null);
        }
    }
}
