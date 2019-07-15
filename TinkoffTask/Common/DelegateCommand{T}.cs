using System;
using System.Windows.Input;

namespace TinkoffTask.Common
{
    public sealed class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        public bool CanExecute(T parameter) => _canExecute == null ? true : _canExecute(parameter);        

        public void Execute(T parameter)
        {
            if (CanExecute(parameter))
            {
                _execute(parameter);
            }
        }

        bool ICommand.CanExecute(object parameter) => CanExecute((T)parameter);
        void ICommand.Execute(object parameter) => Execute((T)parameter);
    }
}
