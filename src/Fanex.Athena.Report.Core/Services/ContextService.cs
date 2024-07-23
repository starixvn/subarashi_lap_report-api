using Fanex.Athena.Report.Common.Helpers;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Enums;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Fanex.Athena.Report.Common.Model.DataDomain;

namespace Fanex.Athena.Report.Core.Services
{
    public class ContextService : IContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private IReadOnlyDictionary<int, int[]> _agentList;

        private IReadOnlyDictionary<int, string> _currencyList;

        private IEnumerable<CurrencyAgentInfo> _rawCurrencyAgentList;

        public ContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected HttpContext Context => _httpContextAccessor.HttpContext;
        protected ClaimsPrincipal User => Context.User;

        public string ClientIP => Context.Connection.RemoteIpAddress.ToString();

        protected internal IEnumerable<CurrencyAgentInfo> RawCurrencyAgentList
        {
            get
            {
                if (_rawCurrencyAgentList is null)
                {
                    var agentClaims =
                        User.Claims.Where(_ => _.Type.Equals("CurrencyAgent", StringComparison.OrdinalIgnoreCase));
                    _rawCurrencyAgentList =
                        agentClaims.Select(agent => JsonConvert.DeserializeObject<CurrencyAgentInfo>(agent.Value));
                }

                return _rawCurrencyAgentList;
            }
        }

        public bool IsAllowedCustomerComm => GetClaimValue<bool>("IsAllowedCustomerComm");

        public bool IsEnableBA => GetClaimValue<bool>("IsEnableBA");

        public bool IsEnableSingleWallet => GetClaimValue<bool>("IsEnableSingleWallet");

        public int LicenseeId => GetClaimValue("LicenseeId", -1);

        public string Language => CultureInfo.CurrentUICulture.Name;

        public string SiteName => GetClaimValue("SiteName");

        public bool IsSuperAdmin => GetClaimValue<bool>("Administrator");

        public int SpecialPlatform => GetClaimValue<int>("SpecialPlatform");

        public int SpecialPlatformType => GetClaimValue<int>("SpecialPlatformType");

        public string UserName => GetClaimValue("UserID");

        public bool IsFakeLicensee => GetClaimValue<bool>("IsFakeLicensee");

        public string SiteDBServer => GetClaimValue("SiteDBServer");

        public int UserId => GetClaimValue<int>("UserSEQ");

        public byte UserLevel => GetClaimValue<byte>("UserLevel");

        public int WhiteLabelID => GetClaimValue("WhiteLabelID", -1);

        public bool IsHideSaba => SabaHelper.IsHideSaba(SpecialPlatform);

        public bool IsShowIp => GetClaimValue("IsShowIP", true);

        public bool IsShowIpVn => IsShowIp && IsInRoles("Administrator", Roles.BetListViewIpVn);

        public string DefaultBaseCurrency => GetClaimValue("BaseCurrency", "EUR");

        public int DefaultBaseCurrencyId => SystemBaseCurrency.BaseCurrencyList
                                                              .FirstOrDefault(x => x.Value.Equals(
                                                                   DefaultBaseCurrency, 
                                                                   StringComparison.OrdinalIgnoreCase))
                                                              .Key;

        public AppMode ApplicationMode => GetClaimValue("Name").StartsWith("lrf") ? AppMode.Self : AppMode.Parasitic;

        public IEnumerable<string> OfferedOddsGroupsList
        {
            get
            {
                var offeredOddsGroupList = GetClaimValue("OfferedOddsGroups", "");
                return string.IsNullOrEmpty(offeredOddsGroupList) ? new string[] { } : offeredOddsGroupList.Split(',');
            }
        }

        public IEnumerable<string> OfferedSubProductList
        {
            get
            {
                var offeredSubProductList = GetClaimValue("OfferedSubProducts");
                return string.IsNullOrEmpty(offeredSubProductList)
                           ? Enumerable.Empty<string>()
                           : offeredSubProductList.Split(',');
            }
        }

        public byte SubsidiaryViewType
        {
            get
            {
                if (string.IsNullOrEmpty(GetClaimValue("VendorID")))
                {
                    return Constants.SubsidiaryViewType.NotManagement;
                }

                const int SubsidiaryLevel = 2;

                if (GetClaimValue<bool>("IsEnabledSubsidiary") && GetClaimValue<int>("UserLevel") < SubsidiaryLevel)
                {
                    if (GetClaimValue<bool>("IsMasterLicensee") && WhiteLabelID < 0)
                    {
                        return Constants.SubsidiaryViewType.MasterManagement;
                    }

                    return Constants.SubsidiaryViewType.NormalManagement;
                }

                return Constants.SubsidiaryViewType.NotManagement;
            }
        }

        public bool IsInRoles(params string[] roles)
        {
            return Array.Exists(roles, x => User.IsInRole(x));
        }

        public IReadOnlyDictionary<int, int[]> AgentList
        {
            get
            {
                if (_agentList is null)
                {
                    var currencyAgentList = RawCurrencyAgentList;
                    var dicClaims = currencyAgentList.ToDictionary(_ => _.CurrencyId, _ => _.AgentId);
                    _agentList = new ReadOnlyDictionary<int, int[]>(dicClaims);
                }

                return _agentList;
            }
        }

        public IReadOnlyDictionary<int, string> CurrencyList
        {
            get
            {
                if (_currencyList is null)
                {
                    var currencyAgentList = RawCurrencyAgentList;
                    var dicClaims = currencyAgentList.ToDictionary(_ => _.CurrencyId, _ => _.Currency);
                    _currencyList = new ReadOnlyDictionary<int, string>(dicClaims);
                }

                return _currencyList;
            }
        }

        public bool ForceIgnoreSharding =>
            bool.TrueString.Equals(Context.Request.Headers["X-IgnoreSharding"], StringComparison.OrdinalIgnoreCase);

        public IEnumerable<AuditCurrency> AuditCurrencies
        {
            get
            {
                var auditCurrenciesClaims =
                    User.Claims.Where(_ => _.Type.Equals("AuditCurrencies", StringComparison.OrdinalIgnoreCase));
                return auditCurrenciesClaims.Select(_ => JsonConvert.DeserializeObject<AuditCurrency>(_.Value));
            }
        }

        private T GetClaimValue<T>(string claimType)
        {
            return (T)Convert.ChangeType(GetClaimValue(claimType), typeof(T));
        }

        private T GetClaimValue<T>(string claimType, T _default)
        {
            var claimValue = GetClaimValue(claimType);
            return string.IsNullOrEmpty(claimValue) ? _default : (T)Convert.ChangeType(claimValue, typeof(T));
        }

        private string GetClaimValue(string claimType)
        {
            return User.FindFirst(claimType)?.Value;
        }

        protected internal class CurrencyAgentInfo
        {
            public int[] AgentId { get; set; }

            public int CurrencyId { get; set; }

            public string Currency { get; set; }
        }
    }
}