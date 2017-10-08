using System;
using System.Windows.Input;

namespace Hpe.Nga.Octane.VisualStudio
{
    /// <summary>
    /// Implement ICommandHandler which delegate the Execute to a delegate.
    /// </summary>
    public class DelegatedCommand : ICommand
    {
        private readonly Action<object> handler;
        private bool canExecute;

        public DelegatedCommand(Action<object> handler)
        {
            this.handler = handler;
            canExecute = true;
        }

        public event EventHandler CanExecuteChanged;

        public void SetCanExecute(bool value)
        {
            if (canExecute == value)
            {
                return;
            }

            canExecute = value;

            if (CanExecuteChanged != null)
            {
                CanExecuteChanged.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute;
        }

        public void Execute(object parameter)
        {
            handler(parameter);
        }
    }
}
