using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;

namespace Kiosk
{
    public static class CommonEnum
    {
        public enum CategoryEnum
        {
            [Description("치킨")]
            치킨,
            [Description("순살")]
            순살,
            [Description("사이드")]
            사이드,
            [Description("음료")]
            음료,
            [Description("주류")]
            주류
        }

        public enum PopupButtonStyleEnum
        {
            None,
            Cancel,             // 닫기
            Prev,               // 이전
            Next,               // 다음
            OK,                 // 확인
            CancelNext,         // 닫기, 다음
            CancelPrev,         // 닫기, 이전
            All,                // 닫기, 이전, 다음
            CancelOK,           // 닫기, 확인
        }

        public enum PaymentMethodEnum
        {
            [Description("카드")]
            [ImageInfo("Card.png", 80, 80)]
            Card,
            [Description("바코드")]
            [ImageInfo("Barcode.png", 80, 80)]
            Barcode,
        }

        public enum KioskScreenEnum
        {
            First,
            KioskMain,
            AlertPopup,
            CheckOrderPopup,
            SelectPaymentMethodPopup,
            PayPopup,
            PaymentSuccessPopup
        }

        public enum SpeechCommandEnum
        {
            None,
            [ToMessengerType(MessengerEnum.ClickPay)]
            Pay,
            [ToMessengerType(MessengerEnum.SelectItem)]
            Add,
            [ToMessengerType(MessengerEnum.DecreaseItem)]
            Delete,
            [ToMessengerType(MessengerEnum.ChangeCategory)]
            Category,
            [ToMessengerType(MessengerEnum.SelectPaymentMethod)]
            PaymentMethod,
            [ToMessengerType(MessengerEnum.ClearItems)]
            Clear,
            [ToMessengerType(MessengerEnum.ClickHome)]
            Home,
            [ToMessengerType(MessengerEnum.Cancel)]
            Cancel,
            [ToMessengerType(MessengerEnum.Next)]
            Next,
            [ToMessengerType(MessengerEnum.Prev)]
            Prev,
            [ToMessengerType(MessengerEnum.Confirm)]
            Confirm,
        }

        public enum MessengerEnum
        {
            SelectItem,             // 물품을 선택할시
            DecreaseItem,           // 물품을 차감할시
            RemoveItem,             // 물품을 제거할시
            IncTotalPrice,          // 총가격을 더함
            DecTotalPrice,          // 총가격을 차감
            ClearItems,             // 모든 물품들을 제거할시
            SelectCategory,         // 카테고리 선택시
            ClickPay,               // 결제 버튼 클릭시
            ClickHome,              // 홈 버튼 클릭시
            SelectPaymentMethod,    // 결제 수단 선택시
            SelectNormalOrder,      // 일반주문 선택시
            SelectVoiceOrder,       // 음성주문 선택시

            // 음성인식에 필요한 추가 메신저
            MenuNext,               // 메뉴 다음 선택시
            MenuPrev,               // 메뉴 이전 선택시
            ChangeCategory,         // 카테고리 변경시

            // 팝업창
            Cancel,                 // 닫기
            Next,                   // 다음
            Prev,                   // 이전
            Confirm,                // 확인
        }


        /*
            통신 Enum
         */

        public enum MsgIDEnum
        {
            // 송신 100번대
            DeviceInfo = 101,
            OrderDetails = 102,

            // 수신 200번대
            MenuList = 201,
        }


        /*
            Enum 확장 메서드
         */

        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field == null)
            {
                return value.ToString();
            }
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute?.Description ?? value.ToString();
        }

        public static string GetKoreanText(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field == null)
            {
                return value.ToString();
            }
            KoreanTextAttribute attribute = Attribute.GetCustomAttribute(field, typeof(KoreanTextAttribute)) as KoreanTextAttribute;
            return attribute?.Text ?? value.ToString();
        }

        public static MessengerEnum? GetMessengerType(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field == null)
            {
                throw new Exception("No Enum Field");
            }

            ToMessengerTypeAttribute attribute = Attribute.GetCustomAttribute(field, typeof(ToMessengerTypeAttribute)) as ToMessengerTypeAttribute;
            if (attribute == null)
                return null;
            else
                return attribute.MessengerType;
        }
    }
}
