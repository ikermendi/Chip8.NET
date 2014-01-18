using Chip8.NET.Chip8;
using SharpGL;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.Threading;
using Chip8.NET.Mediator;
using Chip8.NET.ViewModel;

namespace Chip8.NET.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private MainViewModel Vm
        {
            get
            {
                return (MainViewModel)DataContext;
            }
        }

        public MainView()
        {
            InitializeComponent();

            DataContext = new MainViewModel();

            Messenger.Register(Notifications.DrawNotification, (o) =>
            {
                 Dispatcher.Invoke((Action)delegate()
                 {
                     Draw();
                 });
            });

            Messenger.Register(Notifications.HandleKeyNotification, (o) =>
            {
                Dispatcher.Invoke((Action)delegate()
                {
                    HandleKeys();
                });
            });

            Messenger.Register(Notifications.UpdatePauseNotification, (o) =>
            {
                bool isPaused = (bool)o;
                if(isPaused)
                {
                    PauseButton.Content = "Resume";
                }
                else
                {
                    PauseButton.Content = "Pause";
                }
            });


        }

        private void HandleKeys()
        {
            if (Keyboard.IsKeyDown(Key.D1))
                Vm.CPU.InputKey(0, true);
            else
                Vm.CPU.InputKey(0, false);

            if (Keyboard.IsKeyDown(Key.D2))
                Vm.CPU.InputKey(1, true);
            else
                Vm.CPU.InputKey(1, false);

            if (Keyboard.IsKeyDown(Key.D3))
                Vm.CPU.InputKey(2, true);
            else
                Vm.CPU.InputKey(2, false);

            if (Keyboard.IsKeyDown(Key.D4))
                Vm.CPU.InputKey(3, true);
            else
                Vm.CPU.InputKey(3, false);

            if (Keyboard.IsKeyDown(Key.Q))
                Vm.CPU.InputKey(4, true);
            else
                Vm.CPU.InputKey(4, false);

            if (Keyboard.IsKeyDown(Key.W))
                Vm.CPU.InputKey(5, true);
            else
                Vm.CPU.InputKey(5, false);

            if (Keyboard.IsKeyDown(Key.E))
                Vm.CPU.InputKey(6, true);
            else
                Vm.CPU.InputKey(6, false);

            if (Keyboard.IsKeyDown(Key.R))
                Vm.CPU.InputKey(7, true);
            else
                Vm.CPU.InputKey(7, false);

            if (Keyboard.IsKeyDown(Key.A))
                Vm.CPU.InputKey(8, true);
            else
                Vm.CPU.InputKey(8, false);

            if (Keyboard.IsKeyDown(Key.S))
                Vm.CPU.InputKey(9, true);
            else
                Vm.CPU.InputKey(9, false);

            if (Keyboard.IsKeyDown(Key.D))
                Vm.CPU.InputKey(10, true);
            else
                Vm.CPU.InputKey(10, false);

            if (Keyboard.IsKeyDown(Key.F))
                Vm.CPU.InputKey(11, true);
            else
                Vm.CPU.InputKey(11, false);

            if (Keyboard.IsKeyDown(Key.Z))
                Vm.CPU.InputKey(12, true);
            else
                Vm.CPU.InputKey(12, false);

            if (Keyboard.IsKeyDown(Key.X))
                Vm.CPU.InputKey(13, true);
            else
                Vm.CPU.InputKey(13, false);

            if (Keyboard.IsKeyDown(Key.C))
                Vm.CPU.InputKey(14, true);
            else
                Vm.CPU.InputKey(14, false);

            if (Keyboard.IsKeyDown(Key.V))
                Vm.CPU.InputKey(15, true);
            else
                Vm.CPU.InputKey(15, false);
        }

        private void Draw()
        {
            if (Vm.CPU != null)
            {
                byte[] screen = Vm.CPU.GetDisplay();
                OpenGL gl = OpenGLControl.OpenGL;

                gl.MatrixMode(OpenGL.GL_PROJECTION);
                gl.LoadIdentity();
                gl.Ortho(0, 640, 320, 0, 0, 1);
                gl.MatrixMode(OpenGL.GL_MODELVIEW);

                for (int y = 0; y < 32; ++y)
                {
                    for (int x = 0; x < 64; ++x)
                    {
                        if (screen[(y * 64) + x] == 0)
                            gl.Color(0.0f, 0.0f, 0.0f);
                        else
                            gl.Color(1.0f, 1.0f, 1.0f);

                        drawPixel(gl, x, y);
                    }
                }
            }
        }

        void drawPixel(OpenGL gl, int x, int y)
        {
            int modifier = 10;

            gl.Begin(OpenGL.GL_QUADS);
            gl.Vertex((x * modifier) + 0.0f, (y * modifier) + 0.0f, 0.0f);
            gl.Vertex((x * modifier) + 0.0f, (y * modifier) + modifier, 0.0f);
            gl.Vertex((x * modifier) + modifier, (y * modifier) + modifier, 0.0f);
            gl.Vertex((x * modifier) + modifier, (y * modifier) + 0.0f, 0.0f);
            gl.End();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".ch8";
            dlg.Filter = "Chip 8 programs (.ch8)|*.ch8";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                Messenger.Notify(Notifications.FileSelectedNotification, filename);
            }
        }

        private void InstructionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox ll = (ListBox)sender;
            ll.ScrollIntoView(ll.SelectedItem);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Messenger.UnRegister(Notifications.DrawNotification);
            Messenger.UnRegister(Notifications.FileSelectedNotification);
            Messenger.UnRegister(Notifications.UpdatePauseNotification);
            Messenger.UnRegister(Notifications.HandleKeyNotification);

            if(Vm.GameThread != null)
                Vm.GameThread.Abort();
        }
    }
}
