using System;
using System.Collections.Generic;

namespace Fanex.Athena.Report.Resources.Constants
{
    public static class SportType
    {
        public const int SportsLottery = 169;

        public const int BitCoin = 175;

        public const int Soccer = 1;

        public const int BasketBall = 2;

        public const int IceHockey = 4;

        public const int Tennis = 5;

        public const int Cricket = 50;

        public const int NumberGameHappy5 = 164;

        public const int VirtualBasketball = 193;

        public const int SabaSoccer = -1;

        public const int SabaBasketball = -2;

        public const int SabaCricket = -3;

        public const int Futsal = 49;

        public const int SabaFutsal = -4;

        public const int SabaSoccerPingoal = -5;

        public const int SabaBasketballPinGoal = -6;

        public const int SabaESportsPinGoal = -7;

        public const int Esports = 43;

        public const int SabaTennis = -8;

        public const int SabaIceHockey = -9;

        public const int SabaKabaddi = -10;

        public const int Kabaddi = 47;

        public const int VirtualSport = 180;

        public const int NumberGame = 161;

        public const int LiveCasino = 162;

        public const int VirtualHorseRacing = 198;

        public const int VirtualDogRacing = 177;

        public const int VirtualGreyhound = 182;

        public const int VirtualSpeedway = 183;

        public const int VirtualMotoRacing = 184;

        public const int VirtualCycling = 185;

        public const int BetbyBasketball = 134;

        public const int VirtuallySports = 147;

        public const int VirtualSteepleChase = 125;

        public const int VirtualHarnessRacing = 126;

        public const int VirtualMotorRacing = 127;

        public const int UnknowThaiSport = 46;

        public const int DuplicatedCricket = 27;

        public const int AllBet = 211;

        private static readonly List<Tuple<int, int, string>> SabaVirtualSports = new List<Tuple<int, int, string>>
        {
            Tuple.Create(SabaSoccer, Soccer, ""),
            Tuple.Create(SabaSoccerPingoal, 0, "SoccerPinGoal"),
            Tuple.Create(SabaBasketball, BasketBall, ""),
            Tuple.Create(SabaBasketballPinGoal, 0, "BasketballPinGoal"),
            Tuple.Create(SabaCricket, Cricket, ""),
            Tuple.Create(SabaFutsal, Futsal, ""),
            Tuple.Create(SabaTennis, Tennis, ""),
            Tuple.Create(SabaIceHockey, IceHockey, ""),
            Tuple.Create(SabaKabaddi, Kabaddi, ""),
            Tuple.Create(SabaESportsPinGoal, 0, "ESportsPinGoal")
        };

        public static List<Tuple<int, int, string>> GetSabaVirtualSports()
        {
            return SabaVirtualSports;
        }
    }
}