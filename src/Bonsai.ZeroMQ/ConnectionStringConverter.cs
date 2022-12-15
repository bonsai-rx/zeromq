using System;
using System.Collections;
using System.ComponentModel;

namespace Bonsai.ZeroMQ
{
    internal class ConnectionStringConverter : StringConverter
    {
        const char BindCharacter = '@';
        const char ConnectCharacter = '>';
        const string ProtocolDelimiter = "://";

        static char GetDefaultAction(object value)
        {
            if (value is string connectionString && connectionString.Length > 0)
            {
                return connectionString[0];
            }

            return ConnectCharacter;
        }

        static string GetProtocol(object value)
        {
            if (value is string connectionString)
            {
                var trimString = connectionString.Trim(BindCharacter, ConnectCharacter);
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

        static string GetActionString(DefaultAction value)
        {
            return value switch
            {
                DefaultAction.Connect => ">",
                DefaultAction.Bind => "@",
                _ => string.Empty
            };
        }

        static string GetProtocolString(SocketProtocol value)
        {
            return value switch
            {
                SocketProtocol.TCP => "tcp",
                SocketProtocol.InProc => "inproc",
                SocketProtocol.PGM => "pgm",
                _ => string.Empty
            };
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            var action = (DefaultAction)propertyValues[nameof(DefaultAction)];
            var protocol = (SocketProtocol)propertyValues[nameof(SocketProtocol)];
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
                new DefaultActionDescriptor(),
                new SocketProtocolDescriptor(),
                new AddressDescriptor()
            }).Sort(new[] { nameof(DefaultAction), nameof(SocketProtocol) });
        }

        class DefaultActionDescriptor : SimplePropertyDescriptor
        {
            public DefaultActionDescriptor()
                : base(typeof(string), nameof(DefaultAction), typeof(DefaultAction))
            {
            }

            public override Type PropertyType => typeof(string);

            public override TypeConverter Converter => TypeDescriptor.GetConverter(typeof(DefaultAction));

            public override object GetValue(object component)
            {
                return GetDefaultAction(component) switch
                {
                    '@' => DefaultAction.Bind,
                    _ => DefaultAction.Connect
                };
            }

            public override void SetValue(object component, object value)
            {
            }
        }

        class SocketProtocolDescriptor : SimplePropertyDescriptor
        {

            public SocketProtocolDescriptor()
                : base(typeof(string), nameof(SocketProtocol), typeof(SocketProtocol))
            {
            }

            public override TypeConverter Converter => TypeDescriptor.GetConverter(typeof(SocketProtocol));

            public override object GetValue(object component)
            {
                return GetProtocol(component) switch
                {
                    "tcp" => SocketProtocol.TCP,
                    "pgm" => SocketProtocol.PGM,
                    "inproc" => SocketProtocol.InProc,
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

        internal enum DefaultAction
        {
            Connect,
            Bind
        }

        internal enum SocketProtocol
        {
            TCP,
            InProc,
            PGM
        }
    }
}
