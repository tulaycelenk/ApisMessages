using AviatorManager.ViewModel.Aodb;
using Bridge.App.Core.Authentication;
using Bridge.Common;
using Bridge.Common.Logging;
using DCS.App.Service.Entity;
using DCS.App.Service.Helper;
using DCS.App.Service.Resources;
using DCS.App.Service.Service.Dcs;
using DCS.ViewModel.Dcs;
using DCS.ViewModel.Paging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DCS.App.Service.Service.Exporter.PaxLst
{
   class YeniApisMesaj : IExporter, ApisEdifactHelper
    {
        private readonly CultureInfo enUs = new CultureInfo("en-Us");
        List<string> apisMessageList = new List<string>();
        List<string> apisMessageHeaderList = new List<string>();
        List<string> apisMessagePassengerList = new List<string>();
        List<string> apisMessageFootherList = new List<string>();
        List<string> apisMessageDividedPassengerList = new List<string>();
        /// <summary>
        /// Get Create APIS MESSAGES
        /// </summary>
        /// <param name="flight"></param>
        /// <param name="exportRequest"></param>
        /// <returns></returns>
        public List<string> GetCreateMessage(FlightResponse flight, ExportRequest exportRequest)
        {
            IEnumerable<PassengerDocs> passengerDocsList = PassengerDocsService.GetPassengerWithDocsInformation(new PassengerDocsPagingRequest() { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 }).Item1.Where(p => (p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Boarded || p.PassengerStatusCode == ParameterHelper.PaxStatusCode.CheckedIn) && p.PassengerId != 0).OrderBy(s => s.Surname);
            IEnumerable<Passenger> passengerList = PassengerService.QueryPassengersJoined(new PassengerPagingRequest { FlightId = flight.Id }).Where(p => p.PaxStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PaxStatusCode == ParameterHelper.PaxStatusCode.Boarded || p.PaxStatusCode == ParameterHelper.PaxStatusCode.CheckedIn).OrderBy(s => s.Surname);
            IEnumerable<Baggage> passengerBaggageList = BaggageService.QueryBaggagesJoined(new BaggagePagingRequest { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 });

            RestHelper rst = new RestHelper(new UrlProviderWithToken(), new HeaderProvider());
            var adesTimezone = rst.GetTimezoneOffsetByAirport(flight.AdesId, DateTime.UtcNow);
            var adesOfset = adesTimezone != null ? TimeSpan.FromSeconds(adesTimezone.GmtOffset).TotalSeconds : 0;
            var adepTimezone = rst.GetTimezoneOffsetByAirport(flight.AdepId, DateTime.UtcNow);
            var adepOfset = adepTimezone != null ? TimeSpan.FromSeconds(adepTimezone.GmtOffset).TotalSeconds : 0;

            if (adesTimezone == null)
                Logger.Default.Append(LogLevel.Error, "35001 - Time Zone Values is not Found for " + flight.DestinationCountryIsoCode);
            else if (adesTimezone == null && adepTimezone == null)
                Logger.Default.Append(LogLevel.Error, "35002 - There is not any Time Zone Value found for " + flight.DestinationCountryIsoCode + " and " + flight.DepartureCountryIsoCode + ". APIS messages build by using UTC dates.");
            Random rnd = new Random();
            var enUs = new CultureInfo("en-US");
            var nowDate = DateTime.Now.ToString("yyMMdd", enUs) + colon + DateTime.Now.ToString("HHmm", enUs);

            int apisMessageHeaderListCharecterCount = 0;
            int apisMessagePassengerListCharecterCount = 0;
            int apisMessageFootherListCharecterCount = 0;
            int fromUNHtoUNTTotalRow = 0;
            int confirmation1 = rnd.Next(100000000, 999999999); //UNB ve UNZ için
            int confirmation2 = rnd.Next(100000000, 999999999); //UNG ve UNE için
            int confirmation3 = rnd.Next(100000000, 999999999); //UNH ve UNT için
            int confirmation4 = rnd.Next(100000000, 999999999); //UNH
            int partIdentifier = 1;
            bool segmentUna = false, segmentUnb = false, segmentUng = false, segmentUnh = false, segmentBgm745 = false, segmentBgm266 = false, segmentBgm266Cl = false, segmentRffTn = false, segmentNadMs = false, segmentCom = false, segmentTdt20 = false, segmentTdt34 = false, segmentLoc125 = false, segmentDtm189 = false, segmentLoc87 = false, segmentDtm232 = false, segmentNadFl = false, segmentAtt2 = false, segmentDtm329 = false, segmentMeaCt = false, segmentMeaWt = false, segmentGei = false, segmentFtx = false, segmentLoc178 = false, segmentLoc179 = false, segmentLoc22 = false, segmentNat2 = false, segmentRffAvf = false, segmentRffAbo = false, segmentRffSea = false, segmentRffCr = false, segmentDoc = false, segmentDtm36 = false, segmentLoc91 = false, segmentCnt42 = false, segmentUnt = false, segmentUne = false, segmentUnz = false;
            string recive = null, recive2 = null;
            string version = null;
            var yearToMin = DateTime.Now.ToString("yyMMdd", enUs) + ":" + DateTime.Now.ToString("HHmm", enUs);
            var yearToDay = DateTime.Now.ToString("yyMMdd", enUs);

            #region Export Type
            if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.BelgiumApis)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "BEGOVAPI";
                version = "15B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.DenmarkApis)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentLoc22 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "DKGOVAPI";
                version = "05B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.FinlandApis)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentLoc22 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "FIAPIS";
                version = "05B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.FranceApis)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "APIPNRFR";
                version = "02B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.GermanyEdifactPaxLst)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "BPOLAPIS";
                recive2 = "ZNVPIXH";
                version = "02B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.JordanEdifactApis)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentRffTn = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentLoc22 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentRffCr = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "JORAPIS";
                version = "12B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.IraqApis)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "IQGOVAPI";
                version = "15B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.ItalyApis)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "BCSAPIS";
                version = "02B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.LebenonApis)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "BCSAPIS";
                version = "02B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.NetherlandsPaxLst)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "NLDAPIKM";
                version = "02B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.RomaniaApis)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentLoc22 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "";
                version = "05B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.RussiaApisPostDepartureForRussiaArrivalPaxLst)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm266 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "RUSAPIS";
                version = "02B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.RussiaApisPostDepartureForRussiaOverflyPaxLst)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm266 = true; segmentNadMs = true; segmentCom = true; segmentTdt34 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "RUSAPIS";
                version = "02B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.RussiaApisPreDepartureForRussiaArrivalPaxLst)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm266Cl = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "RUSAPIS";
                version = "02B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.RussiaApisPreDepartureForRussiaOverflyPaxLst)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm266Cl = true; segmentNadMs = true; segmentCom = true; segmentTdt34 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "RUSAPIS";
                version = "02B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.SwissEdifactApis)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "HDQCH2X";
                version = "02B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.UkraineApis)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentLoc22 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "DKGOVAPI";
                version = "05B";
            }
            else if (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.QatarApis)
            {
                segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentLoc22 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                recive = "";
                version = "05B";
            }
            #endregion

            if (segmentUna && (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.DenmarkApis))
                apisMessageHeaderList.Add(una????);
            if (segmentUna && (exportRequest.ExporterType != ParameterHelper.ExporterFactoryCode.DenmarkApis))
                apisMessageHeaderList.Add("UNA:+.? '");
            if (segmentUnb && (exportRequest.ExporterType != ParameterHelper.ExporterFactoryCode.JordanEdifactApis && exportRequest.ExporterType != ParameterHelper.ExporterFactoryCode.NetherlandsPaxLst))
                apisMessageHeaderList.Add(unb+sender + plus + recive + plus + yearToMin + plus + BZcode + apos);
            if (segmentUnb && (exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.JordanEdifactApis || exportRequest.ExporterType == ParameterHelper.ExporterFactoryCode.NetherlandsPaxLst))
                apisMessageHeaderList.Add(unb+sender+ plus + recive + plus + yearToMin + plus + BZcode + "++" + recive + apos);
            if (segmentUng)
                apisMessageHeaderList.Add(ung+sender + plus + (!string.IsNullOrEmpty(recive2) ? recive2 : recive) + plus + nowDate + plus + GEcode + typeVersionNum + version + apos);
            if (segmentUnh)
                apisMessageHeaderList.Add(unh + HTcode + unhMid1 + version + unhMid2 + confirmation4 + unhSinglePart);
            if (segmentBgm745)
                apisMessageHeaderList.Add(bgm745+apos);

            //BGM IDENTIFIER CODE BÖLÜMÜ 12B VE SONRASINDA VAR ORADA DA 266 İÇİN INTERACTIVE API MESSAGES ONLY DİYOR 
            if (segmentBgm266)
                apisMessageHeaderList.Add("BGM+266+CLOB'");
            if (segmentBgm266Cl)
                apisMessageHeaderList.Add("BGM+266+CL'");

            //BURADAKİ BR SAYININ BAŞINDA OLMASI GEREKİYOR MU BİLMİYORUM HELPERDA BU ŞEKİLDE TANIMLAMAMIŞTIM (rffTn)
            if (segmentRffTn) 
                apisMessageHeaderList.Add("RFF+TN:BR" + rnd.Next(100000000, 999999999) + apos);
            if (segmentNadMs)
                apisMessageHeaderList.Add(nadMs);
            if (segmentCom)
                apisMessageHeaderList.Add(com);
            if (segmentTdt20)
                apisMessageHeaderList.Add(tdt20+ flight.AirlineIataCode + flight.FlightNumber.ToString().PadLeft(4, '0') + threePlus + flight.AirlineIataCode + apos);
            if (segmentTdt34)
                apisMessageHeaderList.Add(tdt34 + flight.AirlineIataCode + flight.FlightNumber.ToString().PadLeft(4, '0') + threePlus + flight.AirlineIataCode + apos);
            if (segmentLoc125 && !string.IsNullOrEmpty(flight.DepartureIata))
                apisMessageHeaderList.Add(loc125 + flight.DepartureIata + apos);
            if (segmentDtm189 && flight.Sobt.HasValue)
                apisMessageHeaderList.Add(dtm189 + ((DateTime)flight.Sobt).AddSeconds(adepOfset).ToString("yyMMddHHmm", enUs) + dtmEnd);
            if (segmentLoc87 && !string.IsNullOrEmpty(flight.DestinationIata))
                apisMessageHeaderList.Add(loc87 + flight.DestinationIata + apos);
            if (segmentDtm232 && flight.Sldt.HasValue)
                apisMessageHeaderList.Add(dtm232 + ((DateTime)flight.Sldt).AddSeconds(adesOfset == 0 ? adepOfset : adesOfset).ToString("yyMMddHHmm", enUs) + dtmEnd);

            foreach (var passengerDocs in passengerDocsList)
            {
                var passengerDoco = PassengerDocoService.GetPassengerDoco(passengerDocs.PassengerId).FirstOrDefault();
                var passengerDoca = PassengerDocaService.GetPassengerDocaWithPaging(new PassengerDocaPagingRequest { PassengerId = passengerDocs.PassengerId, PageNumber = 1, PageSize = 2000 }).Item1.FirstOrDefault();
                var passenger = passengerList.Where(filter => filter.Id == passengerDocs.PassengerId).FirstOrDefault();
                var passengerBaggages = passengerBaggageList.Where(filter => filter.PassengerId == passengerDocs.PassengerId).OrderBy(s => s.BagTag);

                if (segmentNadFl && !string.IsNullOrEmpty(passengerDocs.Surname))
                    apisMessagePassengerList.Add(nadFl + (!string.IsNullOrEmpty(passengerDocs.Surname) ? (passengerDocs.Surname.Length > 35 ? passengerDocs.Surname.Substring(0, 35) : passengerDocs.Surname) : "") + (!string.IsNullOrEmpty(passengerDocs.Name) ? (colon+ (passengerDocs.Name.Length > 35 ? passengerDocs.Name.Substring(0, 35) : passengerDocs.Name).Replace(" ", colon)) : "") + (passengerDoca != null ? (plus + (!string.IsNullOrEmpty(passengerDoca.ResidenceAddress) ? passengerDoca.ResidenceAddress.Length > 35 ? passengerDoca.ResidenceAddress.Substring(0, 35) : passengerDoca.ResidenceAddress : "") + plus + passengerDoca.ResidenceCity + plus + passengerDoca.ResidenceZipCode + plus + passengerDoca.ResidenceCountryIso3Code) : "") + apos);

                if (segmentAtt2 && !string.IsNullOrEmpty(passengerDocs.GenderCode))
                    apisMessagePassengerList.Add("ATT+2++" + (passengerDocs.GenderCode == ParameterHelper.PaxGenderCode.FemaleChild ? "F" : (passengerDocs.GenderCode == ParameterHelper.PaxGenderCode.MaleChild ? "M" : (passengerDocs.GenderCode == ParameterHelper.PaxGenderCode.Child || passengerDocs.GenderCode == ParameterHelper.PaxGenderCode.Infant ? "U" : passengerDocs.GenderCode))) + apos);
                else
                    apisMessagePassengerList.Add(attU);
                /* 15B DE X VE U FARKINI TAM AÇIKLAMAMIŞ AYRICA UI TARAFINDA SANIRIM SADECE FEMALE VE MALE SEÇENEKLERİ VAR BEN AŞAĞIDAKİ GİBİ YAPTIM
                  passengerDocs.GenderCode == PaxGenderCode.FemaleChild || passengerDocs.GenderCode == PaxGenderCode.Female ? attF : (passengerDocs.GenderCode == PaxGenderCode.MaleChild || passengerDocs.GenderCode == PaxGenderCode.Male ? attM : (passengerDocs.GenderCode == PaxGenderCode.Child || passengerDocs.GenderCode == PaxGenderCode.Infant ? attU )));
               */
                if (segmentDtm329 && passengerDocs.Dob.HasValue)
                    apisMessagePassengerList.Add(dtm329 + passengerDocs.Dob.Value.ToString("yyMMdd") + apos);
                if (segmentMeaCt && passenger != null && passenger.BagCount > 0)
                    apisMessagePassengerList.Add(meaCt + passenger.BagCount + apos);
                if (segmentMeaWt && passenger != null && passenger.BagTotalWeight > 0)
                    apisMessagePassengerList.Add(meaWtKg + passenger.BagTotalWeight + apos);
                if (segmentGei)
                    apisMessagePassengerList.Add(gei173);
                if (segmentFtx && passengerBaggages != null)
                {
                    bool isSequentialBagTag = false; int sequentialBagTagNumber = 1; string sequentialBagTagText = null;
                    for (var i = 0; i < passengerBaggages.Count(); i++)
                    {
                        if (i < passengerBaggages.Count() - 1 && Convert.ToInt32(passengerBaggages.ElementAt(i).BagTag) + 1 == Convert.ToInt32(passengerBaggages.ElementAt(i + 1).BagTag))
                        {
                            sequentialBagTagNumber++;
                            isSequentialBagTag = true;
                            sequentialBagTagText = (ftx + flight.AirlineIataCode + passengerBaggages.ElementAt(i - sequentialBagTagNumber + 2).BagTag + colon + sequentialBagTagNumber + apos);
                        }
                        else
                        {
                            if (isSequentialBagTag)
                                apisMessagePassengerList.Add(sequentialBagTagText);
                            else
                                apisMessagePassengerList.Add(ftx+ flight.AirlineIataCode + passengerBaggages.ElementAt(i).BagTag + apos);

                            isSequentialBagTag = false; sequentialBagTagText = null; sequentialBagTagNumber = 1;
                        }
                    }
                    if (isSequentialBagTag)
                        apisMessagePassengerList.Add(sequentialBagTagText);
                }
                if (segmentLoc22 && !string.IsNullOrEmpty(flight.DestinationIata))
                    apisMessagePassengerList.Add(loc22 + flight.DestinationIata + apos);
                if (segmentLoc178 && (!string.IsNullOrEmpty(flight.DepartureIata)))
                    apisMessagePassengerList.Add(loc178 + flight.DepartureIata + apos);
                if (segmentLoc179 && (!string.IsNullOrEmpty(flight.DestinationIata)))
                    apisMessagePassengerList.Add(loc179 + flight.DestinationIata + apos);
                //COM+202 628 9292:TE+202 628 4998:FX+davidsonr.at.iata.org:EM' Indicates telephone number, fax number and email address of the traveller ----> SSR da tutuluyor.
                //EMP+1+CR1:110:111’ Indicates current passenger is a cockpit crew ----> Cockpit Crew  bilgisi grilmediğin için yapılmayacak.
                if (segmentNat2 && !string.IsNullOrEmpty(passengerDocs.NationalityCode))
                    apisMessagePassengerList.Add(nat2+ passengerDocs.NationalityCode + apos);
                //nationality eğer almanya apisinde almanya ile sadece D yazılır
                if (segmentRffAvf && !string.IsNullOrEmpty(passengerDocs.PassengerPnr))
                    apisMessagePassengerList.Add(rffAvf+ passengerDocs.PassengerPnr + apos);
                if (segmentRffAbo && passengerDocs.PassengerId > 0)
                    apisMessagePassengerList.Add(rffAbo+ passengerDocs.PassengerId.ToString().PadLeft(10, '0') + apos);
                if (segmentRffSea && !string.IsNullOrEmpty(passengerDocs.PassengerSeatNumber))
                    apisMessagePassengerList.Add(rffSea+ passengerDocs.PassengerSeatNumber.PadLeft(3, '0') + apos);
                if (segmentRffCr && passenger != null && !string.IsNullOrEmpty(passenger.Fqtv))
                    apisMessagePassengerList.Add(rffCr+ passenger.Fqtv + apos);
                if (segmentDoc && (!string.IsNullOrEmpty(passengerDocs.DocNumber) || !string.IsNullOrEmpty(passengerDocs.DocTypeCode)))
                    apisMessagePassengerList.Add(doc + (passengerDocs.DocTypeCode == "O" ? (!string.IsNullOrEmpty(passengerDocs.CustomDocTypeCode) ? (passengerDocs.CustomDocTypeCode.Substring(0, 1).ToUpper() == "I" || passengerDocs.CustomDocTypeCode.Substring(0, 1).ToUpper() == "P" ? passengerDocs.CustomDocTypeCode.Substring(0, 1) : (passengerDocs.CustomDocTypeCode.ToUpper() == "AC" ? passengerDocs.CustomDocTypeCode : "F")) : "F") : (!string.IsNullOrEmpty(passengerDocs.DocTypeCode) ? passengerDocs.DocTypeCode.Substring(0, 1).ToUpper() : "F")) + plus + passengerDocs.DocNumber + apos);
                if (segmentDtm36 && passengerDocs.Doe.HasValue)
                    apisMessagePassengerList.Add(dtm36 + passengerDocs.Doe.Value.ToString("yyMMdd") + apos);
                if (segmentLoc91 && !string.IsNullOrEmpty(passengerDocs.DocIssuerNationalityCode))
                    apisMessagePassengerList.Add(loc91 + (passengerDocs.DocIssuerNationalityCode) + apos);
                if (passengerDoco != null)
                {
                    if (segmentDoc && (!string.IsNullOrEmpty(passengerDoco.DocNumber) || !string.IsNullOrEmpty(passengerDoco.DocTypeCode)))

                        apisMessagePassengerList.Add(doc + (passengerDoco.DocTypeCode == "O" ? (!string.IsNullOrEmpty(passengerDoco.CustomDocTypeCode) ? (passengerDoco.CustomDocTypeCode.Substring(0, 1).ToUpper() == "I" || passengerDoco.CustomDocTypeCode.Substring(0, 1).ToUpper() == "P" ? passengerDoco.CustomDocTypeCode.Substring(0, 1) : (passengerDoco.CustomDocTypeCode.ToUpper() == "AC" ? passengerDoco.CustomDocTypeCode : "F")) : "F") : (!string.IsNullOrEmpty(passengerDoco.DocTypeCode) ? passengerDoco.DocTypeCode.Substring(0, 1).ToUpper() : "F")) + plus + passengerDoco.DocNumber + apos);


                    /*   UI TARAFINDA P VE V DIŞINDA BELGE TÜRLERİ TUTULMUYOR VE OTHER SEÇENEĞİNDEN SONRA ELLE GİRİLİYOR DİYE HATIRLIYORUM BU BELKİ DROPBOX ŞEKLİNDE DE YAPABİLİRİZ DİYE DÜŞÜNDÜM
                     
                     *   if (!string.IsNullOrEmpty(passengerDocs.DocNumberr) || !string.IsNullOrEmpty(passengerDocs.DocTypeCode)) { 
                            switch (passengerDocs.DocTypeCode)
                            {
                                case "P":
                                    tw.WriteLine(docP+ passengerDocs.DocNumber + apos);
                                    break;
                                case "V":
                                    tw.WriteLine(docV + passengerDocs.DocNumber + apos);
                                    break;
                                case "A":
                                    tw.WriteLine(docA + passengerDocs.DocNumber + apos);
                                    break;
                                case "C":
                                    tw.WriteLine(docI + passengerDocs.DocNumber + apos);
                                    break;
                                case "I":
                                    tw.WriteLine(docAC+ passengerDocs.DocNumber + apos);
                                    break;
                                case "AC":
                                    tw.WriteLine(docIP + passengerDocs.DocNumber + apos);
                                    break;
                                case "IP":
                                    tw.WriteLine(docC+ passengerDocs.DocNumber + apos);
                                    break;
                                case "M":
                                    tw.WriteLine(docF + passengerDocs.DocNumber + apos);
                                    break;
                                default:
                                    tw.WriteLine(docF + passengerDocs.DocNumber + apos);
                                    break;
                        } 
                    }
                     */

                    if (segmentDtm36 && passengerDoco.Doe.HasValue)
                        apisMessagePassengerList.Add(dtm36+ passengerDoco.Doe.Value.ToString("yyMMdd") + apos);
                    if (segmentLoc91 && !string.IsNullOrEmpty(passengerDoco.DocForNationalityCode))
                        apisMessagePassengerList.Add(loc91 + (passengerDoco.DocForNationalityCode) + apos);
                }
            }
            if (segmentCnt42)
                apisMessageFootherList.Add(cnt42 + passengerList.Count() + apos);
            if (segmentUnt)
                apisMessageFootherList.Add(unt + fromUNHtoUNTTotalRow + plus + HTcode + apos);
            if (segmentUne)
                apisMessageFootherList.Add(une + GEcode + apos);
            if (segmentUnz)
                apisMessageFootherList.Add(unz + BZCode + apos);

            foreach (var apisMessageHeader in apisMessageHeaderList)
            {
                apisMessageHeaderListCharecterCount = apisMessageHeaderListCharecterCount + apisMessageHeader.Length;
            }
            foreach (var apisMessageFoother in apisMessageFootherList)
            {
                apisMessageFootherListCharecterCount = apisMessageFootherListCharecterCount + apisMessageFoother.Length;
            }

            #region Single Part
            if (exportRequest.PartType == "S")
            {
                //PART IDENTIFIER MESAJIN KAÇINCI PARTI OLDUĞUNU BELİRTİYORSA ÖRNEKLERE GÖRE SADECE UNH SATIR SONUNDA BULUNUYOR
                apisMessageHeaderList[1] = unb+sender + plus + recive + plus + nowDate + plus + partIdentifier + BZCode + "++" + recive + apos;
                apisMessageHeaderList[2] = ung+sender + plus + (!string.IsNullOrEmpty(recive2) ? recive2 : recive) + plus + nowDate + plus + partIdentifier + confirmation2 + typeVersionNum + version + apos;
                apisMessageHeaderList[3] = unh + HTCode + unhMid1 + version + unhMid2 + partIdentifier + confirmation4 + plus + partIdentifier.ToString().PadLeft(2, '0') + (partIdentifier == 1 ? ":C" : "") + apos;
                //apisMessageList listesine Header-PassengerList-Foother bilgileri eklenip yazdırılıyor.
                apisMessageList.AddRange(apisMessageHeaderList);
                apisMessageList.AddRange(apisMessagePassengerList);
                //Multi part UNH segmentinde UNt segmentine kadar olan segment sayısı
                fromUNHtoUNTTotalRow = apisMessageList.Count() - 3; // -3: UNA-UNB-UNG
                apisMessageFootherList[1] = unt + partIdentifier + (fromUNHtoUNTTotalRow + 2) + plus + confirmation3 + apos; // +2: CNT-UNT
                apisMessageFootherList[2] = une + partIdentifier + confirmation2 + apos;
                apisMessageFootherList[3] = unz + partIdentifier + confirmation1 + apos;
                apisMessageList.AddRange(apisMessageFootherList);
            }

            #endregion Single Part

            #region Multi Part
            else if (exportRequest.PartType == "M")
            {
                //Multi-part seçeneği için partlar en fazla 1900 karakter olacak.
                for (var i = 0; i < apisMessagePassengerList.Count(); i++)
                {
                    apisMessagePassengerListCharecterCount = apisMessagePassengerListCharecterCount + apisMessagePassengerList[i].Length + 1; // +1:Line 
                    if (apisMessagePassengerListCharecterCount <= (1900 - apisMessageHeaderListCharecterCount - apisMessageFootherListCharecterCount))
                    {
                        apisMessageDividedPassengerList.Add(apisMessagePassengerList[i]);
                    }
                    else
                    {
                        for (var j = apisMessageDividedPassengerList.Count(); j >= 0; j--)
                        {
                            if (apisMessageDividedPassengerList[j - 1].Substring(0, 6) == ("NAD+FL"))
                            { // NAD+FL:Passenger Name ve Surname bilgilerinin yazıldığı segment. Karakter sayısında aşım olduğu zaman yolcu bilgileri farklı partlarda olmasın diye bu kontrol eklendi. 
                                i = j - 1;
                                j = 0;
                            }
                        }
                        //UNH segmenti için part number eklendi
                        apisMessageHeaderList[1] = unb+sender + plus + recive + plus + nowDate + plus + partIdentifier + confirmation1 + "++" + recive + apos;
                        apisMessageHeaderList[2] = ung+sender + plus + (!string.IsNullOrEmpty(recive2) ? recive2 : recive) + plus + nowDate + plus + partIdentifier + confirmation2 + typeVersionNum + version + apos;
                        apisMessageHeaderList[3] = unh + confirmation3 + unhMid1 + version + unhMid2 + partIdentifier + confirmation4 + plus + partIdentifier.ToString().PadLeft(2, '0') + (partIdentifier == 1 ? ":C" : "") + apos;
                        //apisMessageList listesine Header-PassengerList-Foother bilgileri eklenip yazdırılıyor.
                        apisMessageList.AddRange(apisMessageHeaderList);
                        for (var a = 0; a < i; a++)
                        {
                            apisMessageList.Add(apisMessageDividedPassengerList[a]);
                            apisMessagePassengerList.Remove(apisMessageDividedPassengerList[a]);
                        }
                        //Multi part UNH segmentinde UNt segmentine kadar olan segment sayısı
                        fromUNHtoUNTTotalRow = apisMessageList.Count() - 3; // -3: UNA-UNB-UNG
                        apisMessageFootherList[1] = unt + partIdentifier + (fromUNHtoUNTTotalRow + 2) + plus + confirmation3 + apos; // +2: CNT-UNT
                        apisMessageFootherList[2] = une + partIdentifier + confirmation2 + apos;
                        apisMessageFootherList[3] = unz + partIdentifier + confirmation1 + apos;
                        apisMessageList.AddRange(apisMessageFootherList);
                        //partları ayırmak için eklendi.
                        apisMessageList.Add("*******************************************************************");
                        apisMessageDividedPassengerList = new List<string>();
                        apisMessagePassengerListCharecterCount = 0;
                        i = -1;
                        partIdentifier++;
                    }
                }
                if (apisMessageDividedPassengerList.Count > 0 && apisMessagePassengerListCharecterCount < (1900 - apisMessageHeaderListCharecterCount - apisMessageFootherListCharecterCount))
                {
                    //UNH segmenti için part number eklendi
                    apisMessageHeaderList[1] = unb+sender + plus + recive + plus + nowDate + plus + partIdentifier + confirmation1 + "++" + recive + apos;
                    apisMessageHeaderList[2] = ung+sender + plus + (!string.IsNullOrEmpty(recive2) ? recive2 : recive) + plus + nowDate + plus + partIdentifier + confirmation2 + typeVersionNum + version + apos;
                    apisMessageHeaderList[3] = unh + confirmation3 + unhMid1 + version + unhMid2 + partIdentifier + confirmation4 + plus + partIdentifier.ToString().PadLeft(2, '0') + ":F'";
                    //apisMessageList listesine Header-PassengerList-Foother bilgileri eklenip yazdırılıyor.
                    apisMessageList.AddRange(apisMessageHeaderList);
                    apisMessageList.AddRange(apisMessageDividedPassengerList);  
                    //Multi part UNH segmentinde UNt segmentine kadar olan segment sayısı
                    fromUNHtoUNTTotalRow = apisMessageList.Count() - 3; // -3: UNA-UNB-UNG
                    apisMessageFootherList[1] = unt + partIdentifier + (fromUNHtoUNTTotalRow + 2) + plus + confirmation3 + apos; // +2: CNT-UNT
                    apisMessageFootherList[2] = une + partIdentifier + confirmation2 + apos;
                    apisMessageFootherList[3] = unz + partIdentifier + confirmation1 + apos;
                    apisMessageList.AddRange(apisMessageFootherList);

                }
            }
            else
            {
                Logger.Default.Append(LogLevel.Error, string.Format(ErrorResource.PartIdentifierUnknown, exportRequest.PartType));
            }
            #endregion Multi Part
            apisMessageList.Add(partIdentifier.ToString().PadLeft(3, '0') + "PartIdentifier:");//PartIdentifier Mesaj liste eklyoruz.
            return apisMessageList;
        }
        /// <summary>
        /// Get Content Type
        /// </summary>
        /// <returns></returns>
        public string GetContentType()
        {
            return "text/plain";
        }
    }
}