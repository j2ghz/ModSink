using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModSink.WPF.ViewModel
{
    public abstract class Command<T> : Microsoft.Practices.Composite.Presentation.Commands.DelegateCommand<T>
    {
        public Command()
        {
        }

        public event EventHandler CanExecuteChanged;

        public abstract bool CanExecute(T parameter);

        public abstract void Execute(T parameter);
    }
}