using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using ViewModels.ViewModels;

namespace ybp0.Views
{
    public partial class ChatsView : UserControl
    {
        private INotifyCollectionChanged _messagesCollection;

        public ChatsView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += (sender, args) => ScrollToBottom();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_messagesCollection != null)
            {
                _messagesCollection.CollectionChanged -= OnMessagesCollectionChanged;
                _messagesCollection = null;
            }

            var viewModel = DataContext as ChatsViewModel;
            if (viewModel?.Messages != null)
            {
                _messagesCollection = viewModel.Messages;
                _messagesCollection.CollectionChanged += OnMessagesCollectionChanged;
            }

            ScrollToBottom();
        }

        private void OnMessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new System.Action(ScrollToBottom));
        }

        private void ScrollToBottom()
        {
            MessagesScrollViewer?.ScrollToEnd();
        }
    }
}
