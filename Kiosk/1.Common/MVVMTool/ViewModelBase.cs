using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Kiosk.CommonEnum;

namespace Kiosk
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }
    }

    public class PopupViewModelBase : ViewModelBase
    {
        public string Title { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public bool ShowCancel { get; private set; }
        public bool ShowOK { get; private set; }
        public bool ShowNext { get; private set; }
        public bool ShowPrev { get; private set; }

        public Command CancelCommand { get; set; }
        public Command OKCommand { get; set; }
        public Command NextCommand { get; set; }
        public Command PrevCommand { get; set; }

        public Action CancelAction { get; set; }
        public Action OKAction { get; set; }
        public Action NextAction { get; set; }
        public Action PrevAction { get; set; }

        public PopupViewModelBase PrevPopup { get; set; }
        public PopupViewModelBase NextPopup { get; set; }

        public PopupViewModelBase(string title = null, PopupButtonStyleEnum popupButtonStyle = PopupButtonStyleEnum.None, int width = 900, int height = 600)
        {
            InitPopup(title, width, height);
            InitButtonStyle(popupButtonStyle);
            InitCommands();
        }

        private void InitPopup(string title, int width, int height)
        {
            Title = title ?? "";
            Width = width;
            Height = height;
        }

        private void InitButtonStyle(PopupButtonStyleEnum popupButtonStyle)
        {
            switch (popupButtonStyle)
            {
                case PopupButtonStyleEnum.None:
                    break;
                case PopupButtonStyleEnum.Cancel:
                    ShowCancel = true;
                    break;
                case PopupButtonStyleEnum.Prev:
                    ShowPrev = true;
                    break;
                case PopupButtonStyleEnum.Next:
                    ShowNext = true;
                    break;
                case PopupButtonStyleEnum.CancelNext:
                    ShowCancel = true;
                    ShowNext = true;
                    break;
                case PopupButtonStyleEnum.CancelPrev:
                    ShowCancel = true;
                    ShowPrev = true;
                    break;
                case PopupButtonStyleEnum.All:
                    ShowCancel = true;
                    ShowNext = true;
                    ShowPrev = true;
                    break;
                case PopupButtonStyleEnum.CancelOK:
                    ShowCancel = true;
                    ShowOK = true;
                    break;
                default:
                    break;
            }
        }

        private void InitCommands()
        {
            CancelCommand = ShowCancel ? new Command(OnCancel) : null;
            OKCommand = ShowOK ? new Command(OnOK) : null;
            NextCommand = ShowNext ? new Command(OnNext) : null;
            PrevCommand = ShowPrev ? new Command(OnPrev) : null;
        }

        protected virtual void OnCancel() { CancelAction?.Invoke(); }
        protected virtual void OnOK() { OKAction?.Invoke(); }
        protected virtual void OnNext() { NextAction?.Invoke(); }
        protected virtual void OnPrev() { PrevAction?.Invoke(); }
    }
}
