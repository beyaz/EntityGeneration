using System;
using System.Collections;
using System.Linq;
using WpfControls;

namespace BOA.EntityGeneration.UI.Container.Components
{
    public class ProfileNameComponent : AutoCompleteTextBox, ISuggestionProvider
    {
        #region Constructors
        public ProfileNameComponent()
        {
            Provider = this;
        }
        #endregion

        #region Methods
        IEnumerable ISuggestionProvider.GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return null;
            }

            return App.Config.ProfileNames.Where(name => name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        #endregion
    }
}