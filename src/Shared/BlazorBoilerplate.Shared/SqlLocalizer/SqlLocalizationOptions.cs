namespace BlazorBoilerplate.Shared.SqlLocalizer
{
    public class SqlLocalizationOptions
    {
        /// <summary>
        /// If UseOnlyPropertyNames is false, this property can be used to define keys with full type names or just the name of the class
        /// </summary>
        public bool UseTypeFullNames { get; set; }

        /// <summary>
        /// This can be used to use only property names to find the keys
        /// </summary>
        public bool UseOnlyPropertyNames { get; set; }

        /// <summary>
        /// Returns only the Key if the value is not found. If set to false, the search key in the database is returned.
        /// </summary>
        public bool ReturnOnlyKeyIfNotFound { get; set; }

        /// <summary>
        /// Creates a new item in the SQL database if the resource is not found
        /// </summary>
        public bool CreateNewRecordWhenLocalisedStringDoesNotExist { get; set; }

        /// <summary>
        /// You can set the required properties to set, get, display the different localization
        /// </summary>
        /// <param name="useTypeFullNames"></param>
        /// <param name="useOnlyPropertyNames"></param>
        /// <param name="returnOnlyKeyIfNotFound"></param>
        /// <param name="createNewRecordWhenLocalisedStringDoesNotExist"></param>
        public void UseSettings(bool useTypeFullNames, bool useOnlyPropertyNames, bool returnOnlyKeyIfNotFound, bool createNewRecordWhenLocalisedStringDoesNotExist)
        {
            UseTypeFullNames = useTypeFullNames;
            UseOnlyPropertyNames = useOnlyPropertyNames;
            ReturnOnlyKeyIfNotFound = returnOnlyKeyIfNotFound;
            CreateNewRecordWhenLocalisedStringDoesNotExist = createNewRecordWhenLocalisedStringDoesNotExist;
        }
    }
}
