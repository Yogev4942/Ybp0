using System.Windows.Controls;
using System.Windows;
using ViewModels.ViewModels;

namespace ybp0.Views
{
    /// <summary>
    /// Interaction logic for TrainerRegisterView.xaml
    /// </summary>
    public partial class TrainerRegisterView : UserControl
    {
        public TrainerRegisterView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.Password = passwordBox.Password;
            }
        }
    }
}
