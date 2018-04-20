namespace Fridge
{
    using System;
    using System.Linq;
    using System.Reflection;

    // These constraints are about as close as we canc urrently get to limiting to numeric types.
    public abstract class ValueObject<TValue> : IComparable, IComparable<TValue>, IEquatable<TValue>
        where TValue : struct, IComparable, IComparable<TValue>, IConvertible, IEquatable<TValue>, IFormattable
    {

        public TValue Value { get; }

        public string Name { get; }

        public ValueObject(TValue value, string name)
        {
            this.Value = value;
            this.Name = name;
        }


        public static TObject FromValue<TObject>(TValue value) where TObject : ValueObject<TValue>
        {
            // Not sure of a better way at the moment to locate by value than getting each instance and checking.
            var objectType = typeof(TObject);

            var located = objectType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(prop => prop.GetValue(null, null) as TObject)
                .FirstOrDefault(obj => obj.Value.Equals(value));

            if (located == null)
                throw new InvalidOperationException($"No implementation of '{objectType.FullName}' for the value '{value}'");

            return located;
        }

        public static TObject FromName<TObject>(string name) where TObject : ValueObject<TValue>
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var objectType = typeof(TObject);

            // The assumption is the properties are static readonly similar to an enum, but with callable methods and properties.
            var prop = objectType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .FirstOrDefault(p => name.Equals(p.Name, StringComparison.OrdinalIgnoreCase) && p.PropertyType == objectType);

            // Not entirely sure if invalid operation should be used, or a custom exception should be created.
            if (prop == null)
                throw new InvalidOperationException($"The type '{objectType.FullName}' does not contain a value that resolves from '{name}'");
            
            return prop.GetValue(null, null) as TObject;
        }
        

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ValueObject<TValue>))
                return false;

            return this.Value.CompareTo(obj) == 0;
        }

        public bool Equals(TValue other)
        {
            return this.Value.Equals(other);
        }

        public int CompareTo(object obj)
        {
            return this.Value.CompareTo(obj);
        }

        public int CompareTo(TValue other)
        {
            return this.Value.CompareTo(other);
        }
    }
}
