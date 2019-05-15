using MkvToMp4.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MkvToMp4
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool InputSetuped;
        private bool OutputSetuped;

        private bool IsRunning;
        public delegate void UpdateOutputCallback(string message);

        private ToolConfig toolConfig;

        private Process Proc_running { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            toolConfig = new ToolConfig();
            IsRunning = false;
            InputSetuped = false;
            OutputFileName.Text = "output";
            OutputSetuped = true;
            if (!File.Exists("ffmpeg.exe"))
            {
                MessageBox.Show("FFmpeg is not present.\nDownload and place ffmpeg.exe in the same folder of this tool.\n\nhttps://ffmpeg.org/download.html","ERROR");
                Application.Current.Shutdown(1);
            }
        }

        private void Select_inputfile_click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.Multiselect = false;
            openFileDlg.DefaultExt = ".mkv";
            openFileDlg.Filter = "Video MKV (*.mkv)|*.mkv";

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();

            if (result == true)
            {
                this.toolConfig.InputFile = openFileDlg.FileName;
                InputSelectFile.Text = openFileDlg.FileName;
                InputSetuped = true;
            }
            else
            {
                InputSelectFile.Text = "Please select correct file";
                InputSetuped = false;
            }
            UpdateStatement();
        }

        public void UpdateStatement()
        {
            if (InputSetuped && OutputSetuped)
            {
                ViewCommandButton.IsEnabled = true;
                ConvertButton.IsEnabled = true;
            }
            else
            {
                ViewCommandButton.IsEnabled = false;
                ConvertButton.IsEnabled = false;
            }
            ConvertButton.Content = "Convert to " + toolConfig.Format.ToUpper();
        }

        private void UpdateOutput(object sender, RoutedEventArgs e)
        {
            OutputSetuped = !String.IsNullOrEmpty(OutputFileName.Text);
            toolConfig.OutputFile = OutputFileName.Text;
            UpdateStatement();
        }

        private void Convert(object sender, RoutedEventArgs e)
        {
            if (InputSetuped && OutputSetuped)
            {
                ExecTool();
            }
        }

        private async void ExecTool(string args=null)
        {
            if (IsRunning)
            {
                MessageBox.Show("A task is already in progress.","Operation not possible");
                return;
            }

            IsRunning = true;
            ResultTextBox.Text = "";
            try
            {
                await Task.Run(() =>
                {
                    using (Process process = new Process())
                    {
                        //KillButton.IsEnabled = true;
                        process.StartInfo.FileName = "ffmpeg.exe";
                        if (!String.IsNullOrEmpty(args))
                        {
                            process.StartInfo.Arguments = args;
                        }
                        else
                        {
                            process.StartInfo.Arguments = toolConfig.GenerateArguments();
                        }
                        //ResultTextBox.Text = string.Format("Running : ./ffmpeg.exe {0}\nPlease wait ... (Might be long)\n", process.StartInfo.Arguments);
                        ResultTextBox.Dispatcher.Invoke(
                            new UpdateOutputCallback(this.UpdateResultText),
                            string.Format("[INFO] Running : ./ffmpeg.exe {0}\nPlease wait ... (Might be long)", process.StartInfo.Arguments)
                            );

                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.CreateNoWindow = true;

                        process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                        {
                            // Prepend line numbers to each line of the output.
                            if (!String.IsNullOrEmpty(e.Data))
                            {
                                ResultTextBox.Dispatcher.Invoke(
                                    new UpdateOutputCallback(this.UpdateResultText),
                                    e.Data);
                            }
                        });
                        process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                        {
                            // Prepend line numbers to each line of the output.
                            if (!String.IsNullOrEmpty(e.Data))
                            {
                                ResultTextBox.Dispatcher.Invoke(
                                    new UpdateOutputCallback(this.UpdateResultText),
                                    e.Data);
                            }
                        });

                        process.Start();

                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        process.WaitForExit();
                    }
                    //Proc_running = null;
                    //KillButton.IsEnabled = false;
                    ResultTextBox.Dispatcher.Invoke(
                                    new UpdateOutputCallback(this.UpdateResultText),
                                    "[INFO] Task terminated");
                    IsRunning = false;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Execution exception");
                IsRunning = false;
            }
        }

        private void UpdateResultText(string msg)
        {
            Regex regex = new Regex(@"^frame=[\s\d]*fps=[\s\d.]*q=[\s\d.]*size=[\s\d]*[\w]+[\s]+time=[\d:.]+[\s]+bitrate=[\s]*[\d.]+[\w\/]+[\s]+speed=[\s]*[\d.x]+[\s]+$");
            if (regex.Match(msg).Success)
            {
                ProgressTextBox.Text = msg;
            }
            else
            {
                ResultTextBox.Text += msg + "\n";
                ResultScroll.ScrollToBottom();
            }
        }

        private void RunHelp(object sender, RoutedEventArgs e)
        {
            ExecTool("-h");
        }

        private void OpenSetting(object sender, RoutedEventArgs e)
        {
            AdvanceSetup setting = new AdvanceSetup();
            setting.Init(toolConfig, this);
            setting.Show();
        }

        private void KillProcess(object sender, RoutedEventArgs e)
        {
            if (Proc_running != null)
            {
                Proc_running.Kill();
                KillButton.IsEnabled = false;
            }
        }

        private void ClearLogs(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Text = "";
        }

        private void ViewCommand(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format("./ffmpeg.exe {0}", toolConfig.GenerateArguments()), "Prepared statement");
        }
    }
}
