using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN
{
    public class Message:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _Id;
        public int Id
        {
            get { return _Id; }
            set
            {
                _Id = value;
                NotifyPropertyChanged(nameof(Id));
            }
        }
        private int _Data0;
        public int Data0
        {
            get { return _Data0; }
            set { _Data0 = value; NotifyPropertyChanged(nameof(Data0)); }
        }
        private int _Data1;
        public int Data1
        {
            get { return _Data1; }
            set { _Data1 = value; NotifyPropertyChanged(nameof(Data1)); }
        }
        private int _Data2;
        public int Data2
        {
            get { return _Data2; }
            set { _Data2 = value; NotifyPropertyChanged(nameof(Data2)); }
        }
        private int _Data3;
        public int Data3
        {
            get { return _Data3; }
            set { _Data3 = value; NotifyPropertyChanged(nameof(Data3)); }
        }
        private int _Data4;
        public int Data4
        {
            get { return _Data4; }
            set { _Data4 = value; NotifyPropertyChanged(nameof(Data4)); }
        }
        private int _Data5;
        public int Data5
        {
            get { return _Data5; }
            set { _Data5 = value; NotifyPropertyChanged(nameof(Data5)); }
        }
        private int _Data6;
        public int Data6
        {
            get { return _Data6; }
            set { _Data6 = value; NotifyPropertyChanged(nameof(Data6)); }
        }
        private int _Data7;
        public int Data7
        {
            get { return _Data7; }
            set { _Data7 = value; NotifyPropertyChanged(nameof(Data7)); }
        }
    }
}
