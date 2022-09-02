using AviatorManager.ViewModel.Aodb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApisMessages
{
    public class Version05B : ApisEdifactHelper
    {
        //versiyonlar helper ı içerecek tw a atılacak ve ülkeler bunu kullanacak 
        //ülkere göre değişiklikler ülkelerde yapılacak 
        public byte[] Export(FlightResponse flight, string exporterType, bool singleOrMultiPaxLst)
        {

            var pagingRequest = new PassengerDocsPagingRequest()
            {
                FlightId = flight.Id,
                PageNumber = 1,
                PageSize = 2000
            };
            IEnumerable<PassengerDocs> passengerDocsListAll = PassengerDocsService.GetPassengerWithDocsInformation(pagingRequest).Item1;
            IEnumerable<PassengerDocs> passengerDocsList = new List<PassengerDocs>();

            if (passengerDocsListAll == null || !passengerDocsListAll.Any())
                throw ValidationException.ValidationFailure(ErrorResource.PassengerApisNotFound);

            if (passengerDocsListAll != null)
                passengerDocsList = passengerDocsListAll.Where(p => p.PassengerStatusCode == Bridge.Common.ParameterHelper.PaxStatusCode.Flown || p.PassengerStatusCode == Bridge.Common.ParameterHelper.PaxStatusCode.Boarded || p.PassengerStatusCode == Bridge.Common.ParameterHelper.PaxStatusCode.CheckedIn);

            if (!passengerDocsList.Any())
                throw ValidationException.ValidationFailure(ErrorResource.NoFlownPassenger);
            using (var ms = new MemoryStream())
            {
                TextWriter tw = new StreamWriter(ms);
                var enUs = new CultureInfo("en-US");
                var airlineName = flight.AirlineName.Trim().Replace(" ", string.Empty);
                airlineName = !string.IsNullOrEmpty(airlineName) ? (airlineName.Length > 35 ? airlineName = airlineName.Substring(0, 35) : airlineName) : "";

                var sobt = flight.Sobt.HasValue ? flight.Sobt.Value.ToString("yyMMddHHmm", enUs) : "";
                var sldt = flight.Sldt.HasValue ? flight.Sldt.Value.ToString("yyMMddHHmm", enUs) : "";

                var yearToMin = DateTime.Now.ToString("yyMMdd", enUs) + ":" + DateTime.Now.ToString("HHmm", enUs);
                var yearToDay = DateTime.Now.ToString("yyMMdd", enUs);
                RestHelper rst = new RestHelper(new UrlProviderWithToken(), new HeaderProvider());
                var adepTimezone = rst.GetTimezoneOffsetByAirport(flight.AdepId, DateTime.UtcNow);
                var adepOfset = adepTimezone != null ? TimeSpan.FromSeconds(adepTimezone.GmtOffset).TotalSeconds : 0;

                if (!singleOrMultiPaxLst)
                {
                    #region Single Part PAXLST
                    tw.WriteLine(una);
                    tw.WriteLine(unb + airlineName + yearToMin + plus + BZcode + unbEnd);
                    tw.WriteLine(ung + airlineName + "{receive}" +yearToMin + plus + flight.DestinationIata + GEcode + ungEnd + version05B);
                    tw.WriteLine(unh + HTcode + unhMid1 + version05B + unhMid2);
                    tw.WriteLine(bgm745);
                    tw.WriteLine(nadMs);
                    tw.WriteLine(com);
                    tw.WriteLine(tdt20 + flight.AirlineIataCode + flight.FlightNumber.ToString() + threePlus + flight.AirlineIataCode + apos);
                    tw.WriteLine(loc125 + flight.DepartureIata + apos);
                    tw.WriteLine(dtm189 + ((DateTime)flight.Sobt).AddSeconds(adepOfset).ToString("yyMMddHHmm", enUs) + dtm189End);
                    tw.WriteLine(loc87 + flight.DestinationIata + apos);
                    tw.WriteLine(dtm232 + yearToDay);

                    foreach (var passengerDocs in passengerDocsList)
                    {
                        var passengerDoco = PassengerDocoService.GetPassengerDoco(passengerDocs.PassengerId).FirstOrDefault();
                        var passengerDoca = PassengerDocaService.GetPassengerDocaWithPaging(new PassengerDocaPagingRequest { PassengerId = passengerDocs.PassengerId, PageNumber = 1, PageSize = 2000 }).Item1.FirstOrDefault();
                        var passenger = passengerList.Where(filter => filter.Id == passengerDocs.PassengerId).FirstOrDefault();
                        var passengerBaggages = passengerBaggageList.Where(filter => filter.PassengerId == passengerDocs.PassengerId).OrderBy(s => s.BagTag);
                    }

                    tw.WriteLine(cnt42 + passengerDocsList.Count() + apos);

                    int totalRow = (passengerDocsList.Count() * 10) + 9;
                    tw.WriteLine(unt + totalRow + plus + HTcode+apos);
                                        
                    tw.WriteLine(une + GEcode + apos);
                    tw.WriteLine(unz+BZcode + apos);
                    string deneme = $"vkjfdnv{ }";
                }
            }
        }
    }  
}