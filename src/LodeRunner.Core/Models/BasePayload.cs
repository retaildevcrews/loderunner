using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// Base class for payload.
    /// </summary>
    public class BasePayload : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the properties changed list.
        /// </summary>
        /// <value>
        /// The changed properties.
        /// </value>
        public List<string> PropertiesChanged { get; set; } = new ();

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        protected void OnPropertyChanged([CallerMemberName] string fieldName = null)
        {
            if (!this.PropertiesChanged.Contains(fieldName))
            {
                this.PropertiesChanged.Add(fieldName);
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(fieldName));
            }
        }

        /// <summary>
        /// Sets the field on an entity based on the field.
        /// Assuming property names in this class are the same as in entity.
        /// We're using the Reflected [CallerMemberName] property, assuming we're calling this from the same property we want to change in entity.
        /// If this is called from a method, propertyName should be set explicitly.
        /// </summary>
        /// <typeparam name="T">The property value.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="propertyName">Name of the caller property.</param>
        protected virtual void SetField<T>(T value, [CallerMemberName] string propertyName = null)
        {
            throw new NotImplementedException("SetField not implemented.");
        }
    }
}
