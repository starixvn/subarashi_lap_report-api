using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fanex.Athena.Report.Core.Services
{
    public class DataPermissionService : IDataPermissionService
    {
        private readonly IContextService _contextService;

        public DataPermissionService(IContextService contextService)
        {
            _contextService = contextService;
        }

        public T ClearDataByLimitPermission<T>(T data)
        {
            var properties = GetPropertiesByDataAttribute<T>();

            if (_contextService.IsSuperAdmin)
            {
                return data;
            }

            foreach (var property in properties)
            {
                var (info, attribute) = property;

                if (!string.IsNullOrEmpty(attribute.Permission) && _contextService.IsInRoles(attribute.Permission))
                {
                    info.SetValue(data, null);
                }
            }

            return data;
        }

        private static IEnumerable<(PropertyInfo, DataPermissionAttribute)> GetPropertiesByDataAttribute<T>()
        {
            var properties = typeof(T).GetProperties()
                                      .Where(_ => Attribute.IsDefined(_, typeof(DataPermissionAttribute)));
            return properties.Select(_ =>
            {
                var attribute =
                    Attribute.GetCustomAttribute(_, typeof(DataPermissionAttribute)) as DataPermissionAttribute;
                return (_, attribute);
            });
        }
    }
}
