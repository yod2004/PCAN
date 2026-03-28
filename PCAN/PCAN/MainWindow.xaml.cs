using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Specialized;

namespace PCAN
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewmodel = new MainWindowViewModel();
            DataContext =  viewmodel;

            /*自動スクロール機能*/
            viewmodel.RxDataList.CollectionChanged += (sender, e) =>//リストの中身が変わった時のお知らせ（イベント）に、今から書く名無しの処理を追加（+=）します！ その処理は sender と e を受け取って（=>）この { 処理 } を実行してね！
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    var newItem = e.NewItems[0];

                    // 「画面の描画（Background）が終わったら、この処理をやってね」と予約する
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        RxDataGrid.ScrollIntoView(newItem);
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            };
        }
    }
}
