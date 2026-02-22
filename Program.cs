using Photino.NET;
using System;
using System.Runtime.InteropServices; // 💡 DWM API 호출용

namespace GEKImoeStreamNT
{
    class Program
    {
        // 💡 Windows 네이티브 모서리 제어 API (Win 11 전용)
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [STAThread]
        static void Main(string[] args)
        {
            // 💡 창 사이즈 변수 (이전 WinForms 사이즈 완벽 반영)
            int windowWidth = 400;
            int windowHeight = 920;

            var window = new PhotinoWindow()
                .SetTitle("Gekimoe Stream NT")
                .SetUseOsDefaultSize(false)
                .SetUseOsDefaultLocation(false) // 💡 자동 위치 배정을 끄고 수동 계산
                .SetSize(windowWidth, windowHeight)
                .SetChromeless(true) // 💡 타이틀바 없는 깔끔한 폼 (구. FormBorderStyle.None)
                .SetResizable(false)

                // 💡 창이 생성되는 시점에 위치와 모서리를 조각합니다.
                .RegisterWindowCreatedHandler((sender, e) =>
                {
                    var win = (PhotinoWindow)sender!;

                    // 1️⃣ 창 위치를 '중앙 맨 오른쪽'으로 이동!
                    var monitor = win.MainMonitor;
                    int x = monitor.WorkArea.Width - windowWidth; // 맨 오른쪽 끝
                    int y = (monitor.WorkArea.Height - windowHeight) / 2; // 세로 중앙
                    win.SetLocation(new System.Drawing.Point(x, y));

                    // 2️⃣ Win11/10 모서리 둥글기 분기 처리
                    // 빌드 번호 22000 이상이면 Windows 11입니다.
                    if (Environment.OSVersion.Version.Build >= 22000)
                    {
                        int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
                        int DWMWCP_ROUND = 2; // 둥근 모서리 강제 적용
                        DwmSetWindowAttribute(win.WindowHandle, DWMWA_WINDOW_CORNER_PREFERENCE, ref DWMWCP_ROUND, sizeof(int));
                    }
                })

                .RegisterWebMessageReceivedHandler((object? sender, string message) =>
                {
                    var win = (PhotinoWindow)sender!;
                    Console.WriteLine($"[C# 로그] JS에서 받은 명령: {message}");

                    // ... 처리 로직 ...
                })

                // 💡 HTML 파일 이름 확인 (main.html로 변경)
                .Load("wwwroot/main.html");

            window.WaitForClose();
        }
    }
}