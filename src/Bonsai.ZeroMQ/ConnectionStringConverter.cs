using System;
using System.Collections;
using System.ComponentModel;

namespace Bonsai.ZeroMQ
{
    internal class ConnectionStringConverter : StringConverter
    {
        const char BindPrefix = '@';
        const char ConnectPrefix = '>';
        const string ProtocolDelimiter = "://";

        static char? GetDefaultAction(object value)
        {
            if (value is string connectionString && connectionString.Length > 0)
            {
                return connectionString[0];
            }

            return null;
        }

        static string GetProtocol(object value)
        {
            if (value is string connectionString)
            {
                var trimString = connectionString.Trim(BindPrefix, ConnectPrefix);
                var separatorIndex = trimString.IndexOf(ProtocolDelimiter);
                if (separatorIndex >= 0)
                {
                    return trimString.Substring(0, separatorIndex);
                }
            }

            return string.Empty;
        }

        static string GetAddress(object value)
        {
            if (value is string connectionString)
            {
                var separatorIndex = connectionString.IndexOf(ProtocolDelimiter);
                return separatorIndex >= 0
                    ? connectionString.Substring(separatorIndex + ProtocolDelimiter.Length)
                    : connectionString;
            }

            return string.Empty;
        }

        static char? GetActionString(Action? value)
        {
            return value switch
            {
                Action.Connect => ConnectPrefix,
                Action.Bind => BindPrefix,
                _ => null
            };
        }

        static string GetProtocolString(Protocol value)
        {
            return value switch
            {
                Protocol.InProc => "inproc",
                Protocol.Tcp => "tcp",
                Protocol.Ipc => "ipc",
                Protocol.Pgm => "pgm",
                Protocol.Epgm => "epgm",
                _ => string.Empty
            };
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            var action = (Action?)propertyValues[nameof(Action)];
            var protocol = (Protocol)propertyValues[nameof(Protocol)];
            var address = propertyValues["Address"];
            return $"{GetActionString(action)}{GetProtocolString(protocol)}{ProtocolDelimiter}{address}";
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(new PropertyDescriptor[]
            {
                new ActionDescriptor(),
                new ProtocolDescriptor(),
                new AddressDescriptor()
            }).Sort(new[] { nameof(Action), nameof(Protocol) });
        }

        class ActionDescriptor : SimplePropertyDescriptor
        {
            public ActionDescriptor()
                : base(typeof(string), nameof(Action), typeof(Action?))
            {
            }

            public override Type PropertyType => typeof(string);

            public override TypeConverter Converter => TypeDescriptor.GetConverter(typeof(Action?));

            public override object GetValue(object component)
            {
                return GetDefaultAction(component) switch
                {
                    BindPrefix => Action.Bind,
                    ConnectPrefix => Action.Connect,
                    _ => null
                };
            }

            public override void SetValue(object component, object value)
            {
            }
        }

        class ProtocolDescriptor : SimplePropertyDescriptor
        {

            public ProtocolDescriptor()
                : base(typeof(string), nameof(Protocol), typeof(Protocol))
            {
            }

            public override TypeConverter Converter => TypeDescriptor.GetConverter(typeof(Protocol));

            public override object GetValue(object component)
            {
                return GetProtocol(component) switch
                {
                    "inproc" => Protocol.InProc,
                    "tcp" => Protocol.Tcp,
                    "ipc" => Protocol.Ipc,
                    "pgm" => Protocol.Pgm,
                    "epgm" => Protocol.Epgm,
                    _ => throw new ArgumentException("Invalid protocol type string"),
                };
            }

            public override void SetValue(object component, object value)
            {
            }
        }

        class AddressDescriptor : SimplePropertyDescriptor
        {
            public AddressDescriptor()
                : base(typeof(string), "Address", typeof(string))
            {
            }

            public override object GetValue(object component)
            {
                return GetAddress(component);
            }

            public override void SetValue(object component, object value)
            {
            }
        }

        internal enum Action
        {
            Connect,
            Bind
        }

        internal enum Protocol
        {
            InProc,
            Tcp,
            Ipc,
            Pgm,
            Epgm
        }
    }
}
