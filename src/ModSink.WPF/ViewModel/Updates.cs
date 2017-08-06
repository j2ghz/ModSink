using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModSink.WPF.ViewModel
{
    public class Updates : ObservableObject
    {
        private readonly ObservableCollection<string> _history = new ObservableCollection<string>();
        private readonly TextConverter _textConverter = new TextConverter(s => s.ToUpper());
        private string _someText;

        public ICommand ConvertTextCommand
        {
            get { return new DelegateCommand(ConvertText); }
        }

        public IEnumerable<string> History
        {
            get { return _history; }
        }

        public string SomeText
        {
            get { return _someText; }
            set
            {
                _someText = value;
                RaisePropertyChangedEvent("SomeText");
            }
        }

        private void AddToHistory(string item)
        {
            if (!_history.Contains(item))
                _history.Add(item);
        }

        private void ConvertText()
        {
            if (string.IsNullOrWhiteSpace(SomeText)) return;
            AddToHistory(_textConverter.ConvertText(SomeText));
            SomeText = string.Empty;
        }
    }
}