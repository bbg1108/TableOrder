using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Kiosk.CommonEnum;

namespace Kiosk
{
    public static class AlertPopup
    {
        public static IAlertPopupViewModel Show(string title, string message, PopupButtonStyleEnum button = PopupButtonStyleEnum.Cancel)
        {
            var popup = new AlertPopupView();
            var vm = new AlertPopupViewModel(title, message, button);

            InitVM(popup, vm);
            InitPopup(popup, vm);
            DataManager.instance.SetCurrentScreen(KioskScreenEnum.AlertPopup);

            if (button == PopupButtonStyleEnum.None)
                popup.Show();
            else
                popup.ShowDialog();

            return vm;
        }

        private static void InitPopup(AlertPopupView popup, AlertPopupViewModel vm)
        {
            popup.ContentRendered += (s, e) =>
            {
                var owner = App.Current.MainWindow;
                popup.Left = owner.Left + ((owner.ActualWidth - popup.ActualWidth) / 2);
                popup.Top = owner.Top + ((owner.ActualHeight - popup.ActualHeight) / 2);
            };
            popup.DataContext = vm;
        }

        private static void InitVM(Window popup, AlertPopupViewModel vm)
        {
            if (vm.ShowCancel)
                vm.CancelAction = () => InitPopupClose(popup);

            if (vm.ShowOK)
                vm.OKAction = () => InitPopupClose(popup);

            vm.ClosePopupRequested += () => { InitPopupClose(popup); };
        }

        private static void InitPopupClose(Window popup)
        {
            popup.Close();
            DataManager.instance.SetCurrentScreen(DataManager.instance.PreviousScreen);
        }
    }
}
