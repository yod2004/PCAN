using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCAN
{
    public class FastObservableCollection<T> : ObservableCollection<T>
    {
        private bool _IsSuppressing = false;//trueなら画面通知しない
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_IsSuppressing)//falseなら通知
            {
                base.OnCollectionChanged(e);
            }
        }
        public void AddRange(IEnumerable<T> items)
        {
            _IsSuppressing = true;
            foreach(var item in items)
            {
                Add(item);
            }
            _IsSuppressing = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
