using MkvToMp4.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MkvToMp4
{
    /// <summary>
    /// Logique d'interaction pour AdvanceSetup.xaml
    /// </summary>
    public partial class AdvanceSetup : Window
    {
        private ToolConfig toolConfig;
        private ToolConfig src;
        private MainWindow parent;

        public AdvanceSetup()
        {
            InitializeComponent();
        }

        public void Init(ToolConfig tool, MainWindow main)
        {
            src = tool;
            parent = main;
            toolConfig = new ToolConfig(tool);
            FormatSelect.Text = toolConfig.Format;
            ProfileSelect.Text = toolConfig.Profile;
            PresetSelect.Text = toolConfig.Preset;
            VCodecSelect.Text = toolConfig.VCodec;
            ACodecSelect.Text = toolConfig.ACodec;
            CustomInput.Text = toolConfig.AdvanceArgs;
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            src.HideBanner = (bool)HideBanner_Check.IsChecked;
            src.Format = FormatSelect.Text;
            src.Profile = ProfileSelect.Text;
            src.Preset = PresetSelect.Text;
            src.VCodec = VCodecSelect.Text;
            src.ACodec = ACodecSelect.Text;
            src.AdvanceArgs = CustomInput.Text;

            parent.UpdateStatement();
            Close();
        }
    }
}
