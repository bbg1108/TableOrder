using Kiosk.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Kiosk
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            StartInit();
            _ = StartInitAsync();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private void StartInit()
        {
            try
            {
                InitExceptionHandling();
                AddService();
            }
            catch (Exception ex)
            {
                FileLogger.Log(ex);
            }
        }

        private async Task StartInitAsync()
        {
            try
            {
                await TcpConnAsync();
                MonitorTcpDisconn();
            }
            catch (Exception ex)
            {
                AlertPopup.Show("초기화 실패", ex.Message + "\n프로그램이 종료됩니다");
                FileLogger.Log(ex);
                Shutdown();
            }
        }

        private void AddService()
        {
            var serviceCollection = new ServiceCollection();
            var speechKey = ConfigurationManager.AppSettings["AzureSpeechKey"];
            var speechRegion = ConfigurationManager.AppSettings["AzureSpeechRegion"];

            // 서비스 등록
            serviceCollection.AddSingleton<ISpeechRecognizer>(sp => new SpeechRecService(speechKey, speechRegion));
            serviceCollection.AddSingleton<SpeechProcessor>();

            // 뷰모델 등록
            serviceCollection.AddTransient<ViewModels.VoiceInteractionViewModel>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private async Task TcpConnAsync()
        {
            string ip = ConfigurationManager.AppSettings["ServerIP"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);

            await TcpComm.Instance.ConnectAsync(ip, port);
            await SendTableInfoAsync();
        }

        private async Task SendTableInfoAsync()
        {
            int num = Convert.ToInt32(ConfigurationManager.AppSettings["TableNo"]);
            DataManager.instance.TableNo = num;

            DeviceInfo info = new DeviceInfo(num);
            await TcpComm.Instance.SendAsync(info);
        }

        private void MonitorTcpDisconn()
        {
            TcpComm.Instance.Disconnected += ShowReconnectServerAsync;
        }

        private async void ShowReconnectServerAsync(string message)
        {
            IAlertPopupViewModel popup = null;
            int connCnt = 3;

            _ = Dispatcher.InvokeAsync(() =>
            {
                popup = AlertPopup.Show("연결 끊김",
                    message + " 자동으로 재연결을 시도하고 있습니다. 잠시만 기다려 주세요.",
                    CommonEnum.PopupButtonStyleEnum.None);
            });

            await Task.Run(async () =>
            {
                while (connCnt > 0)
                {
                    try
                    {
                        await TcpConnAsync();
                        await Dispatcher.InvokeAsync(() =>
                        {
                            popup.RequestClosePopup();
                        });
                        break;
                    }
                    catch (System.Net.Sockets.SocketException ex)   // 연결 실패
                    {
                        connCnt--;
                        await Task.Delay(3000);
                        FileLogger.Log(ex);
                    }
                    catch (Exception ex)
                    {
                        FileLogger.Log(ex);
                    }
                }
            });

            if (connCnt == 0)
            {
                _ = Dispatcher.InvokeAsync(() =>
                {
                    AlertPopup.Show("알림", " 서버 연결시도 횟수 초과, 관리자에게 문의해주세요.\n 닫기 버튼을 누르면 서버 연결을 다시 시도합니다.");
                    popup.RequestClosePopup();
                    ShowReconnectServerAsync(message);
                });
            }
        }

        private void InitExceptionHandling()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs ex)
        {
            FileLogger.Log(ex.Exception, "UI Thread Exception");
            ex.Handled = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs ex)
        {
            FileLogger.Log((Exception)ex.ExceptionObject, "Fatal Exception");
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs ex)
        {
            FileLogger.Log(ex.Exception, "Unobserved Task Exception");
            ex.SetObserved();
        }
    }
}
