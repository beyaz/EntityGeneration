using System.Threading;
using System.Windows;
using BOA.EntityGeneration.ConstantsProjectGeneration;

namespace BOA.EntityGeneration.UI.Container.ConstantsProjectGeneration.Components
{
    public partial class Container
    {
        #region Constructors
        public Container()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Properties
        public ConstantsProjectGenerationConfig Config { get; set; } = ConstantsProjectGenerationConfig.CreateFromFile();
        #endregion

        #region Methods
        void OnGenerateClicked(object sender, RoutedEventArgs e)
        {
            var checkinComment = App.Model.CheckinComment;

            if (string.IsNullOrWhiteSpace(checkinComment))
            {
                MessageBox.Show("Tfs Check-in comment girilmelidir.");
                return;
            }

            StartGeneration();
        }

        void StartGeneration()
        {
            var ui = new GenerationProcess();
            ui.ProcessCompletedSuccessfully += () =>
            {
                Dispatcher?.Invoke(() => { processContainer.Children.Remove(ui); });
            };

            ui.Margin = new Thickness(0, 10, 0, 0);

            processContainer.Children.Add(ui);

            ui.Start();
        }
        #endregion
    }
}