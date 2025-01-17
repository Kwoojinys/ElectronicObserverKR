﻿using System;
using System.IO;
using System.Windows.Forms;

namespace Browser
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // FormBrowserHostから起動された時は引数に通信用URLが渡される
            if (args.Length == 0)
            {
                MessageBox.Show("이 프로그램은 74식 전자관측의의 서브 프로그램입니다. 단독 실행은 불가능합니다.\r\n본 프로그램을 실행해주세요.",
                    "정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Application.Run(new FormBrowser(args[0]));
        }

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                string asmname = args.Name.Split(",".ToCharArray(), 2)[0] + ".dll";
                string arch = System.IO.Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, Environment.Is64BitProcess ? "x64" : "x86", asmname);
                if (!System.IO.File.Exists(arch))
                    return null;
                try
                {
                    return System.Reflection.Assembly.LoadFile(arch);
                }
                catch (IOException ex) when (ex is FileNotFoundException || ex is FileLoadException)
                {
                    if (MessageBox.Show(
                            $@"브라우저 구성 요소를 로드 할 수 없습니다. 실행에 필요한
                            「Microsoft Visual C++ 2015 재배포 가능 패키지」
                            가 설치되어 있지 않은 것이 원인일 수 있습니다.
                            다운로드 페이지를 여시겠습니까?
                            (vc_redist.{(Environment.Is64BitProcess ? "x64" : "x86")}.exe 을 설치해주세요.)",
                        "CefSharp 로드에러", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
                        == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(@"https://www.microsoft.com/ja-jp/download/details.aspx?id=53587");
                    }
                    // なんにせよ今回は起動できないのであきらめる
                    throw;
                }       // なんにせよ今回は起動できないのであきらめる
                catch (NotSupportedException)
                {
                    // 概ね ZoneID を外し忘れているのが原因

                    if (MessageBox.Show(
                            @"브라우저 시작에 실패 했습니다.
                            설치에 필요한 작업이 수행되지 않은 것이 원인일수있습니다.
                            설치 가이드를 여시겠습니까? (원어 페이지로 연결됩니다.)",
                            "브라우저 실행 실패", MessageBoxButtons.YesNo, MessageBoxIcon.Error)
                        == DialogResult.Yes)
                        System.Diagnostics.Process.Start(@"https://github.com/andanteyk/ElectronicObserver/wiki/Install");

                }
            }
            return null;
        }
    }
}
    

