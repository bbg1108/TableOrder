using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.CognitiveServices.Speech;

namespace Kiosk
{
    public interface ISpeechRecognizer
    {
        event Action<string> Recognizing;   // 중간 결과
        event Action<string> Recognized;    // 최종 결과
        event Action<string> Error;

        Task StartAsync();
        Task StopAsync();
    }

    public class SpeechRecService : ISpeechRecognizer
    {
        private readonly string SpeechKey;
        private readonly string SpeechRegion;
        private readonly string SpeechLanguage;

        private SpeechRecognizer Recognizer;
        private bool IsRunning;

        public event Action<string> Recognizing;
        public event Action<string> Recognized;
        public event Action<string> Error;

        public SpeechRecService(string key, string region, string language = "ko-KR")
        {
            SpeechKey = key;
            SpeechRegion = region;
            SpeechLanguage = language;
        }

        public async Task StartAsync()
        {
            if (IsRunning)
                return;
            try
            {
                if (Recognizer == null)
                {
                    var config = SpeechConfig.FromSubscription(SpeechKey, SpeechRegion);
                    config.SpeechRecognitionLanguage = SpeechLanguage;

                    Recognizer = new SpeechRecognizer(config);
                }

                Recognizer.Recognizing += OnRecognizing;
                Recognizer.Recognized += OnRecognized;
                Recognizer.Canceled += OnCanceled;

                // 비동기로 음성 인식 시작
                await Recognizer.StartContinuousRecognitionAsync();
                IsRunning = true;
            }
            catch (Exception ex)
            {
                Error?.Invoke(ex.ToString());
            }
        }

        public async Task StopAsync()
        {
            if (!IsRunning || Recognizer == null)
                return;

            try
            {
                await Recognizer.StopContinuousRecognitionAsync();
            }
            catch (Exception ex)
            {
                Error?.Invoke(ex.ToString());
            }
            finally
            {
                Cleanup();
            }
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
            if (Recognizer != null)
            {
                Recognizer.Recognizing -= OnRecognizing;
                Recognizer.Recognized -= OnRecognized;
                Recognizer.Canceled -= OnCanceled;
            }
            IsRunning = false;
        }

        /// <summary>
        /// 장시간 미사용 및 앱 종료시 등 음성인식 리소스를 안전하게 해제할때 사용
        /// </summary>
        public void Dispose()
        {
            Cleanup();
            if (Recognizer != null)
            {
                Recognizer.Dispose();
                Recognizer = null;
            }
        }
    }
}
