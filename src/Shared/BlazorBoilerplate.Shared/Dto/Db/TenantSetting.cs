using System.ComponentModel;

namespace BlazorBoilerplate.Shared.Dto.Db
{
    public partial class TenantSetting
    {
        private T GetterValue<T>()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            T result = default;

            try
            {
                result = (T)converter.ConvertFromString(Value);
            }
            catch (Exception)
            {
                Value = result.ToString();
            }

            return result;
        }

        public int ValueAsInt
        {
            get => GetterValue<int>();

            set => Value = value.ToString();
        }

        public bool ValueAsBool
        {
            get => GetterValue<bool>();

            set => Value = value.ToString();
        }
    }
}
