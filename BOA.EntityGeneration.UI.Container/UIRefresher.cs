using System.Timers;
using System.Windows;

namespace BOA.EntityGeneration.UI.Container
{
    class UIRefresher
    {
        #region Fields
        Timer timer = new Timer();
        #endregion

        #region Public Properties
        public FrameworkElement Element { get; set; }
        #endregion

        #region Public Methods
        public void Start()
        {
            timer         =  new Timer(100);
            timer.Elapsed += OnTimedEvent;
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }
        #endregion

        #region Methods
        void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Element.Dispatcher?.Invoke(RefreshDataContext);
        }

        void RefreshDataContext()
        {
            var temp = Element.DataContext;
            Element.DataContext = null;
            Element.DataContext = temp;
        }
        #endregion
    }
}