using System.Windows.Controls;
using System.Windows;
using ViewModels.ViewModels;

namespace ybp0.Views
{
    /// <summary>
    /// Interaction logic for TraineeRegisterView.xaml
    /// </summary>
    public partial class TraineeRegisterView : UserControl
    {
        public TraineeRegisterView()
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
