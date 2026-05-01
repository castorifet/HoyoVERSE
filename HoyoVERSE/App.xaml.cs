using System.Windows;
using System.Windows.Threading;

namespace HoyoVERSE
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            base.OnStartup(e);
        }

        // Without this the app dies silently on a UI-thread exception. Show the
        // actual error so the failure is debuggable instead of "it just crashed".
        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                MessageBox.Show(
                    e.Exception.ToString(),
                    "Unhandled error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch { }
            e.Handled = true;
        }
    }
}
