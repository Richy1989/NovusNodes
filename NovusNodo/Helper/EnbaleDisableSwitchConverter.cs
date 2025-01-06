using MudBlazor;

namespace NovusNodo.Helper
{
    public class EnbaleDisableSwitchConverter : BoolConverter<string>
    {

        public EnbaleDisableSwitchConverter()
        {
            SetFunc = OnSet;
            GetFunc = OnGet;
        }

        private string TrueString = "Enabled";
        private string FalseString = "Disabled";
        private string NullString = "";

        private string OnGet(bool? value)
        {
            try
            {
                return (value == true) ? TrueString : FalseString;
            }
            catch (Exception e)
            {
                UpdateGetError("Conversion error: " + e.Message);
                return NullString;
            }
        }

        private bool? OnSet(string arg)
        {
            if (arg == null)
                return null;
            try
            {
                if (arg == TrueString)
                    return true;
                if (arg == FalseString)
                    return false;
                else
                    return null;
            }
            catch (FormatException e)
            {
                UpdateSetError("Conversion error: " + e.Message);
                return null;
            }
        }

    }
}