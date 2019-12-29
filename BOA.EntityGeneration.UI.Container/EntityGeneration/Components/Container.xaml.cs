using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace BOA.EntityGeneration.UI.Container.EntityGeneration.Components
{
    public partial class Container
    {
        #region Fields
        readonly Queue<string> generationQueue = new Queue<string>();
        #endregion

        #region Constructors
        public Container()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Properties
        public string SelectedSchemaName { get; set; }
        #endregion

        #region Methods
        void ConsumeGenerationQueue()
        {
            if (!generationQueue.Any())
            {
                return;
            }

            StartGeneration(generationQueue.Dequeue());
        }

        void OnGenerateClicked(object sender, RoutedEventArgs e)
        {
            var schemaName = SelectedSchemaName;

            if (string.IsNullOrWhiteSpace(schemaName))
            {
                MessageBox.Show("Schema Name seçilmelidir.");
                return;
            }

            var checkinComment = App.Model.CheckinComment;

            if (string.IsNullOrWhiteSpace(checkinComment))
            {
                MessageBox.Show("Tfs Check-in comment girilmelidir.");
                return;
            }

            if (schemaName == "*")
            {
                foreach (var item in App.Config.SchemaNames.Where(x => x != "*"))
                {
                    generationQueue.Enqueue(item);
                }

                // Max three thread
                ConsumeGenerationQueue();
                ConsumeGenerationQueue();
                ConsumeGenerationQueue();
                return;
            }

            StartGeneration(schemaName);
        }

        void StartGeneration(string schemaName)
        {
            var ui = new SchemaGenerationProcess(schemaName);
            ui.ProcessCompletedSuccessfully += () =>
            {
                Dispatcher?.Invoke(() => { processContainer.Children.Remove(ui); });
                Dispatcher?.Invoke(ConsumeGenerationQueue);
            };

            ui.Margin = new Thickness(0, 10, 0, 0);

            processContainer.Children.Add(ui);

            ui.Start();
        }
        #endregion
    }
}