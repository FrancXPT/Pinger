using System;
using IWshRuntimeLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.IO;
using System.Drawing;
using System.Windows.Controls.Primitives;
using Pinger;

namespace Gauge
{
    /// <summary>
    /// Interaction logic for UserControlGauge.xaml
    /// </summary>
    public partial class UserControlGauge : UserControl
    {
        public UserControlGauge()
        {
            InitializeComponent();
            chkStartUp.IsChecked = false;
            CheckIfItsExiste();


            BeginStoryboard_Click(this, null);
            res.Text = "0";
            gaugeAn.Angle = -85;
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                /* run your code here */
                while (true)
                {
                    DoEvents();
                    Thread.Sleep(15);
                    this.Dispatcher.Invoke(() =>
                    {
                        PingIt();
                    });
                }

            }).Start();
        }
        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                                  new Action(delegate { }));
        }
        public static int PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;
            int repll = 0;
            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
                repll = Convert.ToInt32(reply.RoundtripTime);
            }
            catch (PingException x)
            {
                Console.WriteLine(x.Message);
                return 0;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return repll;
        }
        private void PingIt()
        {
            //double value = PingHost("www.google.com");
            //double end = (value / 100) * 17;

            //res.Text = end.ToString();
            //gaugeAn.Angle = Convert.ToInt32(end) - 85;


            double value = PingHost("www.google.com");
            double end = (value / 100) * 17;
            
            res.Text = value.ToString();
            gaugeAn.Angle = Convert.ToInt32(end) - 85;
            Taskkk.ToolTipText = value.ToString();
            popupText.Text = value.ToString();
            
            if (value > 0 && value <= 200)
            {
                Taskkk.IconSource = (ImageSource)Resources["Connected"]; ;
            }
            else if(value >200 && value <=500)
            {
                Taskkk.IconSource = (ImageSource)Resources["connectedish"]; ;
            }
            else
            {
                Taskkk.IconSource = (ImageSource)Resources["disconnected"]; ;
            }
        }
      
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }



        private void Minimuze_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Application.Current.MainWindow;
            parentWindow.Hide();
        }

        
        private void Show_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Application.Current.MainWindow;
            parentWindow.Show();
        }
        public void CreateStartupFolderShortcut()
        {
            WshShell wshShell = new WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut;
            string startUpFolderPath =
              Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            // Create the shortcut
            shortcut =
              (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(
                startUpFolderPath + "\\" +
                "Pinger.lnk");

            shortcut.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location; ;
            shortcut.WorkingDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location; 
            shortcut.Description = "Launch My Application";
            // shortcut.IconLocation = Application.StartupPath + @"\App.ico";
            shortcut.Save();
        }

        public string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = Path.GetFileName(shortcutFilename);

            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder folder = shell.NameSpace(pathOnly);
            Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link =
                  (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }

            return String.Empty; // Not found
        }
        public void CheckIfItsExiste()
        {
            string startUpFolderPath =
              Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            DirectoryInfo di = new DirectoryInfo(startUpFolderPath);
            FileInfo[] files = di.GetFiles("*.lnk");

            foreach (FileInfo fi in files)
            {
                string shortcutTargetFile = GetShortcutTargetFile(fi.FullName);
                string targetExeName = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                if (shortcutTargetFile.EndsWith(targetExeName,
                      StringComparison.InvariantCultureIgnoreCase))
                {
                    chkStartUp.IsChecked = true;
                }
            }
        }
        public void DeleteStartupFolderShortcuts(string targetExeName)
        {
            string startUpFolderPath =
              Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            DirectoryInfo di = new DirectoryInfo(startUpFolderPath);
            FileInfo[] files = di.GetFiles("*.lnk");

            foreach (FileInfo fi in files)
            {
                string shortcutTargetFile = GetShortcutTargetFile(fi.FullName);

                if (shortcutTargetFile.EndsWith(targetExeName,
                      StringComparison.InvariantCultureIgnoreCase))
                {
                    System.IO.File.Delete(fi.FullName);
                }
            }
        }

        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Application.Current.MainWindow;

            
            parentWindow.Hide();
        }

        private void ChkStartUp_Checked(object sender, RoutedEventArgs e)
        {
            CreateStartupFolderShortcut();
        }

        private void ChkStartUp_Unchecked(object sender, RoutedEventArgs e)
        {
            DeleteStartupFolderShortcuts(Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        }

        FancyBalloon balloon = null;
        private void BeginStoryboard_Click(object sender, RoutedEventArgs e)
        {
            Taskkk.CloseBalloon();
            balloon = new FancyBalloon();
            balloon.BalloonText = "Pinger";
            //show balloon and close it after 4 seconds
            Taskkk.ShowCustomBalloon(balloon, PopupAnimation.Slide, 2000);
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            BeginStoryboard_Click(this, null);
        }
    }
}
