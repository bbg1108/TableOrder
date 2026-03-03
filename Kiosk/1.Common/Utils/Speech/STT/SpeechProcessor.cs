using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;
using Kiosk.ViewModels;
using Kiosk.MainWindow;
using Kiosk.Models;
using System.Text.RegularExpressions;

namespace Kiosk
{
    public class SpeechProcessor
    {
        private readonly Dictionary<SpeechCommandEnum, string[]> _Intents;
        private readonly List<string> _SortedProductNames;
        private readonly Dictionary<string, int> _KoreanNumbers;

        private Dictionary<string, int> _ProductInfo;

        public SpeechProcessor()
        {
            try
            {
                Init(out _SortedProductNames, out _KoreanNumbers, out _Intents);
            }
            catch (Exception ex)
            {
                FileLogger.Log(ex);
            }
        }

        public void SpeechCommand(string message)
        {
            try
            {
                // 특수 문자 제거
                message = NormalizeText(message);
                // 하나의 메시지에서 복합 명령 분리
                string[] commands = SplitCommand(message);

                foreach (string command in commands)
                {
                    // 먼저 메시지에서 메뉴에 들어간 상품이 있는지 확인
                    _ProductInfo = FindProduct(command);
                    bool checkMenu = _ProductInfo.Count > 0;

                    // 메시지에서 의도를 분류
                    var intent = MatchIntent(command, checkMenu);
                    var messengerType = intent.GetMessengerType();

                    // SpeechCommandEnum에 등록된 명령 값 따라 메신저 전달
                    if (messengerType != null)
                    {
                        SendMessenger(messengerType.Value, command);
                    }
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log(ex);
            }
        }

        /// <summary>
        /// 메신저를 전송하는 함수
        /// </summary>
        private void SendMessenger(MessengerEnum messengerType, string command)
        {
            try
            {
                var state = DataManager.instance.CurrentScreen;
                switch (messengerType)
                {
                    case MessengerEnum.SelectItem:
                    case MessengerEnum.DecreaseItem:
                        if (state == KioskScreenEnum.KioskMain)
                        {
                            foreach (var kvp in _ProductInfo)
                            {
                                var product = DataManager.instance.GetOneProduct(kvp.Key);
                                if (product == null)
                                    continue;

                                for (int i = 0; i < kvp.Value; i++)
                                {
                                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        Messenger.Instance.Send(messengerType, product);
                                    });
                                }
                            }
                        }
                        break;
                    case MessengerEnum.ChangeCategory:
                        if (state == KioskScreenEnum.KioskMain)
                        {
                            var categoryArray = Enum.GetValues(typeof(CategoryEnum));
                            var category = ((CategoryEnum[])categoryArray).FirstOrDefault(x => command.Contains(x.GetDescription()));
                            
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Messenger.Instance.Send(messengerType, category);
                            });
                        }
                        break;
                    case MessengerEnum.ClickPay:
                        if (state == KioskScreenEnum.KioskMain)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                if (DataManager.instance.GetSelectedOrderItems().Count == 0 || DataManager.instance.TotalPrice == 0)
                                {
                                    AlertPopup.Show("알림", "메뉴를 선택해주세요");
                                }
                                else
                                {
                                    Messenger.Instance.Send<object>(messengerType, null);
                                }
                            });
                        }
                        break;
                    case MessengerEnum.SelectPaymentMethod:
                        if (state == KioskScreenEnum.SelectPaymentMethodPopup)
                        {
                            var array = Enum.GetValues(typeof(PaymentMethodEnum)).OfType<PaymentMethodEnum>();
                            var paymentMethod = array.FirstOrDefault(x => command.Contains(x.GetDescription()));

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Messenger.Instance.Send(messengerType, paymentMethod);
                            });
                        }
                        break;
                    case MessengerEnum.Cancel:
                    case MessengerEnum.Next:
                    case MessengerEnum.Prev:
                    case MessengerEnum.Confirm:
                        if (state == KioskScreenEnum.CheckOrderPopup || state == KioskScreenEnum.SelectPaymentMethodPopup
                            || state == KioskScreenEnum.PayPopup)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Messenger.Instance.Send<object>(messengerType, null);
                            });
                        }
                        else if (state == KioskScreenEnum.AlertPopup)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Messenger.Instance.Send(messengerType, state);
                            });
                        }
                        else if (state == KioskScreenEnum.KioskMain)
                        {
                            if (messengerType == MessengerEnum.Next) messengerType = MessengerEnum.MenuNext;
                            else if (messengerType == MessengerEnum.Prev) messengerType = MessengerEnum.MenuPrev;

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Messenger.Instance.Send<object>(messengerType, null);
                            });
                        }
                        break;
                    case MessengerEnum.ClearItems:
                    case MessengerEnum.ClickHome:
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Messenger.Instance.Send<object>(messengerType, null);
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log(ex);
            }
        }

        /// <summary>
        /// 특수문자 제거 함수
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string NormalizeText(string text)
        {
            return text.Trim()
                .Replace(".", "")
                .Replace(",", "")
                .Replace("?", "")
                .Replace("!", "");
        }

        /// <summary>
        /// 메시지에서 여러개의 명령으로 분리하는 함수
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string[] SplitCommand(string message)
        {
            string[] connectors = { "이랑", "랑", "하고", "그리고", "또" };
            string pattern = string.Join("|", connectors);     // 정규식 패턴 생성
            return Regex.Split(message, $@"(?<={pattern})");
        }

        /// <summary>
        /// 메시지에서 물품 이름과 수량을 찾아주는 함수
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Dictionary<string, int> FindProduct(string message)
        {
            var result = new Dictionary<string, int>();

            // 한글을 숫자로 치환
            foreach (var kvp in _KoreanNumbers)
            {
                message = message.Replace(kvp.Key, kvp.Value.ToString());
            }

            // 수량을 나타내는 숫자의 개수
            int cnt = Regex.Matches(message, @"\d").Count;
            cnt = cnt == 0 ? 1 : cnt;

            foreach (var product in _SortedProductNames)
            {
                if (cnt == 0)
                    break;

                // 메시지에서 물품과 수량을 찾음
                var match = Regex.Match(message, $@"{product}\s*(\d+)?");
                if (match.Success)
                {
                    int quantity = 1;   // 기본 수량

                    if (!string.IsNullOrEmpty(match.Groups[1].Value))
                        quantity = int.Parse(match.Groups[1].Value);

                    result[product] = quantity;
                    message = message.Replace(product, "");
                    cnt--;
                }
            }

            return result;
        }

        /// <summary>
        /// 메시지의 의도를 매칭하여 찾아주는 함수
        /// </summary>
        /// <param name="checkMenu"></param>
        /// <returns></returns>
        private SpeechCommandEnum MatchIntent(string command, bool checkMenu)
        {
            var intent = _Intents.FirstOrDefault(x => x.Value.Any(a => command.Contains(a))).Key;

            // 사용자가 메뉴만 말하거나
            // 메뉴 이름과 같은 카테고리 선택 방지
            if (checkMenu && (intent == SpeechCommandEnum.None || intent == SpeechCommandEnum.Category))
                intent = SpeechCommandEnum.Add;

            return intent;
        }

        private void Init(out List<string> sortedProductNames, out Dictionary<string, int> koreanNumbers, out Dictionary<SpeechCommandEnum, string[]> speechKeywords)
        {
            sortedProductNames = DataManager.instance.GetAllProducts().Select(x => x.Name).OrderByDescending(o => o.Length).ToList();

            koreanNumbers = new Dictionary<string, int>()
            {
                { "하나", 1 }, { "한", 1 },
                { "둘", 2 }, { "두", 2 },
                { "셋", 3 }, { "세", 3 },
                { "넷", 4 }, { "네", 4 },
                { "다섯", 5 },
                { "여섯", 6 },
                { "일곱", 7 },
                { "여덟", 8 },
                { "아홉", 9 },
                { "열", 10 }
            };

            var json = File.ReadAllText(CommonPath.SpeechKeywordListJsonPath);
            speechKeywords = JsonConvert.DeserializeObject<Dictionary<SpeechCommandEnum, string[]>>(json);
        }
    }
}
