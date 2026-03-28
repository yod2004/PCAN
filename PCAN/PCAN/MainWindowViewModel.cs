using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Concurrent; // 複数の作業員が同時に触っても安全な「箱」を使います
using System.Windows.Threading;      // タイマーを使うために必要です

namespace PCAN
{
    public class MainWindowViewModel:INotifyPropertyChanged
    {
        private ConcurrentQueue<Message> _receiveBuffer = new ConcurrentQueue<Message>();

        // ▼ ② 画面を定期的に更新するためのタイマー
        private DispatcherTimer _displayTimer;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private MainManager mainmanager = new MainManager();

        private int _TxBpsIndex;
        public int TxBpsIndex
        {
            get { return _TxBpsIndex; }
            set
            {
                _TxBpsIndex = value;
                NotifyPropertyChanged(nameof(TxBpsIndex));
            }
        }

        private bool _IsTxStandardId = true;
        public bool IsTxStandardId
        {
            get { return _IsTxStandardId; }
            set
            {
                _IsTxStandardId = value;
                NotifyPropertyChanged(nameof(IsTxStandardId));
            }
        }

        private int _TxId;
        public int TxId
        {
            get { return _TxId; }
            set
            {
                _TxId = value;
                NotifyPropertyChanged(nameof(TxId));
            }
        }

        private int _TxData0;
        public int TxData0
        {
            get { return _TxData0; }
            set
            {
                if (value < 0 || 255 < value) value = 0;
                _TxData0 = value;
                NotifyPropertyChanged(nameof(TxData0));
            }
        }
        private int _TxData1;
        public int TxData1
        {
            get { return _TxData1; }
            set
            {
                if (value < 0 || 255 < value) value = 0;
                _TxData1 = value;
                NotifyPropertyChanged(nameof(TxData1));
            }
        }
        private int _TxData2;
        public int TxData2
        {
            get { return _TxData2; }
            set
            {
                if (value < 0 || 255 < value) value = 0;
                _TxData2 = value;
                NotifyPropertyChanged(nameof(TxData2));
            }
        }
        private int _TxData3;
        public int TxData3
        {
            get { return _TxData3; }
            set
            {
                if (value < 0 || 255 < value) value = 0;
                _TxData3 = value;
                NotifyPropertyChanged(nameof(TxData3));
            }
        }
        private int _TxData4;
        public int TxData4
        {
            get { return _TxData4; }
            set
            {
                if (value < 0 || 255 < value) value = 0;
                _TxData4 = value;
                NotifyPropertyChanged(nameof(TxData4));
            }
        }
        private int _TxData5;
        public int TxData5
        {
            get { return _TxData5; }
            set
            {
                if (value < 0 || 255 < value) value = 0;
                _TxData5 = value;
                NotifyPropertyChanged(nameof(TxData5));
            }
        }
        private int _TxData6;
        public int TxData6
        {
            get { return _TxData6; }
            set
            {
                if (value < 0 || 255 < value) value = 0;
                _TxData6 = value;
                NotifyPropertyChanged(nameof(TxData6));
            }
        }
        private int _TxData7;
        public int TxData7
        {
            get { return _TxData7; }
            set
            {
                if (value < 0 || 255 < value) value = 0;
                _TxData7 = value;
                NotifyPropertyChanged(nameof(TxData7));
            }
        }
        private int _TxFreeLevel;
        public int TxFreeLevel
        {
            get { return _TxFreeLevel; }
            set
            {
                _TxFreeLevel = value;
                NotifyPropertyChanged(nameof(TxFreeLevel));
            }
        }
        private int _TxAck;
        public int TxAck
        {
            get { return _TxAck; }
            set
            {
                _TxAck = value;
                NotifyPropertyChanged(nameof(TxAck));
            }
        }
        private int _RxBpsIndex;
        public int RxBpsIndex
        {
            get { return _RxBpsIndex; }
            set
            {
                _RxBpsIndex = value;
                NotifyPropertyChanged(nameof(RxBpsIndex));
            }
        }

