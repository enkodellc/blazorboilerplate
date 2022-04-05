using ObjectCloner.Extensions;
using System.Reflection;

namespace BlazorBoilerplate.Shared.Dto
{
    public abstract class BaseDto : IMementoDto
    {
        private object state;

        public void SaveState()
        {
            state = this.DeepClone();
        }

        public void RestoreState()
        {
            if (state != null)
                foreach (PropertyInfo property in GetType().GetProperties().Where(p => p.CanWrite))
                    property.SetValue(this, property.GetValue(state, null), null);
        }
        public void ClearState()
        {
            state = null;
        }
    }
}
