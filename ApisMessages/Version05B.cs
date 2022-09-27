using AviatorManager.ViewModel.Aodb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApisMessages
{
    //PAXLST IMPLEMENTATION GUIDE ANNEX III 28 June 2010
    public class Version05B : ApisEdifactHelper
    {
        //versiyonlar helper ı içerecek tw a atılacak ve ülkeler bunu kullanacak 
        //ülkere göre değişiklikler ülkelerde yapılacak 
        public byte[] Export(FlightResponse flight, string exporterType, bool singleOrMultiPaxLst)
        {
            IEnumerable<PassengerDocs> passengerDocsList = PassengerDocsService.GetPassengerWithDocsInformation(new PassengerDocsPagingRequest() { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 }).Item1.Where(p => (p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Boarded) && p.PassengerId != 0).OrderBy(s => s.Surname);

            IEnumerable<Passenger> passengerList = PassengerService.QueryPassengersJoined(new PassengerPagingRequest { FlightId = flight.Id }).Where(p => p.PaxStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PaxStatusCode == ParameterHelper.PaxStatusCode.Boarded || p.PaxStatusCode == ParameterHelper.PaxStatusCode.CheckedIn).OrderBy(s => s.Surname);

            IEnumerable<Baggage> passengerBaggageList = BaggageService.QueryBaggagesJoined(new BaggagePagingRequest { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 });


                TextWriter tw = new StreamWriter(ms);
                var airlineName = flight.AirlineName.Trim().Replace(" ", string.Empty);
                airlineName = !string.IsNullOrEmpty(airlineName) ? (airlineName.Length > 35 ? airlineName = airlineName.Substring(0, 35) : airlineName) : "";

                var sobt = flight.Sobt.HasValue ? flight.Sobt.Value.ToString("yyMMddHHmm", enUs) : "";
                var sldt = flight.Sldt.HasValue ? flight.Sldt.Value.ToString("yyMMddHHmm", enUs) : "";

                
                RestHelper rst = new RestHelper(new UrlProviderWithToken(), new HeaderProvider());
                var adepTimezone = rst.GetTimezoneOffsetByAirport(flight.AdepId, DateTime.UtcNow);
                var adepOfset = adepTimezone != null ? TimeSpan.FromSeconds(adepTimezone.GmtOffset).TotalSeconds : 0;

                if (!singleOrMultiPaxLst)
                {
                    #region Single Part PAXLST
                    tw.WriteLine(una);
                    tw.WriteLine(unb + sender + airlineName + yearToMin + plus + BZcode+apos);
                    tw.WriteLine(ung + airlineName + yearToMin + plus + GEcode + typeVersionNum + version05B);
                    tw.WriteLine(unh + HTcode + unhMid1 + version05B + unhMid2);
                    tw.WriteLine(bgm745);
                    tw.WriteLine(nadMs);
                    tw.WriteLine(com);
                    //BURAYA FLIGHT KONTROLLERİ İÇİN İF GELSİN Mİ?
                    tw.WriteLine(tdt20 + flight.AirlineIataCode + flight.FlightNumber.ToString() + threePlus + flight.AirlineIataCode + apos);
                    tw.WriteLine(loc125 + flight.DepartureIata + apos);
                    //YUKARIDA SOBT TANIMLADIK
                    tw.WriteLine(dtm189 + ((DateTime)flight.Sobt).AddSeconds(adepOfset).ToString("yyMMddHHmm", enUs) + dtm189End);
                    tw.WriteLine(loc87 + flight.DestinationIata + apos);
                    tw.WriteLine(dtm232 + yearToDay);

                    foreach (var passengerDocs in passengerDocsList)
                    {
                        var passengerDoco = PassengerDocoService.GetPassengerDoco(passengerDocs.PassengerId).FirstOrDefault();
                        var passengerDoca = PassengerDocaService.GetPassengerDocaWithPaging(new PassengerDocaPagingRequest { PassengerId = passengerDocs.PassengerId, PageNumber = 1, PageSize = 2000 }).Item1.FirstOrDefault();
                        var passenger = passengerList.Where(filter => filter.Id == passengerDocs.PassengerId).FirstOrDefault();
                        var passengerBaggages = passengerBaggageList.Where(filter => filter.PassengerId == passengerDocs.PassengerId).OrderBy(s => s.BagTag);
                        //Name NAD
                        //ELLE GİRİLEN BİLGİLERLE DÖKÜMANDAKİ BİLGİLER KIYASLANIYOR MU. YANLIŞ GİRİLDİYSE ÖRNEĞİN PASAPORTTA YAZAN GİBİ DEĞİLSE? PASAPORT OKUYUCULARINDAN GELEN İSMİ ALIYOR MUYUZ? KARŞILAŞTIRIYOR MUYUZ?
                        if (!string.IsNullOrEmpty(passengerDocs.Surname))
                            tw.WriteLine(nadFl + (passengerDocs.Surname.Length > 35 ? passengerDocs.Surname.Substring(0, 35) : passengerDocs.Surname) : "") + (!string.IsNullOrEmpty(passengerDocs.Name) ? (":" + (passengerDocs.Name.Length > 35 ? passengerDocs.Name.Substring(0, 35) : passengerDocs.Name).Replace(" ", ":")) : "") + (passengerDoca != null ? ("+" + (!string.IsNullOrEmpty(passengerDoca.ResidenceAddress) ? passengerDoca.ResidenceAddress.Length > 35 ? passengerDoca.ResidenceAddress.Substring(0, 35) : passengerDoca.ResidenceAddress : "") + "+" + passengerDoca.ResidenceCity + "+" + passengerDoca.ResidenceZipCode + "+" + passengerDoca.ResidenceCountryIso3Code) : "") + apos);


                        //Gender ATT
                        //passengerDocs.GenderCod UI DAN GELEN VERİNİN KARŞILIĞI MI?
                        tw.WriteLine(passengerDocs.GenderCode == PaxGenderCode.FemaleChild || passengerDocs.GenderCode == PaxGenderCode.Female ? attF : (passengerDocs.GenderCode == PaxGenderCode.MaleChild || passengerDocs.GenderCode == PaxGenderCode.Male ? attM : (passengerDocs.GenderCode == PaxGenderCode.Child || passengerDocs.GenderCode == PaxGenderCode.Infant ? attU )));
                        //Date of Birth DTM
                        tw.WriteLine(dtm329 + passengerDocs.Dob.Value.ToString("yyMMdd") + apos);

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
                        if (!string.IsNullOrEmpty(flight.DepartureIata))
                            tw.WriteLine(loc178 + flight.DepartureIata + apos);

                        //intransit passenger
                        if (!string.IsNullOrEmpty(flight.DestinationIata))
                            tw.WriteLine(loc22 + flight.DestinationIata + apos);

                        //arrival
                        if (!string.IsNullOrEmpty(flight.DestinationIata))
                            tw.WriteLine(loc179 + flight.DestinationIata + apos);

                        //residense
                        if (!string.IsNullOrEmpty(passengerDoca.ResidenceCountryIso3Code))
                            tw.WriteLine(loc174 + passengerDoca.ResidenceCountryIso3Code + apos);

                        //place of birth
                        tw.WriteLine(loc180 + threeColon + PLACE OF BIRTH + apos);

                        //communication number of the passenger
                        tw.WriteLine(com + NUMBER:TE + apos);

                        //nationality
                        if (!string.IsNullOrEmpty(passengerDocs.NationalityCode))
                            tw.WriteLine(nat2 + passengerDocs.NationalityCode + apos);

                        //passenger reservation number
                        tw.WriteLine(rffAvf + RESERVATION NUMBER + apos);

                        //unique passenger reference number(rffAbo)
                        tw.WriteLine(rffAbo + PASSENGER REFERENCE NUM + apos);

                        //assigned seat(rffSea)
                        if (!string.IsNullOrEmpty(passengerDocs.PassengerSeatNumber))
                            tw.WriteLine(rffSea + passengerDocs.PassengerSeatNumber.PadLeft(3, '0') + apos);

                        //governement agency reference number (rffAea)
                        tw.WriteLine(rffAea + GOVERNEMENT AGENCY REF NUMBER apos);

                        //doc type (docp or docv)
                        if (!string.IsNullOrEmpty(passengerDocs.DocNumberr) || !string.IsNullOrEmpty(passengerDocs.DocTypeCode)) { 
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

                        // expiry date of the official travel doc dtm36
                        if (passengerDoco.Doe.HasValue)
                            tw.WriteLine(dtm36 + passengerDoco.Doe.Value.ToString("yyMMdd") + apos);

                        //issue date of the other doc used for travel dtm182
                        //ONAYLANMA TARİHİ TABLOLARDA YOK
                        tw.WriteLine(dtm182 + OTHER DOCUMENTs ISSUE DATE + apos);

                        //issuing country code loc91
                        if (!string.IsNullOrEmpty(passengerDoco.DocForNationalityCode))
                            tw.WriteLine(loc91 + passengerDoco.DocForNationalityCode + apos);

                        //issued city loc91 with three colon
                        //DÖKUMANIN ONAYLANDIĞI ŞEHİR TABLOLARDA YOK
                        tw.WriteLine(loc91 + threeColon + doc FOR CITY CODE + apos);

                    }

                    tw.WriteLine(cnt42 + passengerDocsList.Count() + apos);
                    tw.WriteLine(unt + $"{fromUNHtoUNTTotalRow}" + plus + HTcode + apos);
                    tw.WriteLine(une + GEcode + apos);
                    tw.WriteLine(unz + BZcode + apos);
                    //string deneme = $"vkjfdnv{airlineName }";
                }
                tw.Flush();
                ms.Position = 0;
                var fileSingle = Encoding.UTF8.GetString(ms.ToArray());
                Logger.Default.Append(LogLevel.Debug, fileSingle);
                ms.Position = 0;
                return ms.ToArray();
            }
            
        }
       
    }
}