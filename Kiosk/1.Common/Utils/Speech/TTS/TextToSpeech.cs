using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Threading;

namespace Kiosk
{
    public class TextToSpeech
    {
        private static TextToSpeech _instance;
        private static readonly object _lock = new object();
        private SpeechSynthesizer _speechSynthesizer;
        private Prompt _currentPrompt;

        public event Action<string> Speaking;

        public static TextToSpeech Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new TextToSpeech();
                    }
                    return _instance;
                }
            }
        }

        private TextToSpeech()
        {
            Init();
        }

        private void Init()
        {
            _speechSynthesizer = new SpeechSynthesizer();
            _speechSynthesizer.SetOutputToDefaultAudioDevice();
            _speechSynthesizer.SelectVoice("Microsoft Heami Desktop");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text1">일반주문시 텍스트</param>
        /// <param name="text2">음성주문시 텍스트</param>
        /// <returns></returns>
        public void Speak(string text1, string text2 = null)
        {
            try
            {
                CancelCurrentSpeech();

                string text = DataManager.instance.IsVoiceMode ? text2 : text1;
                var prompt = _speechSynthesizer.SpeakAsync(text);

                DataManager.instance.IsTTSSpeaking = !prompt.IsCompleted;
                _currentPrompt = prompt;

                if (DataManager.instance.IsVoiceMode)
                    Speaking.Invoke(text);

                SubscribeSpeakCompletedEvent(prompt);
            }
            catch (Exception ex)
            {
                FileLogger.Log(ex);
            }
        }

        /// <summary>
        /// 음성이 나오는 도중에 새로운 음성이 필요한 경우 기존 음성을 취소하는 함수
        /// </summary>
        private void CancelCurrentSpeech()
        {
            if (_currentPrompt != null)
            {
                _speechSynthesizer.SpeakAsyncCancel(_currentPrompt);
            }
        }

        /// <summary>
        /// 기존 음성을 취소하는 함수인 SpeakAsyncCancel()가 실행될때
        /// 발생하는 SpeakCompleted 이벤트 핸들러를 정의
        /// </summary>
        /// <param name="cts"></param>
        private void SubscribeSpeakCompletedEvent(Prompt prompt)
        {
            EventHandler<SpeakCompletedEventArgs> handler = null;
            handler += (s, e) =>
            {
                if (e.Prompt != prompt)
                    return;

                DataManager.instance.IsTTSSpeaking = !_currentPrompt.IsCompleted;
                if (!DataManager.instance.IsTTSSpeaking)
                    _currentPrompt = null;

                _speechSynthesizer.SpeakCompleted -= handler;
            };
            _speechSynthesizer.SpeakCompleted += handler;
        }
    }
}
