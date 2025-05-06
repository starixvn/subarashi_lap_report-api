namespace Fanex.Athena.Report.Core.Repositories;

using System;
using System.Collections.Generic;
using Abstraction;
using Common.Extensions;
using Data;
using Models.DTOs;

public class IpRepository : IIpRepository
{
    public Location GetIpLocation(string IPAddress)
    {
        const string UspKey = "Acc_GetIPInfo_Full";
        var location = new Location();

        using (var db = ObjectDbFactory.CreateInstance(UspKey))
        {
            db.SetParameterValue("IP", IPAddress);

            using (var reader = db.ExecuteReader())
            {
                if (reader.Read())
                {
                    location.CountryCode = reader.GetVal<string>("countrySHORT");
                    location.CountryName = reader.GetVal<string>("countryLONG");
                }
            }
        }

        return location;
    }

    public Tuple<IList<IPV4RangeEntity>, IList<IPV6RangeEntity>> GetVNIPs()
    {
        const string UspKey = "Lrf_GetAllIPVN";

        var ipVNList = new List<IPV4RangeEntity>();
        var ipV6VNList = new List<IPV6RangeEntity>();

        using (var db = ObjectDbFactory.CreateInstance(UspKey))
        {
            using (var reader = db.ExecuteReader())
            {
                while (reader.Read())
                {
                    var ipRange = new IPV4RangeEntity
                    {
                        IPFrom = reader.GetVal<long>("IPFrom"),
                        IPTo = reader.GetVal<long>("IPTo")
                    };
                    ipVNList.Add(ipRange);
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        var ipV6Range = new IPV6RangeEntity
                        {
                            IPFrom = reader.GetVal<string>("IPFrom"),
                            IPTo = reader.GetVal<string>("IPTo")
                        };
                        ipV6VNList.Add(ipV6Range);
                    }
                }
            }
        }

        return Tuple.Create<IList<IPV4RangeEntity>, IList<IPV6RangeEntity>>(ipVNList, ipV6VNList);
    }
}