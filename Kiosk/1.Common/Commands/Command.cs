using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Kiosk
{
    public class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private Action _execute;
        private Action<object> _executeObj;

        public Command(Action execute)
        {
            _execute = execute;
        }

        public Command(Action<object> parameter)
        {
            _executeObj = parameter;
        }

        public bool CanExecute(object parameter)
        {
            //if (parameter != null)
            //    CanExecuteChanged?.Invoke(parameter, new EventArgs()); // 임시 작성, 인터넷 찾아보면서 수정 필요
            return true;
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke();
            _executeObj?.Invoke(parameter);
        }
    }
}
