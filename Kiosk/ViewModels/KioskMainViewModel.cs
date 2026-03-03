using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;
using Microsoft.Extensions.DependencyInjection;

namespace Kiosk.ViewModels
{
    public class KioskMainViewModel : ViewModelBase
    {
        public bool IsVoiceMode { get; set; }

        private CategoryViewModel _CategoryViewModel;
        public CategoryViewModel CategoryViewModel { get => _CategoryViewModel; set => SetValue(ref _CategoryViewModel, value); }

        private MenuViewModel _MenuViewModel;
        public MenuViewModel MenuViewModel { get => _MenuViewModel; set => SetValue(ref _MenuViewModel, value); }

        private CartViewModel _CartViewModel;
        public CartViewModel CartViewModel { get => _CartViewModel; set => SetValue(ref _CartViewModel, value); }

        private SideBarViewModel _SideBarViewModel;
        public SideBarViewModel SideBarViewModel { get => _SideBarViewModel; set => SetValue(ref _SideBarViewModel, value); }

        private VoiceInteractionViewModel _VoiceInteractionViewModel;
        public VoiceInteractionViewModel VoiceInteractionViewModel { get => _VoiceInteractionViewModel; set => SetValue(ref _VoiceInteractionViewModel, value); }

        #region 팝업
        private bool _IsPopupOpen;
        public bool IsPopupOpen { get => _IsPopupOpen; set => SetValue(ref _IsPopupOpen, value); }

        private PopupViewModelBase _CurrentPopup;
        public PopupViewModelBase CurrentPopup { get => _CurrentPopup; set => SetValue(ref _CurrentPopup, value); }
        #endregion

        public KioskMainViewModel(bool voiceMode)
        {
            MenuViewModel = new MenuViewModel();
            CategoryViewModel = new CategoryViewModel();
            CartViewModel = new CartViewModel();
            SideBarViewModel = new SideBarViewModel();
            CurrentPopup = new PopupViewModelBase();

            IsVoiceMode = voiceMode;
            DataManager.instance.SetCurrentScreen(KioskScreenEnum.KioskMain);
            _ = InitSpeechRecognizerAsync();

            Messenger.Instance.Subscribe(MessengerEnum.ClickPay, this, (object obj) =>
            {
                InitCurrentPopup(new CheckOrderPopupViewModel());
                IsPopupOpen = true;
            });
            Messenger.Instance.Subscribe(MessengerEnum.Cancel, this, (object obj) => { OnPopupCancel(); });
            Messenger.Instance.Subscribe(MessengerEnum.Next, this, (object obj) => { OnPopupNext(); });
            Messenger.Instance.Subscribe(MessengerEnum.Prev, this, (object obj) => { OnPopupPrev(); });
        }

        /// <summary>
        /// 음성 모드일시 음성인식 사용하게 설정하는 함수
        /// </summary>
        /// <returns></returns>
        private async Task InitSpeechRecognizerAsync()
        {
            try
            {
                if (IsVoiceMode)
                {
                    VoiceInteractionViewModel = App.ServiceProvider.GetService<VoiceInteractionViewModel>();
                    await VoiceInteractionViewModel.StartSTTAsync();
                }

                TextToSpeech.Instance.Speak("원하는 메뉴를 선택하세요",
                "원하는 메뉴를 말해주세요. 다 골랐으면 결제라고 말해주세요.");
            }
            catch (Exception ex)
            {
                FileLogger.Log(ex);
            }
        }

        #region 팝업
        private void InitCurrentPopup(PopupViewModelBase popup)
        {
            SetPopupActions(popup);
            CurrentPopup = popup;
        }

        private void SetPopupActions(PopupViewModelBase popup)
        {
            popup.CancelAction = OnPopupCancel;
            popup.NextAction = OnPopupNext;
            popup.PrevAction = OnPopupPrev;
        }
        #endregion

        #region 팝업 버튼 기능 구현
        public void OnPopupCancel()
        {
            if (CurrentPopup.ShowCancel)
            {
                IsPopupOpen = false;
                CurrentPopup = null;
                DataManager.instance.SetCurrentScreen(KioskScreenEnum.KioskMain);
            }
        }

        public void OnPopupNext()
        {
            if (CurrentPopup.NextPopup != null)
                InitCurrentPopup(CurrentPopup.NextPopup);
        }

        public void OnPopupPrev()
        {
            if (CurrentPopup.PrevPopup != null)
                InitCurrentPopup(CurrentPopup.PrevPopup);
        }
        #endregion
    }
}
