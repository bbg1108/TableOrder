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
            return true;
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke();
            _executeObj?.Invoke(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
