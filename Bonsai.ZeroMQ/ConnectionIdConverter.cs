using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Bonsai.ZeroMQ
{
    class ConnectionIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(InstanceDescriptor) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string connectionString = value as string;
            if (connectionString != null)
            {
                return new ConnectionId(connectionString);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            object result = null;
            ConnectionId connectionId = value as ConnectionId;

            if (connectionId != null)
            {
                if (destinationType == typeof(string))
                    result = connectionId.ToString();
                else if (destinationType == typeof(InstanceDescriptor))
                {
                    ConstructorInfo constructorInfo;

                    constructorInfo = typeof(ConnectionId).GetConstructor(new[] { typeof(SocketSettings.SocketConnection), typeof(string), typeof(string) });
                    result = new InstanceDescriptor(constructorInfo, new object[] { connectionId.SocketConnection, connectionId.Host, connectionId.Port });
                }
            }

            return result ?? base.ConvertTo(context, culture, value, destinationType);
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            return new ConnectionId(
                (SocketSettings.SocketConnection)propertyValues["SocketConnection"],
                (SocketSettings.SocketProtocol)propertyValues["SocketProtocol"],
                (string)propertyValues["Host"], 
                (string)propertyValues["Port"]);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(value, attributes);
        }

        class ConnectionPropertyDescriptor : SimplePropertyDescriptor
        {
            readonly PropertyDescriptor descriptor;

            public ConnectionPropertyDescriptor(string name, PropertyDescriptor descr, Attribute[] attributes)
                : base(descr.ComponentType, name, descr.PropertyType, attributes)
            {
                descriptor = descr;
            }

            public override object GetValue(object component)
            {
                return descriptor.GetValue(component);
            }

            public override void SetValue(object component, object value)
            {
                descriptor.SetValue(component, value);
            }
        }
    }
}