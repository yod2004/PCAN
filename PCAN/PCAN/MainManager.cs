using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Controls;

namespace PCAN
{
    internal class MainManager
    {
        public string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        private SerialPort _serialPort;
        public event Action<Message> MessageReceived;

        public void Connect(string portName)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                }

                _serialPort = new SerialPort(portName, 115200);

                // 💡 罠1への対策：STM32(仮想COM)に「準備OK」の合図を出す魔法のスイッチ！
                _serialPort.DtrEnable = true;
                _serialPort.RtsEnable = true;

                // 💡 罠2への対策：改行コードをSTM32側の '\r' に合わせる！
                // これがないと ReadLine() が \n を永遠に待ち続けてフリーズします
                _serialPort.NewLine = "\r";

                // イベントの登録（これを書き忘れていると絶対に反応しません）
                _serialPort.DataReceived += OnDataReceived;

                _serialPort.Open();
                System.Diagnostics.Debug.WriteLine($"{portName} を開きました！");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"接続エラー: {ex.Message}");
            }
        }
        public void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string rawData = _serialPort.ReadLine();
                rawData = rawData.Trim();
                Message newMessage = ParseSlcanMessage(rawData);
                if(newMessage!= null)
                {
                    MessageReceived?.Invoke(newMessage);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"受信エラー:{ex.Message}");
            }
        }

        private Message ParseSlcanMessage(string rawData)
        {
            if (string.IsNullOrWhiteSpace(rawData) || rawData.Length < 5) return null;
            char type = rawData[0];
            int idLength = (type == 't') ? 3 : (type == 'T') ? 8 : 0;
            if (idLength == 0) return null;
            try
            {
                var msg = new Message();
                string idStr = rawData.Substring(1, idLength);
                msg.Id = Convert.ToInt32(idStr, 16);

                string dlcStr = rawData.Substring(1 + idLength, 1);
                int dlc = Convert.ToInt32(dlcStr, 16);

                int dataStartIndex = 1 + idLength + 1;
                if (dlc >= 1 && rawData.Length >= dataStartIndex + 2) msg.Data0 = Convert.ToInt32(rawData.Substring(dataStartIndex, 2), 16);
                if (dlc >= 2 && rawData.Length >= dataStartIndex + 4) msg.Data1 = Convert.ToInt32(rawData.Substring(dataStartIndex+2, 2), 16);
                if (dlc >= 3 && rawData.Length >= dataStartIndex + 6) msg.Data2 = Convert.ToInt32(rawData.Substring(dataStartIndex + 4, 2), 16);
                if (dlc >= 4 && rawData.Length >= dataStartIndex + 8) msg.Data3 = Convert.ToInt32(rawData.Substring(dataStartIndex + 6, 2), 16);
                if (dlc >= 5 && rawData.Length >= dataStartIndex + 10) msg.Data4 = Convert.ToInt32(rawData.Substring(dataStartIndex + 8, 2), 16);
                if (dlc >= 6 && rawData.Length >= dataStartIndex + 12) msg.Data5 = Convert.ToInt32(rawData.Substring(dataStartIndex + 10, 2), 16);
                if (dlc >= 7 && rawData.Length >= dataStartIndex + 14) msg.Data6 = Convert.ToInt32(rawData.Substring(dataStartIndex + 12, 2), 16);
                if (dlc >= 8 && rawData.Length >= dataStartIndex + 16) msg.Data7 = Convert.ToInt32(rawData.Substring(dataStartIndex + 14, 2), 16);
                return msg;
            }
            catch
            {
                return null;
            }
        }

        public void SendMessage(bool isStandardId, int id, int[] data)
        {
            if (_serialPort == null || !_serialPort.IsOpen) return;
            try
            {
                char type = isStandardId ? 't':'T';
                string idStr = isStandardId ? id.ToString("X3") : id.ToString("X8");
                int dlc = data.Length;
                string dlcStr = dlc.ToString("X1");
                string dataStr = "";
                foreach(int b in data)
                {
                    dataStr += b.ToString("X2");
                }
                string command = $"{type}{idStr}{dlcStr}{dataStr}\r";
                _serialPort.Write(command);
                System.Diagnostics.Debug.WriteLine($"送信した文字列:{command}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"送信エラー:{ex.Message}");
            }
        }
    }
}
