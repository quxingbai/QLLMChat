using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QLLMChat.Helpers
{
    public class ActionCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private Action<object> _ExecuteAction = null;
        private Func<object, bool> _CanExecute = null;

        public ActionCommand(Action<object> executeAction, Func<object, bool>? canExecute = null)
        {
            _ExecuteAction = executeAction;
            _CanExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _CanExecute == null ? true : _CanExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _ExecuteAction(parameter);
        }
        public void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
