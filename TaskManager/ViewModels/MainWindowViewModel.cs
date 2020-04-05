using TaskManager.Tools.Managers;
using TaskManager.Tools.Navigation;

namespace TaskManager.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel, IContentOwner
    {
        #region Fields
        
        private INavigatable _content;

        #endregion

        #region Properties
        
        public INavigatable Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        #endregion
        
        internal MainWindowViewModel()
        {
            NavigationManager.Instance.Initialize(new InitializationNavigationModel(this));
            NavigationManager.Instance.Navigate(ViewType.TaskGrid);
        }
    }
}