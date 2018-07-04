using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModSink.Common.Models.Repo
{
    [Serializable]
    public class Modpack : INotifyPropertyChanged
    {
        private bool selected;
        public ICollection<ModEntry> Mods { get; set; }
        public string Name { get; set; }

        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnPropertyChanged();
            }
        }

        //public ICollection<IServer> Servers { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}