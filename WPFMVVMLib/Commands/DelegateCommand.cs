using System;
using System.Windows.Input;

namespace WPFMVVMLib.Commands
{

    /// <summary>
    /// Simple action command, which is using to binding with buttons
    /// </summary>

    public class DelegateCommand : ICommand
    {
        private Action p;
        
        public DelegateCommand(Action p)
        {
            this.p = p;
        }

        public event EventHandler CanExecuteChanged;
        
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            p();
        }
    }
}