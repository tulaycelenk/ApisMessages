using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AviatorManager.ViewModel.Aodb;
using Bridge.App.Core.Authentication;
using Bridge.Common;
using Bridge.Common.Logging;
using DCS.App.Service.Entity;
using DCS.App.Service.Helper;
using DCS.App.Service.Service.Dcs;
using DCS.ViewModel.Paging;
using System.Globalization;
using System.IO;

using static Bridge.Common.ParameterHelper;

namespace DCS.App.Service.Service.Exporter.PaxLst
{
    class ApisMessages : IExporter
    {
        private readonly CultureInfo enUs = new CultureInfo("en-Us");
        /// <summary>
        /// Denmark Apis
        /// </summary>
        /// <param name="flight"></param>
        /// <param name="singleOrMultiPaxLst"></param>
        /// <returns></returns>
        public byte[] Export(FlightResponse flight, string exporterType, bool singleOrMultiPaxLst)
        {
            IEnumerable<PassengerDocs> passengerDocsList = PassengerDocsService.GetPassengerWithDocsInformation(new PassengerDocsPagingRequest() { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 }).Item1.Where(p => (p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Boarded || p.PassengerStatusCode == ParameterHelper.PaxStatusCode.CheckedIn) && p.PassengerId != 0).OrderBy(s => s.Surname);
           
            
            IEnumerable<Passenger> passengerList = PassengerService.QueryPassengersJoined(new PassengerPagingRequest { FlightId = flight.Id }).Where(p => p.PaxStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PaxStatusCode == ParameterHelper.PaxStatusCode.Boarded || p.PaxStatusCode == ParameterHelper.PaxStatusCode.CheckedIn).OrderBy(s => s.Surname);


            IEnumerable<Baggage> passengerBaggageList = BaggageService.QueryBaggagesJoined(new BaggagePagingRequest { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 });

            using (var ms = new MemoryStream())
            {
                RestHelper rst = new RestHelper(new UrlProviderWithToken(), new HeaderProvider());
                var adesTimezone = rst.GetTimezoneOffsetByAirport(flight.AdesId, DateTime.UtcNow);
                var adesOfset = adesTimezone != null ? TimeSpan.FromSeconds(adesTimezone.GmtOffset).TotalSeconds : 0;
                var adepTimezone = rst.GetTimezoneOffsetByAirport(flight.AdepId, DateTime.UtcNow);
                var adepOfset = adepTimezone != null ? TimeSpan.FromSeconds(adepTimezone.GmtOffset).TotalSeconds : 0;

                if (adesTimezone == null)
                    Logger.Default.Append(LogLevel.Error, "35001 - Time Zone Values is not Found for " + flight.DestinationCountryIsoCode);
                else if (adesTimezone == null && adepTimezone == null)
                    Logger.Default.Append(LogLevel.Error, "35002 - There is not any Time Zone Value found for " + flight.DestinationCountryIsoCode + " and " + flight.DepartureCountryIsoCode + ". APIS messages build by using UTC dates.");
                TextWriter tw = new StreamWriter(ms);
                Random rnd = new Random();
                var enUs = new CultureInfo("en-US");
                var nowDate = DateTime.Now.ToString("yyMMdd", enUs) + ":" + DateTime.Now.ToString("HHmm", enUs);
                List<string> apisMessageList = new List<string>();
                List<string> apisMessageHeaderList = new List<string>();
                List<string> apisMessagePassengerList = new List<string>();
                List<string> apisMessageDividedPassengerList = new List<string>();
                List<string> apisMessageFootherList = new List<string>();
                int apisMessageHeaderListCharecterCount = 0;
                int apisMessagePassengerListCharecterCount = 0;
                int apisMessageFootherListCharecterCount = 0;
                int fromUNHtoUNTTotalRow = 0;
                int confirmation1 = rnd.Next(100000000, 999999999); //UNB ve UNZ için
                int confirmation2 = rnd.Next(100000000, 999999999); //UNG ve UNE için
                int confirmation3 = rnd.Next(100000000, 999999999); //UNH ve UNT için
                int partNumber = 1;
                bool segmentUna = false, segmentUnb = false, segmentUng = false, segmentUnh = false, segmentBgm745 = false, segmentBgm266 = false, segmentBgm266Cl = false, segmentRffTn = false, segmentNadMs = false, segmentCom = false, segmentTdt20 = false, segmentTdt34 = false, segmentLoc125 = false, segmentDtm189 = false, segmentLoc87 = false, segmentDtm232 = false, segmentNadFl = false, segmentAtt2 = false, segmentDtm329 = false, segmentMeaCt = false, segmentMeaWt = false, segmentGei = false, segmentFtx = false, segmentLoc178 = false, segmentLoc179 = false, segmentLoc22 = false, segmentNat2 = false, segmentRffAvf = false, segmentRffAbo = false, segmentRffSea = false, segmentRffCr = false, segmentDoc = false, segmentDtm36 = false, segmentLoc91 = false, segmentCnt42 = false, segmentUnt = false, segmentUne = false, segmentUnz = false;
                string recive = null; ;
                string version = null;

                #region Export Type
                if (exporterType == ParameterHelper.ExporterFactoryCode.BelgiumApis)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "BEGOVAPI";
                    version = "15B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.DenmarkApis)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentLoc22 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "DKGOVAPI";
                    version = "05B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.FinlandApis)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentLoc22 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "FIAPIS";
                    version = "05B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.FranceApis)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "APIPNRFR";
                    version = "02B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.JordanEdifactApis)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentRffTn = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentLoc22 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentRffCr = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "JORAPIS";
                    version = "12B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.IraqApis)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "IQGOVAPI";
                    version = "15B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.ItalyApis)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "BCSAPIS";
                    version = "02B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.LebenonApis)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "BCSAPIS";
                    version = "02B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.NetherlandsPaxLst)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "NLDAPIKM";
                    version = "02B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.RomaniaApis)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentLoc22 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "";
                    version = "05B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.RussiaApisPostDepartureForRussiaArrivalPaxLst)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm266 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "RUSAPIS";
                    version = "02B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.RussiaApisPostDepartureForRussiaOverflyPaxLst)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm266 = true; segmentNadMs = true; segmentCom = true; segmentTdt34 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "RUSAPIS";
                    version = "02B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.RussiaApisPreDepartureForRussiaArrivalPaxLst)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm266Cl = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "RUSAPIS";
                    version = "02B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.RussiaApisPreDepartureForRussiaOverflyPaxLst)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm266Cl = true; segmentNadMs = true; segmentCom = true; segmentTdt34 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "RUSAPIS";
                    version = "02B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.SwissEdifactApis)
                {
                    segmentUna = true; segmentUnb = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentLoc178 = true; segmentLoc179 = true; segmentNat2 = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "HDQCH2X";
                    version = "02B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.UkraineApis)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentLoc22 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "DKGOVAPI";
                    version = "05B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.QatarApis)
                {
                    segmentUna = true; segmentUnb = true; segmentUng = true; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentLoc22 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "";
                    version = "05B";
                }
                #endregion
                ///////////////
                else if (exporterType == ParameterHelper.ExporterFactoryCode.GermanyEdifactPaxLst)
                {
                    //Germany Apiste ung olmadığı için false 

                    segmentUna = true; segmentUnb = true; segmentUng = false; segmentUnh = true; segmentBgm745 = true; segmentNadMs = true; segmentCom = true; segmentTdt20 = true; segmentLoc125 = true; segmentDtm189 = true; segmentLoc87 = true; segmentDtm232 = true; segmentNadFl = true; segmentAtt2 = true; segmentDtm329 = true; segmentMeaCt = true; segmentGei = true; segmentFtx = true; segmentLoc178 = true; segmentLoc179 = true; segmentLoc22 = true; segmentNat2 = true; segmentRffAvf = true; segmentRffAbo = true; segmentRffSea = true; segmentDoc = true; segmentDtm36 = true; segmentLoc91 = true; segmentCnt42 = true; segmentUnt = true; segmentUne = true; segmentUnz = true;
                    recive = "BPOLAPIS";
                    version = "02B";
                }
                else if (exporterType == ParameterHelper.ExporterFactoryCode.GermanyCsvPaxLst)
                {
                    apisMessageList.Add(flight.AirlineIataCode + ";" + flight.FlightNumber.ToString().PadLeft(4, '0') + ";" + flight.DepartureIata + ";" + ((DateTime)flight.Sobt).AddSeconds(adepOfset).ToString("yyMMddHHmm", enUs) + ";"+ flight.DestinationIata + ";" + ((DateTime)flight.Sldt).AddSeconds(adesOfset == 0 ? adepOfset : adesOfset).ToString("yyMMddHHmm", enUs)+ ";" + passengerList.Count()+";;;;;;<CR+LF>");

                    if(passengerList.Count()<=99)
                    {
                        foreach (var passengerDocs in passengerDocsList)
                        {
                            apisMessageList.Add(passengerDocs.Surname + ";" + passengerDocs.Name + ";" + passengerDocs.GenderCode + ";" + passengerDocs.Dob.Value.ToString("yyMMdd") + ";" + passengerDocs.NationalityCode + ";" + flight.DepartureIata + ";" + flight.DestinationIata + ";" + passengerDocs.DocTypeCode + ";" + passengerDocs.DocNumber + ";" + passengerDocs.DocIssuerNationalityCode);


                            if (!string.IsNullOrEmpty(passengerDocs.DocNumber) || !string.IsNullOrEmpty(passengerDocs.DocTypeCode)) 
                                apisMessagePassengerList.Add((passengerDocs.DocTypeCode == "O" ? (!string.IsNullOrEmpty(passengerDocs.CustomDocTypeCode) ? (passengerDocs.CustomDocTypeCode.Substring(0, 1).ToUpper() == "I" || passengerDocs.CustomDocTypeCode.Substring(0, 1).ToUpper() == "P" ? passengerDocs.CustomDocTypeCode.Substring(0, 1) : (passengerDocs.CustomDocTypeCode.ToUpper() == "AC" ? passengerDocs.CustomDocTypeCode : "F")) : "F") : (!string.IsNullOrEmpty(passengerDocs.DocTypeCode) ? passengerDocs.DocTypeCode.Substring(0, 1).ToUpper() : "F")) + "+" + passengerDocs.DocNumber + "'");
                       

                        if (segmentDoc && (!string.IsNullOrEmpty(passengerDocs.DocNumber) || !string.IsNullOrEmpty(passengerDocs.DocTypeCode)))
                            apisMessagePassengerList.Add("DOC+" + (passengerDocs.DocTypeCode == "O" ? (!string.IsNullOrEmpty(passengerDocs.CustomDocTypeCode) ? (passengerDocs.CustomDocTypeCode.Substring(0, 1).ToUpper() == "I" || passengerDocs.CustomDocTypeCode.Substring(0, 1).ToUpper() == "P" ? passengerDocs.CustomDocTypeCode.Substring(0, 1) : (passengerDocs.CustomDocTypeCode.ToUpper() == "AC" ? passengerDocs.CustomDocTypeCode : "F")) : "F") : (!string.IsNullOrEmpty(passengerDocs.DocTypeCode) ? passengerDocs.DocTypeCode.Substring(0, 1).ToUpper() : "F")) + "+" + passengerDocs.DocNumber + "'");
                        }
                    }

                    }

                    #region UN/EDIFACT
                    if (segmentUna && (exporterType == ParameterHelper.ExporterFactoryCode.DenmarkApis))
                    apisMessageHeaderList.Add("UNA:+.?*'");
                if (segmentUna && (exporterType != ParameterHelper.ExporterFactoryCode.DenmarkApis))
                    apisMessageHeaderList.Add("UNA:+.? '");

                //Germany Apiste 

                if (segmentUnb && (exporterType != ParameterHelper.ExporterFactoryCode.JordanEdifactApis && exporterType != ParameterHelper.ExporterFactoryCode.NetherlandsPaxLst))
                    apisMessageHeaderList.Add("UNB+UNOA:4+BRIDGEIS" + "+" + recive + "+" + nowDate + "+" + confirmation1 + "'");
                if (segmentUnb && (exporterType == ParameterHelper.ExporterFactoryCode.JordanEdifactApis || exporterType == ParameterHelper.ExporterFactoryCode.NetherlandsPaxLst))
                  
                    apisMessageHeaderList.Add("UNB+UNOA:4+BRIDGEIS" + "+" + recive + "+" + nowDate + "+" + confirmation1 + "++" + recive + "'");
                
                //&& (exporterType != ParameterHelper.ExporterFactoryCode.GermanyEdifactPaxLst)
                if (segmentUng)
                    apisMessageHeaderList.Add("UNG+PAXLST+BRIDGEIS" + "+" + recive + "+" + nowDate + "+" + confirmation2 + "+UN+D:" + version + "'");
                //
                if (segmentUnh)
                    //":UN:IATA+" şeklinde olup ardından nowDate gerekli ama YYMMDDHHmmss şeklinde.
                    //Ayrıca 3 segmente ayrılmış segmentlerin kuralları var "+01" "+02" "+03" olabiliyor. F ve C harfi koyulabiliyor ne olduklarını araştır.
                    apisMessageHeaderList.Add("UNH+" + confirmation3 + "+PAXLST:D:" + version + ":UN:IATA'");
                if (segmentBgm745)
                    apisMessageHeaderList.Add("BGM+745'");
                if (segmentBgm266)
                    apisMessageHeaderList.Add("BGM+266+CLOB'");
                if (segmentBgm266Cl)
                    apisMessageHeaderList.Add("BGM+266+CL'");
                if (segmentRffTn)
                    apisMessageHeaderList.Add("RFF+TN:BR" + rnd.Next(100000000, 999999999) + "'");
                if (segmentNadMs)
                    apisMessageHeaderList.Add("NAD+MS+++DEGIRMENCI:GAMZE'");
                if (segmentCom)
                    apisMessageHeaderList.Add("COM+212 211 1003:TE+212 211 1004:FX+SUPPORT BRIDGEIST COM:EM'");
                if (segmentTdt20)
                    apisMessageHeaderList.Add("TDT+20+" + flight.AirlineIataCode + flight.FlightNumber.ToString().PadLeft(4, '0') + "+++" + flight.AirlineIataCode + "'");
                if (segmentTdt34)
                    apisMessageHeaderList.Add("TDT+34+" + flight.AirlineIataCode + flight.FlightNumber.ToString().PadLeft(4, '0') + "+++" + flight.AirlineIataCode + "'");
                if (segmentLoc125 && !string.IsNullOrEmpty(flight.DepartureIata))
                    apisMessageHeaderList.Add("LOC+125+" + flight.DepartureIata + "'");
                if (segmentDtm189 && flight.Sobt.HasValue)
                    apisMessageHeaderList.Add("DTM+189:" + ((DateTime)flight.Sobt).AddSeconds(adepOfset).ToString("yyMMddHHmm", enUs) + ":201'");
                if (segmentLoc87 && !string.IsNullOrEmpty(flight.DestinationIata))
                    apisMessageHeaderList.Add("LOC+87+" + flight.DestinationIata + "'");
                if (segmentDtm232 && flight.Sldt.HasValue)
                    apisMessageHeaderList.Add("DTM+232:" + ((DateTime)flight.Sldt).AddSeconds(adesOfset == 0 ? adepOfset : adesOfset).ToString("yyMMddHHmm", enUs) + ":201'");
                    #endregion
                    foreach (var passengerDocs in passengerDocsList)
                {
                    var passengerDoco = PassengerDocoService.GetPassengerDoco(passengerDocs.PassengerId).FirstOrDefault();
                    var passengerDoca = PassengerDocaService.GetPassengerDocaWithPaging(new PassengerDocaPagingRequest { PassengerId = passengerDocs.PassengerId, PageNumber = 1, PageSize = 2000 }).Item1.FirstOrDefault();
                    var passenger = passengerList.Where(filter => filter.Id == passengerDocs.PassengerId).FirstOrDefault();
                    var passengerBaggages = passengerBaggageList.Where(filter => filter.PassengerId == passengerDocs.PassengerId).OrderBy(s => s.BagTag);

                    if (segmentNadFl && !string.IsNullOrEmpty(passengerDocs.Surname))
                        apisMessagePassengerList.Add("NAD+FL+++" + (!string.IsNullOrEmpty(passengerDocs.Surname) ? (passengerDocs.Surname.Length > 35 ? passengerDocs.Surname.Substring(0, 35) : passengerDocs.Surname) : "") + (!string.IsNullOrEmpty(passengerDocs.Name) ? (":" + (passengerDocs.Name.Length > 35 ? passengerDocs.Name.Substring(0, 35) : passengerDocs.Name).Replace(" ", ":")) : "") + (passengerDoca != null ? ("+" + (!string.IsNullOrEmpty(passengerDoca.ResidenceAddress) ? passengerDoca.ResidenceAddress.Length > 35 ? passengerDoca.ResidenceAddress.Substring(0, 35) : passengerDoca.ResidenceAddress : "") + "+" + passengerDoca.ResidenceCity + "+" + passengerDoca.ResidenceZipCode + "+" + passengerDoca.ResidenceCountryIso3Code) : "") + "'");
                    if (segmentAtt2 && !string.IsNullOrEmpty(passengerDocs.GenderCode))
                        apisMessagePassengerList.Add("ATT+2++" + (passengerDocs.GenderCode == PaxGenderCode.FemaleChild ? "F" : (passengerDocs.GenderCode == PaxGenderCode.MaleChild ? "M" : (passengerDocs.GenderCode == PaxGenderCode.Child || passengerDocs.GenderCode == PaxGenderCode.Infant ? "U" : passengerDocs.GenderCode))) + "'");
                    else
                        apisMessagePassengerList.Add("ATT+2++U'");
                    if (segmentDtm329 && passengerDocs.Dob.HasValue)
                        apisMessagePassengerList.Add("DTM+329:" + passengerDocs.Dob.Value.ToString("yyMMdd") + "'");
                    if (segmentMeaCt && passenger != null && passenger.BagCount > 0)
                        apisMessagePassengerList.Add("MEA+CT++:" + passenger.BagCount + "'");
                    if (segmentMeaWt && passenger != null && passenger.BagTotalWeight > 0)
                        apisMessagePassengerList.Add("MEA+WT++KGM:" + passenger.BagTotalWeight + "'");
                    if (segmentGei)
                        apisMessagePassengerList.Add("GEI+4+173'");
                    if (segmentFtx && passengerBaggages != null)
                    {
                        bool isSequentialBagTag = false; int sequentialBagTagNumber = 1; string sequentialBagTagText = null;
                        for (var i = 0; i < passengerBaggages.Count(); i++)
                        {
                            if (i < passengerBaggages.Count() - 1 && Convert.ToInt32(passengerBaggages.ElementAt(i).BagTag) + 1 == Convert.ToInt32(passengerBaggages.ElementAt(i + 1).BagTag))
                            {
                                sequentialBagTagNumber++;
                                isSequentialBagTag = true;
                                sequentialBagTagText = ("FTX+BAG+++" + flight.AirlineIataCode + passengerBaggages.ElementAt(i - sequentialBagTagNumber + 2).BagTag + ":" + sequentialBagTagNumber + "'");
                            }
                            else
                            {
                                if (isSequentialBagTag)
                                    apisMessagePassengerList.Add(sequentialBagTagText);
                                else
                                    apisMessagePassengerList.Add("FTX+BAG+++" + flight.AirlineIataCode + passengerBaggages.ElementAt(i).BagTag + "'");

                                isSequentialBagTag = false; sequentialBagTagText = null; sequentialBagTagNumber = 1;
                            }
                        }
                        if (isSequentialBagTag)
                            apisMessagePassengerList.Add(sequentialBagTagText);
                    }
                    if (segmentLoc22 && !string.IsNullOrEmpty(flight.DestinationIata))
                        apisMessagePassengerList.Add("LOC+22+" + flight.DestinationIata + "'");
                    if (segmentLoc178 && (!string.IsNullOrEmpty(flight.DepartureIata)))
                        apisMessagePassengerList.Add("LOC+178+" + flight.DepartureIata + "'");
                    if (segmentLoc179 && (!string.IsNullOrEmpty(flight.DestinationIata)))
                        apisMessagePassengerList.Add("LOC+179+" + flight.DestinationIata + "'");
                    //COM+202 628 9292:TE+202 628 4998:FX+davidsonr.at.iata.org:EM' Indicates telephone number, fax number and email address of the traveller ----> SSR da tutuluyor.
                    //EMP+1+CR1:110:111’ Indicates current passenger is a cockpit crew ----> Cockpit Crew  bilgisi grilmediğin için yapılmayacak.
                    if (segmentNat2 && !string.IsNullOrEmpty(passengerDocs.NationalityCode))
                        apisMessagePassengerList.Add("NAT+2+" + passengerDocs.NationalityCode + "'");
                    if (segmentRffAvf && !string.IsNullOrEmpty(passengerDocs.PassengerPnr))
                        apisMessagePassengerList.Add("RFF+AVF:" + passengerDocs.PassengerPnr + "'");
                    if (segmentRffAbo && passengerDocs.PassengerId > 0)
                        apisMessagePassengerList.Add("RFF+ABO:BR" + passengerDocs.PassengerId.ToString().PadLeft(10, '0') + "'");
                    if (segmentRffSea && !string.IsNullOrEmpty(passengerDocs.PassengerSeatNumber))
                        apisMessagePassengerList.Add("RFF+SEA:" + passengerDocs.PassengerSeatNumber.PadLeft(3, '0') + "'");
                    if (segmentRffCr && passenger != null && !string.IsNullOrEmpty(passenger.Fqtv))
                        apisMessagePassengerList.Add("RFF+CR:" + passenger.Fqtv + "'");
                    if (segmentDoc && (!string.IsNullOrEmpty(passengerDocs.DocNumber) || !string.IsNullOrEmpty(passengerDocs.DocTypeCode)))
                        apisMessagePassengerList.Add("DOC+" + (passengerDocs.DocTypeCode == "O" ? (!string.IsNullOrEmpty(passengerDocs.CustomDocTypeCode) ? (passengerDocs.CustomDocTypeCode.Substring(0, 1).ToUpper() == "I" || passengerDocs.CustomDocTypeCode.Substring(0, 1).ToUpper() == "P" ? passengerDocs.CustomDocTypeCode.Substring(0, 1) : (passengerDocs.CustomDocTypeCode.ToUpper() == "AC" ? passengerDocs.CustomDocTypeCode : "F")) : "F") : (!string.IsNullOrEmpty(passengerDocs.DocTypeCode) ? passengerDocs.DocTypeCode.Substring(0, 1).ToUpper() : "F")) + "+" + passengerDocs.DocNumber + "'");
                    if (segmentDtm36 && passengerDocs.Doe.HasValue)
                        apisMessagePassengerList.Add("DTM+36:" + passengerDocs.Doe.Value.ToString("yyMMdd") + "'");
                    if (segmentLoc91 && !string.IsNullOrEmpty(passengerDocs.DocIssuerNationalityCode))
                        apisMessagePassengerList.Add("LOC+91+" + (passengerDocs.DocIssuerNationalityCode) + "'");
                    if (passengerDoco != null)
                    {
                        if (segmentDoc && (!string.IsNullOrEmpty(passengerDoco.DocNumber) || !string.IsNullOrEmpty(passengerDoco.DocTypeCode)))
                            apisMessagePassengerList.Add("DOC+" + (passengerDoco.DocTypeCode == "O" ? (!string.IsNullOrEmpty(passengerDoco.CustomDocTypeCode) ? (passengerDoco.CustomDocTypeCode.Substring(0, 1).ToUpper() == "I" || passengerDoco.CustomDocTypeCode.Substring(0, 1).ToUpper() == "P" ? passengerDoco.CustomDocTypeCode.Substring(0, 1) : (passengerDoco.CustomDocTypeCode.ToUpper() == "AC" ? passengerDoco.CustomDocTypeCode : "F")) : "F") : (!string.IsNullOrEmpty(passengerDoco.DocTypeCode) ? passengerDoco.DocTypeCode.Substring(0, 1).ToUpper() : "F")) + "+" + passengerDoco.DocNumber + "'");
                        if (segmentDtm36 && passengerDoco.Doe.HasValue)
                            apisMessagePassengerList.Add("DTM+36:" + passengerDoco.Doe.Value.ToString("yyMMdd") + "'");
                        if (segmentLoc91 && !string.IsNullOrEmpty(passengerDoco.DocForNationalityCode))
                            apisMessagePassengerList.Add("LOC+91+" + (passengerDoco.DocForNationalityCode) + "'");
                    }
                }
                if (segmentCnt42)
                    apisMessageFootherList.Add("CNT+42:" + passengerList.Count() + "'");
                if (segmentUnt)
                    apisMessageFootherList.Add("UNT+" + fromUNHtoUNTTotalRow + "+" + confirmation3 + "'");
                if (segmentUne)
                    apisMessageFootherList.Add("UNE+1+" + confirmation2 + "'");
                if (segmentUnz)
                    apisMessageFootherList.Add("UNZ+1+" + confirmation1 + "'");

                foreach (var apisMessageHeader in apisMessageHeaderList)
                {
                    apisMessageHeaderListCharecterCount = apisMessageHeaderListCharecterCount + apisMessageHeader.Length;
                }
                foreach (var apisMessageFoother in apisMessageFootherList)
                {
                    apisMessageFootherListCharecterCount = apisMessageFootherListCharecterCount + apisMessageFoother.Length;
                }

                #region Single Part
                if (!singleOrMultiPaxLst)
                {
                    //apisMessageList listesine Header-PassengerList-Foother bilgileri eklenip yazdırılıyor.
                    apisMessageList.AddRange(apisMessageHeaderList);
                    apisMessageList.AddRange(apisMessagePassengerList);
                    //Multi part UNH segmentinde UNt segmentine kadar olan segment sayısı
                    fromUNHtoUNTTotalRow = apisMessageList.Count() - 3; // -3: UNA-UNB-UNG
                    apisMessageFootherList[1] = "UNT+" + (fromUNHtoUNTTotalRow + 2) + "+" + confirmation3 + "'"; // +2: CNT-UNT
                    apisMessageList.AddRange(apisMessageFootherList);
                    foreach (var apisMessage in apisMessageList)
                        tw.WriteLine(apisMessage);
                }
                #endregion Single Part

                #region Multi Part
                else
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
                            //apisMessageList listesine Header-PassengerList-Foother bilgileri eklenip yazdırılıyor.
                            apisMessageList.AddRange(apisMessageHeaderList);
                            for (var a = 0; a < i; a++)
                            {
                                apisMessageList.Add(apisMessageDividedPassengerList[a]);
                                apisMessagePassengerList.Remove(apisMessageDividedPassengerList[a]);
                            }
                            //Multi part UNH segmentinde UNt segmentine kadar olan segment sayısı
                            fromUNHtoUNTTotalRow = apisMessageList.Count() - 3; // -3: UNA-UNB-UNG
                            apisMessageFootherList[1] = "UNT+" + (fromUNHtoUNTTotalRow + 2) + "+" + confirmation3 + "'"; // +2: CNT-UNT
                            apisMessageList.AddRange(apisMessageFootherList);

                            foreach (var apisMessage in apisMessageList)
                                tw.WriteLine(apisMessage);

                            tw.WriteLine("");
                            apisMessageList = new List<string>();
                            apisMessageDividedPassengerList = new List<string>();
                            apisMessagePassengerListCharecterCount = 0;
                            i = -1;
                            partNumber++;
                        }
                    }
                    if (apisMessageDividedPassengerList.Count > 0 && apisMessagePassengerListCharecterCount < (1900 - apisMessageHeaderListCharecterCount - apisMessageFootherListCharecterCount))
                    {
                        //apisMessageList listesine Header-PassengerList-Foother bilgileri eklenip yazdırılıyor.
                        apisMessageList.AddRange(apisMessageHeaderList);
                        apisMessageList.AddRange(apisMessageDividedPassengerList);
                        //Multi part UNH segmentinde UNt segmentine kadar olan segment sayısı
                        fromUNHtoUNTTotalRow = apisMessageList.Count() - 3; // -3: UNA-UNB-UNG
                        apisMessageFootherList[1] = "UNT+" + (fromUNHtoUNTTotalRow + 2) + "+" + confirmation3 + "'"; // +2: CNT-UNT
                        apisMessageList.AddRange(apisMessageFootherList);
                        foreach (var apisMessage in apisMessageList)
                            tw.WriteLine(apisMessage);
                    }
                }
                #endregion Multi Part

                tw.Flush();
                ms.Position = 0;
                var fileSingle = Encoding.UTF8.GetString(ms.ToArray());
                Logger.Default.Append(LogLevel.Debug, fileSingle);
                ms.Position = 0;
                return ms.ToArray();
            }
        }
        /// <summary>
        /// Get Content Type
        /// </summary>
        /// <returns></returns>
        public string GetContentType()
        {
            return "text/plain";
        }
        /// <summary>
        /// Get File Name
        /// </summary>
        /// <param name="flight"></param>
        /// <returns></returns>
        public string GetFileName(FlightResponse flight)
        {
            return flight.AirlineIataCode + flight.FlightNumber + (flight.AdepFlightDate.HasValue ? flight.AdepFlightDate.Value.ToString("ddMMMyyyy", enUs).ToUpper() : flight.AdesFlightDate.Value.ToString("ddMMMyyyy", enUs).ToUpper()) + flight.DepartureIata + flight.DestinationIata + "ApisFileFile.txt";
        }
    }
}