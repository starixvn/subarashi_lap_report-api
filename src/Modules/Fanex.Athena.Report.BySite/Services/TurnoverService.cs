using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Services
{
    public class TurnoverService : ITurnoverService
    {
        private readonly ITurnoverRepository _turnoverRepository;

        public TurnoverService(ITurnoverRepository turnoverRepository)
        {
            _turnoverRepository = turnoverRepository;
        }

        public async Task<IList<TurnoverDto>> GetDataList(TurnoverMetaModel metaModel)
        {
            return await _turnoverRepository.GetDataList(metaModel).ConfigureAwait(false);
        }

        public TurnoverDto SumTotalData(IList<TurnoverDto> dataList)
        {
            var resultTotal = new TurnoverDto
            {
                Deposit = dataList.Sum(p => p.Deposit),
                Withdrawal = dataList.Sum(p => p.Withdrawal),
                Bonus = dataList.Sum(p => p.Bonus),
                Adjustment = dataList.Sum(p => p.Adjustment),
                Promotion = dataList.Sum(p => p.Promotion),
                Turnover = dataList.Sum(p => p.Turnover),
                DrawGame = dataList.Sum(p => p.DrawGame),
                NetTurnover = dataList.Sum(p => p.NetTurnover),
                PendingFunds = dataList.Sum(p => p.PendingFunds),
                WinLoss = dataList.Sum(p => p.WinLoss),
                PlayerComm = dataList.Sum(p => p.PlayerComm),
                ClosingBalance = dataList.Sum(p => p.ClosingBalance),
                AffiliateTransfer = dataList.Sum(p => p.AffiliateTransfer),
                Transfer3rd = dataList.Sum(p => p.Transfer3rd),
                TurnoverBA = dataList.Sum(p => p.TurnoverBA),
                CommBA = dataList.Sum(p => p.CommBA),
                AmountDueToBA = dataList.Sum(p => p.AmountDueToBA)
            };
            return resultTotal;
        }
    }
}
