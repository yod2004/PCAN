using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PCAN
{
    public class DelegateCommand:ICommand
    {
        private readonly Action _execute;
        public DelegateCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return true; // 今回は常にボタンを押せる状態にしておきます
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
