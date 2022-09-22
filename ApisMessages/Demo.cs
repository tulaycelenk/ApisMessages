using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApisMessages
{
    class Demo:ApisEdifactHelper
    {
        List<string> ApisMessageList = new List<string>();
        List<string> ApisMessageHeaderList = new List<string>();
        List<string> ApisMessagePassengerList = new List<string>();
        List<string> ApisMessageFooterList = new List<string>();

        public byte[] Export(FlightResponse flight, string exporterType, bool singleOrMultiPaxLst)
        {         

            IEnumerable<PassengerDocs> passengerDocsList = PassengerDocsService.GetPassengerWithDocsInformation(new PassengerDocsPagingRequest() { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 }).Item1.Where(p => (p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Boarded) && p.PassengerId != 0).OrderBy(s => s.Surname);

            IEnumerable<Passenger> passengerList = PassengerService.QueryPassengersJoined(new PassengerPagingRequest { FlightId = flight.Id }).Where(p => p.PaxStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PaxStatusCode == ParameterHelper.PaxStatusCode.Boarded || p.PaxStatusCode == ParameterHelper.PaxStatusCode.CheckedIn).OrderBy(s => s.Surname);

            IEnumerable<Baggage> passengerBaggageList = BaggageService.QueryBaggagesJoined(new BaggagePagingRequest { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 });

            using (var ms = new MemoryStream())
            {
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
                int fromUNHtoUNTTotalRow = 0;

                if (exportRequest.PartType == "S"){
                #region Single Part PAXLST
                ApisMessageHeaderList.Add(una);
                ApisMessageHeaderList.Add(unb + sender + airlineName + yearToMin + plus + BZcode + apos);
                ApisMessageHeaderList.Add(ung + airlineName + yearToMin + plus + GEcode + typeVersionNum + version05B);
                ApisMessageHeaderList.Add(unh + HTcode + unhMid1 + version05B + unhMid2);
                ApisMessageHeaderList.Add(bgm745);
                ApisMessageHeaderList.Add(nadMs);
                ApisMessageHeaderList.Add(com);
                //BURAYA FLIGHT KONTROLLERİ İÇİN İF GELSİN Mİ?
                ApisMessageHeaderList.Add(tdt20 + flight.AirlineIataCode + flight.FlightNumber.ToString() + threePlus + flight.AirlineIataCode + apos);
                ApisMessageHeaderList.Add(loc125 + flight.DepartureIata + apos);
                //YUKARIDA SOBT TANIMLADIK
                ApisMessageHeaderList.Add(dtm189 + ((DateTime)flight.Sobt).AddSeconds(adepOfset).ToString("yyMMddHHmm", enUs) + dtmEnd);
                ApisMessageHeaderList.Add(loc87 + flight.DestinationIata + apos);
                ApisMessageHeaderList.Add(dtm232 + yearToDay);

                foreach (var passengerDocs in passengerDocsList)
                {
                    var passengerDoco = PassengerDocoService.GetPassengerDoco(passengerDocs.PassengerId).FirstOrDefault();
                    var passengerDoca = PassengerDocaService.GetPassengerDocaWithPaging(new PassengerDocaPagingRequest { PassengerId = passengerDocs.PassengerId, PageNumber = 1, PageSize = 2000 }).Item1.FirstOrDefault();
                    var passenger = passengerList.Where(filter => filter.Id == passengerDocs.PassengerId).FirstOrDefault();
                    var passengerBaggages = passengerBaggageList.Where(filter => filter.PassengerId == passengerDocs.PassengerId).OrderBy(s => s.BagTag);
                    //Name NAD
                    //ELLE GİRİLEN BİLGİLERLE DÖKÜMANDAKİ BİLGİLER KIYASLANIYOR MU. YANLIŞ GİRİLDİYSE ÖRNEĞİN PASAPORTTA YAZAN GİBİ DEĞİLSE? PASAPORT OKUYUCULARINDAN GELEN İSMİ ALIYOR MUYUZ? KARŞILAŞTIRIYOR MUYUZ?
                    if (!string.IsNullOrEmpty(passengerDocs.Surname))
                        ApisMessagePassengerList.Add(nadFl + (passengerDocs.Surname.Length > 35 ? passengerDocs.Surname.Substring(0, 35) : passengerDocs.Surname) : "") + (!string.IsNullOrEmpty(passengerDocs.Name) ? (":" + (passengerDocs.Name.Length > 35 ? passengerDocs.Name.Substring(0, 35) : passengerDocs.Name).Replace(" ", ":")) : "") + (passengerDoca != null ? (plus + (!string.IsNullOrEmpty(passengerDoca.ResidenceAddress) ? passengerDoca.ResidenceAddress.Length > 35 ? passengerDoca.ResidenceAddress.Substring(0, 35) : passengerDoca.ResidenceAddress : "") + plus + passengerDoca.ResidenceCity + plus + passengerDoca.ResidenceZipCode + plus + passengerDoca.ResidenceCountryIso3Code) : "") + apos);

                    //Gender ATT
                    //passengerDocs.GenderCod UI DAN GELEN VERİNİN KARŞILIĞI MI?
                    ApisMessagePassengerList.Add(passengerDocs.GenderCode == PaxGenderCode.FemaleChild || passengerDocs.GenderCode == PaxGenderCode.Female ? attF : (passengerDocs.GenderCode == PaxGenderCode.MaleChild || passengerDocs.GenderCode == PaxGenderCode.Male ? attM : (passengerDocs.GenderCode == PaxGenderCode.Child || passengerDocs.GenderCode == PaxGenderCode.Infant ? attU )));
                    //Date of Birth DTM
                    ApisMessagePassengerList.Add(dtm329 + passengerDocs.Dob.Value.ToString("yyMMdd") + apos);

                    //Baggage MEA
                    ApisMessagePassengerList.Add(meaCt + passenger.BagCount + apos);

                    //the passenger verified or not
                    ApisMessagePassengerList.Add(gei173);//ApisMessagePassengerList(gei174);

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
                                sequentialBagTagText = (ftx + flight.AirlineIataCode + passengerBaggages.ElementAt(i - sequentialBagTagNumber + 2).BagTag + ":" + sequentialBagTagNumber + "'");
                            }
                            else
                            {
                                if (isSequentialBagTag)
                                    ApisMessagePassengerList.Add(sequentialBagTagText);
                                else
                                    ApisMessagePassengerList.Add(ftx + flight.AirlineIataCode + passengerBaggages.ElementAt(i).BagTag + "'");

                                isSequentialBagTag = false; sequentialBagTagText = null; sequentialBagTagNumber = 1;
                            }
                        }
                        if (isSequentialBagTag)
                            ApisMessagePassengerList.Add(sequentialBagTagText);
                    }
                    //destination
                    if (!string.IsNullOrEmpty(flight.DepartureIata))
                        ApisMessagePassengerList.Add(loc178 + flight.DepartureIata + apos);

                    //intransit passenger
                    if (!string.IsNullOrEmpty(flight.DestinationIata))
                        ApisMessagePassengerList.Add(loc22 + flight.DestinationIata + apos);

                    //arrival
                    if (!string.IsNullOrEmpty(flight.DestinationIata))
                        ApisMessagePassengerList.Add(loc179 + flight.DestinationIata + apos);

                    //residense
                    if (!string.IsNullOrEmpty(passengerDoca.ResidenceCountryIso3Code))
                        ApisMessagePassengerList.Add(loc174 + passengerDoca.ResidenceCountryIso3Code + apos);

                    //place of birth
                    ApisMessagePassengerList.Add(loc180 + threeColon + PLACE OF BIRTH + apos);

                    //communication number of the passenger
                    ApisMessagePassengerList.Add(com + NUMBER:TE + apos);

                    //nationality
                    if (!string.IsNullOrEmpty(passengerDocs.NationalityCode))
                        ApisMessagePassengerList.Add(nat2 + passengerDocs.NationalityCode + apos);

                    //passenger reservation number
                    ApisMessagePassengerList.Add(rffAvf + RESERVATION NUMBER + apos);

                    //unique passenger reference number(rffAbo)
                    ApisMessagePassengerList.Add(rffAbo + PASSENGER REFERENCE NUM + apos);

                    //assigned seat(rffSea)
                    if (!string.IsNullOrEmpty(passengerDocs.PassengerSeatNumber))
                        ApisMessagePassengerList.Add(rffSea + passengerDocs.PassengerSeatNumber.PadLeft(3, '0') + apos);

                    //governement agency reference number (rffAea)
                    ApisMessagePassengerList.Add(rffAea + GOVERNEMENT AGENCY REF NUMBER apos);

                    //doc type (docp or docv)
                    if (!string.IsNullOrEmpty(passengerDocs.DocNumberr) || !string.IsNullOrEmpty(passengerDocs.DocTypeCode))
                    {
                        switch (passengerDocs.DocTypeCode)
                        {
                            case "P":
                                ApisMessagePassengerList.Add(docP + passengerDocs.DocNumber + apos);
                                break;
                            case "V":
                                ApisMessagePassengerList.Add(docV + passengerDocs.DocNumber + apos);
                                break;
                            case "A":
                                ApisMessagePassengerList.Add(docA + passengerDocs.DocNumber + apos);
                                break;
                            case "C":
                                ApisMessagePassengerList.Add(docI + passengerDocs.DocNumber + apos);
                                break;
                            case "I":
                                ApisMessagePassengerList.Add(docAC + passengerDocs.DocNumber + apos);
                                break;
                            case "AC":
                                ApisMessagePassengerList.Add(docIP + passengerDocs.DocNumber + apos);
                                break;
                            case "IP":
                                ApisMessagePassengerList.Add(docC + passengerDocs.DocNumber + apos);
                                break;
                            case "M":
                                ApisMessagePassengerList.Add(docF + passengerDocs.DocNumber + apos);
                                break;
                            default:
                                ApisMessagePassengerList.Add(docF + passengerDocs.DocNumber + apos);
                                break;
                        }
                    }

                    // expiry date of the official travel doc dtm36
                    if (passengerDoco.Doe.HasValue)
                        ApisMessagePassengerList.Add(dtm36 + passengerDoco.Doe.Value.ToString("yyMMdd") + apos);

                    //issue date of the other doc used for travel dtm182
                    //ONAYLANMA TARİHİ TABLOLARDA YOK
                    ApisMessagePassengerList.Add(dtm182 + OTHER DOCUMENTs ISSUE DATE + apos);

                    //issuing country code loc91
                    if (!string.IsNullOrEmpty(passengerDoco.DocForNationalityCode))
                        ApisMessagePassengerList.Add(loc91 + passengerDoco.DocForNationalityCode + apos);

                    //issued city loc91 with three colon
                    //DÖKUMANIN ONAYLANDIĞI ŞEHİR TABLOLARDA YOK
                    ApisMessagePassengerList.Add(loc91 + threeColon + doc FOR CITY CODE + apos);

                }
                ApisMessageFooterList.Add(cnt42 + passengerDocsList.Count() + apos);
                ApisMessageFooterList.Add(unt + $"{fromUNHtoUNTTotalRow}" + plus + HTcode + apos);
                ApisMessageFooterList.Add(une + GEcode + apos);
                ApisMessageFooterList.Add(unz + BZcode + apos);
                    //string deneme = $"vkjfdnv{airlineName }";

                    //ApisMessageList listesine Header-PassengerList-Foother bilgileri eklenip yazdırılıyor.
                    ApisMessageList.AddRange(ApisMessageHeaderList);
                    ApisMessageList.AddRange(ApisMessagePassengerList);
                    //Multi part UNH segmentinde UNt segmentine kadar olan segment sayısı
                    fromUNHtoUNTTotalRow = ApisMessageList.Count() - 2; // -2: -(UNA UNB UNG) + UNT
                    ApisMessageList.AddRange(ApisMessageFooterList);






                    /*
                    var fileSingle = Encoding.UTF8.GetString(ms.ToArray());
                    Logger.Default.Append(LogLevel.Debug, fileSingle);
                    ms.Position = 0;
                    return ms.ToArray();
                    
                     */
                }

        }

    }
}
