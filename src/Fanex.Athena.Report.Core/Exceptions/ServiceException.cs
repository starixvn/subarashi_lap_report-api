namespace Fanex.Athena.Report.Core.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ServiceException : Exception
    {
        private readonly string _serviceName;

        protected ServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public ServiceException(string message, string serviceName)
            : base(message)
        {
            _serviceName = serviceName;
        }

        public ServiceException(string message, string serviceName, Exception exception)
            : base(message, exception)
        {
            _serviceName = serviceName;
        }

        public override string Message =>
            $"Exception when call Service: {_serviceName}. Exception Message: {base.Message}";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(_serviceName), _serviceName);
            base.GetObjectData(info, context);
        }
    }


}
