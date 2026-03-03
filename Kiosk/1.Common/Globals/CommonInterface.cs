using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kiosk.CommonEnum;

namespace Kiosk
{
    /// <summary>
    /// 외부에서 음성인식 리소스에 접근할때 사용하는 인터페이스
    /// </summary>
    public interface ISpeechRecognizer
    {
        bool IsRunning { get; set; }        // 음성인식 리소스가 켜져 있는지 상태
        event Action<string> Recognizing;   // 중간 결과
        event Action<string> Recognized;    // 최종 결과
        event Action<string> Error;         // 에러 이벤트

        Task StartAsync();                  // 비동기 실행
        Task StopAsync();                   // 비동기 종료
    }

    /// <summary>
    /// 외부에서 AlertPopupViewModel에 접근할때 사용하는 인터페이스
    /// </summary>
    public interface IAlertPopupViewModel
    {
        PopupButtonStyleEnum ClickButtonValue { get; set; }     // 팝업창에서 선택한 버튼 정보

        void RequestClosePopup();                               // 팝업창 닫기 요청
    }
}
