using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;

namespace Kiosk
{
    public class TextToSpeech
    {
        private static TextToSpeech _instance;
        private static readonly object _lock = new object();
        private SpeechSynthesizer speechSynthesizer;
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
            speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.SetOutputToDefaultAudioDevice();
            speechSynthesizer.SelectVoice("Microsoft Heami Desktop");
        }

        public async Task SpeakAsync(string text)
        {
            DataManager.instance.IsTTSSpeaking = true;

            var prompt = speechSynthesizer.SpeakAsync(text);

            if (DataManager.instance.IsVoiceMode)
                Speaking.Invoke(text);

            while (!prompt.IsCompleted)
            {
                await Task.Delay(50);
            }

            DataManager.instance.IsTTSSpeaking = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text1">일반주문시 텍스트</param>
        /// <param name="text2">음성주문시 텍스트</param>
        /// <returns></returns>
        public async Task SpeakAsync(string text1, string text2)
        {
            DataManager.instance.IsTTSSpeaking = true;

            string text = DataManager.instance.IsVoiceMode ? text2 : text1;
            var prompt = speechSynthesizer.SpeakAsync(text);

            if (DataManager.instance.IsVoiceMode)
                Speaking.Invoke(text);

            while (!prompt.IsCompleted)
            {
                await Task.Delay(50);
            }

            DataManager.instance.IsTTSSpeaking = false;
        }
    }
}
