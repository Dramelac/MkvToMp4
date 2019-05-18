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
        public delegate void EndProcessCallback();

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
            Proc_running = null;
            if (!File.Exists("ffmpeg.exe"))
            {
                MessageBox.Show("FFmpeg is not present.\nDownload and place ffmpeg.exe in the same folder of this tool.\n\nhttps://ffmpeg.org/download.html","FFmpeg is missing", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown(1);
            }
        }

        private void Select_inputfile_click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = false,
                DefaultExt = ".mkv",
                Filter = "Video MKV (*.mkv)|*.mkv"
            };

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();

            if (result.HasValue && result == true)
            {
                this.toolConfig.InputFile = openFileDlg.FileName;
                InputSelectFile.Text = openFileDlg.SafeFileName;
                InputSetuped = true;
            }
            else
            {
                InputSelectFile.Text = "Please select correct input file";
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
            } else
            {
                MessageBox.Show("Please complete the required fields", "Missing information", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private async void ExecTool(string args=null)
        {
            if (IsRunning)
            {
                MessageBox.Show("A task is already in progress.", "Operation not possible", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            StartProcess();
            try
            {
                Proc_running = new Process();
                Process process = Proc_running;
                await Task.Run(() =>
                {
                    process.StartInfo.FileName = "ffmpeg.exe";
                    if (!String.IsNullOrEmpty(args))
                    {
                        process.StartInfo.Arguments = args;
                    }
                    else
                    {
                        process.StartInfo.Arguments = toolConfig.GenerateArguments();
                    }
                    ResultTextBox.Dispatcher.Invoke(
                        new UpdateOutputCallback(this.UpdateResultText),
                        string.Format("[INFO] Running : ./ffmpeg.exe {0}", process.StartInfo.Arguments)
                        );

                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                    {
                        if (!String.IsNullOrEmpty(e.Data))
                        {
                            ResultTextBox.Dispatcher.Invoke(
                                new UpdateOutputCallback(this.UpdateResultText),
                                e.Data);
                        }
                    });
                    process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                    {
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

                    process = null;

                    KillButton.Dispatcher.Invoke(new EndProcessCallback(this.EndProcess));
                    ResultTextBox.Dispatcher.Invoke(
                                    new UpdateOutputCallback(this.UpdateResultText),
                                    "[INFO] Task terminated");
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Execution exception", MessageBoxButton.OK,MessageBoxImage.Error);
                IsRunning = false;
            }
        }

        private void UpdateResultText(string msg)
        {
            Regex regex = new Regex(@"^frame=[\s\d]*fps=");
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

        private void StartProcess()
        {
            IsRunning = true;
            ResultTextBox.Text = "";
            ProgressTextBox.Text = "";
            KillButton.IsEnabled = true;
            
            ConvertButton.IsEnabled = false;
            ConvertButton.Content = "Running...";
        }

        private void EndProcess()
        {
            IsRunning = false;
            KillButton.IsEnabled = false;
            
            ConvertButton.IsEnabled = true;
            UpdateStatement();
        }

        private void KillProcess(object sender, RoutedEventArgs e)
        {
            if (Proc_running != null)
            {
                Proc_running.StandardInput.WriteLine("q");
            } else
            {
                MessageBox.Show("No tasks in progress", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void ClearLogs(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Text = "";
            ProgressTextBox.Text = "";
        }

        private void ViewCommand(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format("./ffmpeg.exe {0}", toolConfig.GenerateArguments()), "Prepared statement", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
