using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.CognitiveServices.Speech;

namespace Kiosk
{
    public class SpeechRecService : ISpeechRecognizer
    {
        private readonly string _SpeechKey;
        private readonly string _SpeechRegion;
        private readonly string _SpeechLanguage;

        private SpeechRecognizer _Recognizer;
        public bool IsRunning { get; set; }

        public event Action<string> Recognizing;
        public event Action<string> Recognized;
        public event Action<string> Error;

        public SpeechRecService(string key, string region, string language = "ko-KR")
        {
            _SpeechKey = key;
            _SpeechRegion = region;
            _SpeechLanguage = language;
        }

        public async Task StartAsync()
        {
            if (IsRunning)
                return;

            if (_Recognizer == null)
            {
                var config = SpeechConfig.FromSubscription(_SpeechKey, _SpeechRegion);
                config.SpeechRecognitionLanguage = _SpeechLanguage;

                _Recognizer = new SpeechRecognizer(config);
            }

            _Recognizer.Recognizing += OnRecognizing;
            _Recognizer.Recognized += OnRecognized;
            _Recognizer.Canceled += OnCanceled;

            // 비동기로 음성 인식 시작
            await _Recognizer.StartContinuousRecognitionAsync();
            IsRunning = true;
        }

        public async Task StopAsync()
        {
            if (!IsRunning || _Recognizer == null)
                return;

            await _Recognizer.StopContinuousRecognitionAsync();
            Cleanup();
        }

        /// <summary>
        /// 사용자가 말하는 도중에 계속 호출
        /// 아직 확정되지 않은 텍스트
        /// 예: "안녕하세..."
        /// 보통 UI에 실시간 자막처럼 표시할 때 사용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRecognizing(object sender, SpeechRecognitionEventArgs e)
        {
            Recognizing?.Invoke(e.Result.Text);
        }

        /// <summary>
        /// 사용자가 말을 끝내면 호출됨
        /// 확정된 인식 결과
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRecognized(object sender, SpeechRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                Recognized?.Invoke(e.Result.Text);
            }
        }

        /// <summary>
        /// 인식이 중단되거나 에러 발생 시 호출
        /// 원인: 네트워크 문제, 마이크 문제, 인증 실패, 사용자가 중지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCanceled(object sender, SpeechRecognitionCanceledEventArgs e)
        {
            Error?.Invoke($"{e.Reason} : {e.ErrorDetails}");
        }

        private void Cleanup()
        {
            if (_Recognizer != null)
            {
                _Recognizer.Recognizing -= OnRecognizing;
                _Recognizer.Recognized -= OnRecognized;
                _Recognizer.Canceled -= OnCanceled;
            }
            IsRunning = false;
            Dispose();
        }

        /// <summary>
        /// 장시간 미사용 및 앱 종료시 등 음성인식 리소스를 안전하게 해제할때 사용
        /// </summary>
        public void Dispose()
        {
            if (_Recognizer != null)
            {
                _Recognizer.Dispose();
                _Recognizer = null;
            }
        }
    }
}
