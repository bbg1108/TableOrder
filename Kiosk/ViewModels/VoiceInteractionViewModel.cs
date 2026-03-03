using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kiosk.ViewModels
{
    public class VoiceInteractionViewModel : ViewModelBase
    {
        #region 음성인식 기능 목록
        private readonly ISpeechRecognizer _SpeechRecognizer;
        private readonly SpeechProcessor _SpeechProcessor;
        private CancellationTokenSource _Cts;

        private string _STTMessage;
        public string STTMessage { get => _STTMessage; set => SetValue(ref _STTMessage, value); }

        private string _TTSMessage;
        public string TTSMessage { get => _TTSMessage; set => SetValue(ref _TTSMessage, value); }
        #endregion

        public VoiceInteractionViewModel(ISpeechRecognizer speechRecognizer, SpeechProcessor speechProcessor)
        {
            _SpeechRecognizer = speechRecognizer;
            _SpeechProcessor = speechProcessor;
        }

        public async Task StartSTTAsync()
        {
            SubscribeEvent();
            await _SpeechRecognizer.StartAsync();
            DataManager.instance.IsVoiceMode = _SpeechRecognizer.IsRunning;
        }

        public async Task StopSTTAsync()
        {
            UnSubscribeEvent();
            await _SpeechRecognizer.StopAsync();
            DataManager.instance.IsVoiceMode = _SpeechRecognizer.IsRunning;
        }

        private void SubscribeEvent()
        {
            _SpeechRecognizer.Recognizing += OnRecognizing;
            _SpeechRecognizer.Recognized += OnRecognized;
            _SpeechRecognizer.Error += OnSTTError;
            TextToSpeech.Instance.Speaking += TTS_Speaking;
        }

        private void UnSubscribeEvent()
        {
            _SpeechRecognizer.Recognizing -= OnRecognizing;
            _SpeechRecognizer.Recognized -= OnRecognized;
            _SpeechRecognizer.Error -= OnSTTError;
            TextToSpeech.Instance.Speaking -= TTS_Speaking;
        }

        private void TTS_Speaking(string text)
        {
            _ = App.Current.Dispatcher.InvokeAsync(() =>
            {
                TTSMessage = text;
            });
        }

        private void OnRecognizing(string text)
        {
            if (!DataManager.instance.IsTTSSpeaking)
            {
                _ = App.Current.Dispatcher.InvokeAsync(() =>
                {
                    STTMessage = text;
                });

                _Cts?.Cancel();
                _Cts?.Dispose();
                _Cts = new CancellationTokenSource();
                _ = RemoveRecognizedTextAsync(_Cts.Token);
            }
        }

        private void OnRecognized(string text)
        {
            if (!DataManager.instance.IsTTSSpeaking)
            {
                if (text.Equals(""))
                    return;

                OnRecognizing(text);
                _SpeechProcessor.SpeechCommand(text);
            }
        }

        private void OnSTTError(string message)
        {
            FileLogger.Log(new Exception(message), "SpeechRecognizer Exception");
        }

        private async Task RemoveRecognizedTextAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(5000, token);
                _ = App.Current.Dispatcher.InvokeAsync(() =>
                {
                    STTMessage = null;
                });
            }
            catch (TaskCanceledException)
            {
                // 정상적인 취소 (CancellationToken.Cancel)
            }
        }
    }
}
