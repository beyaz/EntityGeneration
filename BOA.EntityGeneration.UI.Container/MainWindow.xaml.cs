using System.ComponentModel;

namespace BOA.EntityGeneration.UI.Container
{
    public partial class MainWindow
    {
        #region Constructors
        public MainWindow()
        {
            InitializeComponent();

            DataContext =  App.Model;
            Closing     += OnClose;
        }
        #endregion

        #region Properties
        MainWindowModel Model => (MainWindowModel) DataContext;
        #endregion

        #region Methods
        void OnClose(object sender, CancelEventArgs e)
        {
            CheckInCommentAccess.SaveCheckInComment(Model.CheckinComment);
        }
        #endregion
    }
}