﻿﻿namespace TaskManager.Tools.Navigation
{
    internal enum ViewType
    {
        TaskGrid
    }

    internal interface INavigationModel
    {
        void Navigate(ViewType viewType);
    }
}
