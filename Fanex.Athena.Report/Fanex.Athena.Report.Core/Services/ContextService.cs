using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using Fanex.Athena.Report.Common.Model.DTO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Fanex.Athena.Report.Core.Services
{
    public class ContextService : IContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected HttpContext Context => _httpContextAccessor.HttpContext;
        protected ClaimsPrincipal User => Context.User;

        public ContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAllowedCustomerComm => GetClaimValue<bool>("IsAllowedCustomerComm");

        public bool IsEnableBA => GetClaimValue<bool>("IsEnableBA");

        public bool IsEnableSingleWallet => GetClaimValue<bool>("IsEnableSingleWallet");

        public string SiteName => GetClaimValue("SiteName");

        public string UserName => GetClaimValue("UserID");

        public bool IsFakeLicensee => GetClaimValue<bool>("IsFakeLicensee");

        public string SiteDBServer => GetClaimValue("SiteDBServer");

        public IEnumerable<string> OfferedSubProductList
        {
            get
            {
                var offeredSubProductList = GetClaimValue("OfferedSubProducts");
                return string.IsNullOrEmpty(offeredSubProductList) ? Enumerable.Empty<string>() : offeredSubProductList.Split(',');
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

                int? GetWhiteLabelID()
                {
                    var value = GetClaimValue<int>("WhiteLabelID");
                    if (value < 0)
                    {
                        return null;
                    }
                    return value;
                }

                const int SubsidiaryLevel = 2;

                if (GetClaimValue<bool>("IsEnabledSubsidiary") && GetClaimValue<int>("UserLevel") < SubsidiaryLevel)
                {
                    if (GetClaimValue<bool>("IsMasterLicensee") && !GetWhiteLabelID().HasValue)
                    {
                        return Constants.SubsidiaryViewType.MasterManagement;
                    }

                    return Constants.SubsidiaryViewType.NormalManagement;
                }
                else
                {
                    return Constants.SubsidiaryViewType.NotManagement;
                }
            }
        }

        public bool IsInRoles(params string[] roles) => roles?.Any(_ => User.IsInRole(_)) ?? false;

        public string ClientIP => Context.Connection.RemoteIpAddress.ToString();

        private IReadOnlyDictionary<int, int[]> _agentList;

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

        private IReadOnlyDictionary<int, string> _currencyList;

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

        private IEnumerable<CurrencyAgentInfo> _rawCurrencyAgentList;

        protected internal IEnumerable<CurrencyAgentInfo> RawCurrencyAgentList
        {
            get
            {
                if (_rawCurrencyAgentList is null)
                {
                    var agentClaims = User.Claims.Where(_ => _.Type.Equals("CurrencyAgent", StringComparison.OrdinalIgnoreCase));
                    _rawCurrencyAgentList = agentClaims.Select(agent => JsonConvert.DeserializeObject<CurrencyAgentInfo>(agent.Value));
                }

                return _rawCurrencyAgentList;
            }
        }

        public bool ForceIgnoreSharding => bool.TrueString.Equals(Context.Request.Headers["X-IgnoreSharding"], StringComparison.OrdinalIgnoreCase);

        public IEnumerable<AuditCurrency> AuditCurrencies
        {
            get
            {
                var auditCurrenciesClaims = User.Claims.Where(_ => _.Type.Equals("AuditCurrencies", StringComparison.OrdinalIgnoreCase));
                return auditCurrenciesClaims.Select(_ => JsonConvert.DeserializeObject<AuditCurrency>(_.Value));
            }
        }

        private T GetClaimValue<T>(string claimType)
            => (T)Convert.ChangeType(GetClaimValue(claimType), typeof(T));

        private string GetClaimValue(string claimType)
            => User.FindFirst(claimType)?.Value;

        protected internal class CurrencyAgentInfo
        {
            public int[] AgentId { get; set; }
            public int CurrencyId { get; set; }
            public string Currency { get; set; }
        }
    }
}