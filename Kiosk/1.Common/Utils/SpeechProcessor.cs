using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk
{
    public class SpeechProcessor<T> where T : Enum
    {
        private readonly Dictionary<T, string[]> SpeechKeywords;

        public SpeechProcessor()
        {
            try
            {
                var json = File.ReadAllText(CommonPath.SpeechKeywordListJsonPath);
                SpeechKeywords = JsonConvert.DeserializeObject<Dictionary<T, string[]>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SpeechCommand(string text)
        {
            string nText = Normalize(text);

            // 음성 명령에 따른 키워드를 먼저 분류
            var command = SpeechKeywords.FirstOrDefault(x => x.Value.Any(a => nText.Contains(a))).Key;

            // SpeechCommandEnum에 등록된 명령 값 따라 메신저 전달
            //SendCommand(command);
            SendMessengerCommands(command, nText);
        }

        /// <summary>
        /// 특수문자 제거 함수
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string Normalize(string text)
        {
            return text.Trim()
                .Replace(".", "")
                .Replace(",", "")
                .Replace("?", "")
                .Replace("!", "");
        }

        /// <summary>
        /// 분류된 명령에 따라 메신저를 전송하는 함수
        /// (많은 작업이 필요할 것으로 예상)
        /// </summary>
        private void SendMessengerCommands(T command, string text)
        {
            var state = DataManager.instance.CurrentKioskState;
            //var messengerType = (MessengerEnum)Enum.Parse(typeof(MessengerEnum), command.GetDescription());
            var messengerType = command.GetMessengerType();

            switch (messengerType)
            {
                case MessengerEnum.NoSpeechKeyword:
                    break;
                case MessengerEnum.SelectItem:
                    if (state == KioskStateEnum.KioskMain)
                    {
                        var products = DataManager.instance.GetAllProducts();
                        var product = products.OrderByDescending(o => o.Name).FirstOrDefault(x => text.Contains(x.Name));

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Messenger.Instance.Send(messengerType, product);
                        });
                    }
                    break;
                case MessengerEnum.SelectDivision:
                    if (state == KioskStateEnum.KioskMain)
                    {
                        var divisionArray = Enum.GetValues(typeof(DivisionEnum));
                        var division = ((DivisionEnum[])divisionArray).FirstOrDefault(x => text.Contains(x.GetKoreanText()));

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Messenger.Instance.Send(messengerType, division);
                        });
                    }
                    break;
            }
        }
    }
}
