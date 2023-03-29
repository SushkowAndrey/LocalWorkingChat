using System.Threading;
using System.Windows;

namespace LocalWorkingChat
{
    /// <summary>
    /// Заставка
    /// </summary>
    public partial class Screensaver : Window
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public Screensaver()
        {
            InitializeComponent();
            Show();
            Thread.Sleep(2000);
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
    }
}