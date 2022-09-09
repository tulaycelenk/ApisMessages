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
            IEnumerable<PassengerDocs> passengerDocsList = PassengerDocsService.GetPassengerWithDocsInformation(new PassengerDocsPagingRequest() { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 }).Item1.Where(p => (p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Boarded) && p.PassengerId != 0).OrderBy(s => s.Surname);
            
            IEnumerable<Passenger> passengerList = PassengerService.QueryPassengersJoined(new PassengerPagingRequest { FlightId = flight.Id }).Where(p => p.PaxStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PaxStatusCode == ParameterHelper.PaxStatusCode.Boarded || p.PaxStatusCode == ParameterHelper.PaxStatusCode.CheckedIn).OrderBy(s => s.Surname);
           
            IEnumerable<Baggage> passengerBaggageList = BaggageService.QueryBaggagesJoined(new BaggagePagingRequest { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 });

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
                    tw.WriteLine(ung + airlineName + "{receive}" + yearToMin + plus + flight.DestinationIata + GEcode + ungEnd + version05B);
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
                        var passengerDoca = PassengerDocaService.GetPassengerDocaWithPaging(new PassengerDocaPagingRequest { PassengerId = passengerDocs.PassengerId, PageNumber = 1, PageSize = 20
                            00 }).Item1.FirstOrDefault();
                        var passenger = passengerList.Where(filter => filter.Id == passengerDocs.PassengerId).FirstOrDefault();
                        var passengerBaggages = passengerBaggageList.Where(filter => filter.PassengerId == passengerDocs.PassengerId).OrderBy(s => s.BagTag);
                        //Name NAD

                        //Gender ATT
                        tw.WriteLine(passengerDocs.GenderCode == PaxGenderCode.FemaleChild || passengerDocs.GenderCode == PaxGenderCode.Female  ? attF : (passengerDocs.GenderCode == PaxGenderCode.MaleChild || passengerDocs.GenderCode == PaxGenderCode.Male ? attM : (passengerDocs.GenderCode == PaxGenderCode.Child || passengerDocs.GenderCode == PaxGenderCode.Infant ? attU )));
                        //Date of Birth DTM
                        tw.WriteLine(dtm329+passengerDocs.Dob.Value.ToString("yyMMdd")+apos);

                        //Baggage MEA
                        tw.WriteLine(meaCt + passenger.BagCount + apos);

                        //the passenger verified or not
                        tw.WriteLine(gei173);//tw.WriteLine(gei174);

                        //baggages tags sequential or not
                        if (passengerBaggages != null)
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
                        //destination
                        tw.WriteLine(loc178 + flight.DepartureIata + apos);
                        //arrival
                        tw.WriteLine(loc179 + flight.DestinationIata + apos);

                        //intransit passenger
                        tw.WriteLine(loc22 + flight.DestinationIata + apos);

                        //residense
                        tw.WriteLine(loc174 + passengerDoca.ResidenceCountryIso3Code + apos);

                        //communication 
                        //nationality
                        tw.WriteLine(nat2+passengerDocs.NationalityCode + apos);

                        //place of birth
                        tw.WriteLine(+ apos);

                        //communication number of the passenger
                        tw.WriteLine(+ apos);

                        //nationality 
                        tw.WriteLine(+ apos);

                        //passenger reservation number
                        tw.WriteLine(+ apos);
                        //unique passenger reference number(rffAbo)
                        tw.WriteLine(+ apos);

                        //assigned seat(rffSea)
                        tw.WriteLine(+ apos);
                        //governement agency reference number (rffAea)
                        tw.WriteLine(+ apos);

                        //doc type (docp or docv)
                        tw.WriteLine(+ apos);

                        // expiry date of the official travel doc dtm36
                        tw.WriteLine(+ apos);
                        //issue date of the other doc used for travel dtm182
                        tw.WriteLine(+ apos);

                        //issuing country code loc91
                        tw.WriteLine(+ apos);

                        //issued city loc91 with three colon
                        tw.WriteLine(+ apos);





                    }

                    tw.WriteLine(cnt42 + passengerDocsList.Count() + apos);
                    int totalRow = (passengerDocsList.Count() * 10) + 9;
                    tw.WriteLine(unt + totalRow + plus + HTcode + apos);
                    tw.WriteLine(une + GEcode + apos);
                    tw.WriteLine(unz + BZcode + apos);
                    string deneme = $"vkjfdnv{airlineName }";
                }
            }
        }
    }
}