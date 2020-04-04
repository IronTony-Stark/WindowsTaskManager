using TaskManager.Tools.Managers;
using TaskManager.Tools.Navigation;

namespace TaskManager.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel, IContentOwner
    {
        #region Fields
        
        private INavigatable _content;
        private bool _isControlEnabled = true;
        
        #endregion

        #region Properties
        
        public INavigatable Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsControlEnabled
        {
            get => _isControlEnabled;
            set
            {
                _isControlEnabled = value;
                OnPropertyChanged();
            }
        }

        #endregion
        
        public MainWindowViewModel()
        {
            NavigationManager.Instance.Initialize(new InitializationNavigationModel(this));
            NavigationManager.Instance.Navigate(ViewType.TaskGrid);
        }
    }
}