        private bool _IsRxStandardId = true;
        public bool IsRxStandardId
        {
            get { return _IsRxStandardId; }
            set
            {
                _IsRxStandardId = value;
                NotifyPropertyChanged(nameof(IsRxStandardId));
            }
        }

        private int _RxFilterId;
        public int RxFilterId
        {
            get { return _RxFilterId; }
            set
            {
                _RxFilterId = value;
                NotifyPropertyChanged(nameof(RxFilterId));
            }
        }
        private int _RxMaskId;
        public int RxMaskId
        {
            get { return _RxMaskId; }
            set
            {
                _RxMaskId = value;
                NotifyPropertyChanged(nameof(RxMaskId));
            }
        }
        private double _RxFrequency;
        public double RxFrequency
        {
            get { return _RxFrequency; }
            set
            {
                _RxFrequency = value;
                NotifyPropertyChanged(nameof(RxFrequency));
            }
        }

        public DelegateCommand TxPushButtonCommand => new DelegateCommand(TxPushButton);
        public void TxPushButton()
        {
            int[] sendData = new int[]
            {
                TxData0, TxData1, TxData2, TxData3,
                TxData4, TxData5, TxData6, TxData7
            };
            mainmanager.SendMessage(IsTxStandardId, TxId, sendData);
        }

        public DelegateCommand ReloadComCommand => new DelegateCommand(ReloadCom);
        public void ReloadCom()
        {
            string[] ports = mainmanager.GetAvailablePorts();
            AvailablePorts.Clear();
            foreach (string port in ports)
            {
                AvailablePorts.Add(port);
            }
            if (AvailablePorts.Count > 0)
            {
                SelectedPort = AvailablePorts[0];
            }
        }
        public DelegateCommand ConnectComCommand => new DelegateCommand(ConnectCom);
        public void ConnectCom()
        {
            if (!string.IsNullOrEmpty(SelectedPort))
            {
                mainmanager.Connect(SelectedPort);
            }
        }

        public FastObservableCollection<Message> RxDataList { get; set; }

        public ObservableCollection<string> AvailablePorts { get; set; }
        private string _SelectedPort;
        public string SelectedPort
        {
            get { return _SelectedPort; }
            set
            {
                _SelectedPort = value;
                NotifyPropertyChanged(nameof(SelectedPort));
            }
        }
        private int TimerLoopCount = 0;
        // ▼ ③ タイマーによって 0.05秒 ごとに呼ばれる「画面更新」の処理
        private void OnDisplayTimerTick(object sender, EventArgs e)
        {
            TimerLoopCount++;
            if (_receiveBuffer.IsEmpty) return;

            var newMessages = new List<Message>();
            while(_receiveBuffer.TryDequeue(out Message msg))
            {
                newMessages.Add(msg);
            }
            RxFrequency = newMessages.Count / (0.2 * TimerLoopCount);
            TimerLoopCount = 0;
            var combinedList = RxDataList.ToList();
            combinedList.AddRange(newMessages);
            if(combinedList.Count > 30)
            {
                combinedList = combinedList.Skip(combinedList.Count - 30).ToList();
            }
            RxDataList.Clear();
            RxDataList.AddRange(combinedList);
        }

        public MainWindowViewModel()//コンストラクタ
        {
            RxDataList = new FastObservableCollection<Message>();
            RxDataList.Add(new Message { Id = 15, Data0 = 0, Data1 = 1, Data2 = 2, Data3 = 3, Data4 = 4, Data5 = 5, Data6 = 6, Data7 = 7 });
            AvailablePorts = new ObservableCollection<string>();
            
            // --- タイマーの準備 ---
            _displayTimer = new DispatcherTimer();
            _displayTimer.Interval = TimeSpan.FromMilliseconds(200); // 50ミリ秒(0.05秒)ごとに画面を更新！
            _displayTimer.Tick += OnDisplayTimerTick;
            _displayTimer.Start();

            mainmanager.MessageReceived += (newMessage) =>
            {
                // 【超重要】ここでは画面を直接いじりません！
                // 代わりに、受け取ったデータを「一時保管庫」に投げ込むだけにしてすぐ終わらせます。
                _receiveBuffer.Enqueue(newMessage);
            };
            ReloadCom();
        }
    }
}
