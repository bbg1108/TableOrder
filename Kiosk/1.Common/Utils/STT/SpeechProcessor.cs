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
        private readonly Dictionary<SpeechCommandEnum, string[]> SpeechKeywords;
        private readonly List<string> SortedProductNames;
        private readonly Dictionary<string, int> KoreanNumbers;

        private Dictionary<string, int> ProductInfo;

        public SpeechProcessor()
        {
            try
            {
                Init(out SortedProductNames, out KoreanNumbers, out SpeechKeywords);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SpeechCommand(string message)
        {
            message = NormalizeText(message);
            string[] commands = SplitCommand(message);

            foreach (string command in commands)
            {
                // 먼저 메시지에서 메뉴에 들어간 상품이 있는지 확인
                ProductInfo = FindProduct(command);
                bool checkMenu = ProductInfo.Count > 0;

                // 메시지에서 키워드를 분류
                var keyword = KeywordMatch(checkMenu, command);
                var messengerType = keyword.GetMessengerType();

                // SpeechCommandEnum에 등록된 명령 값 따라 메신저 전달
                if (messengerType != null)
                {
                    SendMessengerCommand(messengerType.Value, command);
                }
            }
        }

        /// <summary>
        /// 분류된 명령에 따라 메신저를 전송하는 함수
        /// </summary>
        private void SendMessengerCommand(MessengerEnum messengerType, string command)
        {
            var state = DataManager.instance.CurrentScreen;
            switch (messengerType)
            {
                case MessengerEnum.SelectItem:
                case MessengerEnum.DecreaseItem:
                    if (state == KioskScreenEnum.KioskMain)
                    {
                        foreach (var kvp in ProductInfo)
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
                        return;
                    }
                    break;
                case MessengerEnum.SelectDivision:
                    if (state == KioskScreenEnum.KioskMain)
                    {
                        var divisionArray = Enum.GetValues(typeof(DivisionEnum));
                        var division = ((DivisionEnum[])divisionArray).FirstOrDefault(x => command.Contains(x.GetDescription()));

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Messenger.Instance.Send(messengerType, division);
                        });
                        return;
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
                        return;
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
                        return;
                    }
                    break;
                case MessengerEnum.Cancel:
                case MessengerEnum.Next:
                case MessengerEnum.Prev:
                    if (state == KioskScreenEnum.AlertPopup && messengerType == MessengerEnum.Cancel)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            var popup = App.Current.Windows.OfType<AlertPopupView>()?.FirstOrDefault(x => x.IsActive);
                            if (popup != null)
                            {
                                popup.Close();
                            }
                        });
                        return;
                    }
                    else if (state == KioskScreenEnum.CheckOrderPopup || state == KioskScreenEnum.SelectPaymentMethodPopup
                            || state == KioskScreenEnum.PayPopup)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Messenger.Instance.Send<object>(messengerType, null);
                        });
                        return;
                    }
                    break;
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
        /// 메시지에서 여러개의 명령으로 나누는 함수
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
            foreach (var kvp in KoreanNumbers)
            {
                message = message.Replace(kvp.Key, kvp.Value.ToString());
            }

            foreach (var product in SortedProductNames)
            {
                // 메뉴 + 숫자 정규식
                var match = Regex.Match(message, $@"{product}\s*(\d+)?");
                if (match.Success)
                {
                    int quantity = 1;   // 기본 수량

                    if (!string.IsNullOrEmpty(match.Groups[1].Value))
                        quantity = int.Parse(match.Groups[1].Value);

                    result[product] = quantity;
                    message = message.Replace(product, "");
                }
            }

            return result;
        }

        /// <summary>
        /// 키워드 매칭
        /// </summary>
        /// <param name="checkMenu"></param>
        /// <returns></returns>
        private SpeechCommandEnum KeywordMatch(bool checkMenu, string command)
        {
            var keyword = SpeechKeywords.FirstOrDefault(x => x.Value.Any(a => command.Contains(a))).Key;

            // 사용자가 메뉴만 말하거나
            // 메뉴 이름과 같은 카테고리 선택 방지
            if (checkMenu && (keyword == SpeechCommandEnum.None || keyword == SpeechCommandEnum.Category))
                keyword = SpeechCommandEnum.Add;

            return keyword;
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
