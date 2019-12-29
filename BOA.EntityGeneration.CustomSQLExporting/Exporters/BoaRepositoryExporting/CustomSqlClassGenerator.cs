using BOA.EntityGeneration.CustomSQLExporting.ContextManagement;
using DotNetStringUtilities;

namespace BOA.EntityGeneration.CustomSQLExporting.Exporters.BoaRepositoryExporting
{
    class CustomSqlClassGenerator : ContextContainer
    {
        #region Fields
        public readonly PaddedStringBuilder sb = new PaddedStringBuilder();
        #endregion

        #region Public Methods
        public void AttachEvents()
        {
            Context.ProfileInfoInitialized += Begin;

            Context.CustomSqlInfoInitialized += WriteSwitchCaseCondition;

            Context.ProfileInfoRemove += End;
        }
        #endregion

        #region Methods
        void Begin()
        {
            sb.AppendLine("public static class CustomSql");
            sb.OpenBracket();

            sb.AppendLine("public static TOutput Execute<TOutput, T>(ObjectHelper objectHelper, ICustomSqlProxy<TOutput, T> input) where TOutput : GenericResponse<T>");
            sb.OpenBracket();

            sb.AppendLine("switch (input.Index)");
            sb.OpenBracket();
        }

        void End()
        {
            sb.CloseBracket(); // end of switch

            sb.AppendLine();
            sb.AppendLine("throw new InvalidOperationException(input.GetType().FullName);");

            sb.CloseBracket(); // end of method

            sb.CloseBracket(); // end of class
        }

        void WriteSwitchCaseCondition()
        {
            sb.AppendLine($"case {CustomSqlInfo.SwitchCaseIndex}:");
            sb.OpenBracket();
            sb.AppendLine($"return (TOutput) (object) new {NamingMap.RepositoryClassName}(objectHelper.Context).Execute(({NamingMap.InputClassName})(object) input);");
            sb.CloseBracket();
        }
        #endregion
    }
}