namespace BlazorBoilerplate.Shared.Models
{
    public class SelectItem<T> : IEquatable<SelectItem<T>>
    {
        public T Id { get; set; }

        public bool Selected { get; set; }

        public string DisplayValue { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as SelectItem<T>);
        }

        public bool Equals(SelectItem<T> other)
        {
            return other != null &&
                   Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public override string ToString()
        {
            return DisplayValue ?? " - ";
        }

        public static bool operator ==(SelectItem<T> left, SelectItem<T> right)
        {
            return EqualityComparer<SelectItem<T>>.Default.Equals(left, right);
        }

        public static bool operator !=(SelectItem<T> left, SelectItem<T> right)
        {
            return !(left == right);
        }
    }
}
