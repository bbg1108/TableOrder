using Kiosk.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static Kiosk.CommonEnum;

namespace Kiosk.MainWindow
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _CurrntViewModel;
        public ViewModelBase CurrntViewModel { get => _CurrntViewModel; set => SetValue(ref _CurrntViewModel, value); }

        public MainWindowViewModel()
        {
            CurrntViewModel = new FirstViewModel();

            Messenger.Instance.Subscribe(MessengerEnum.SelectNormalOrder, this, (object obj) => { NormalOrder(); });
            Messenger.Instance.Subscribe(MessengerEnum.SelectVoiceOrder, this, (object obj) => { VoiceOrder(); });
            Messenger.Instance.Subscribe(MessengerEnum.ClickHome, this, (object obj) => { _ = SetClickHome(); });
        }

        private async Task SetClickHome()
        {
            try
            {
                if (DataManager.instance.IsVoiceMode)
                {
                    await (CurrntViewModel as KioskMainViewModel)?.VoiceInteractionViewModel?.StopSTTAsync();
                }
                CurrntViewModel = new FirstViewModel();
                DataManager.instance.ClearSelectedOrderItems();
            }
            catch (Exception ex)
            {
                FileLogger.Log(ex);
            }
        }

        private void NormalOrder()
        {
            CurrntViewModel = new KioskMainViewModel(false);
        }

        private void VoiceOrder()
        {
            CurrntViewModel = new KioskMainViewModel(true);
        }
    }
}
