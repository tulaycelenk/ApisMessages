using ApisMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apisMessages
{
    public class Demo : ApisEdifactHelper
    {
        List<string> apisMessageList = new List<string>();
        List<string> apisMessageHeaderList = new List<string>();
        List<string> apisMessagePassengerList = new List<string>();
        List<string> apisMessageFooterList = new List<string>();
        List<string> apisMessageDividedPassengerList = new List<string>();
        //ÖNEMLİ!!! receiver ülkelerde tanımlancak
        string receiver;
        public byte[] Export(FlightResponse flight, string exporterType, bool singleOrMultiPaxLst)
        {

            IEnumerable<PassengerDocs> passengerDocsList = PassengerDocsService.GetPassengerWithDocsInformation(new PassengerDocsPagingRequest() { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 }).Item1.Where(p => (p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Boarded) && p.PassengerId != 0).OrderBy(s => s.Surname);

            IEnumerable<Passenger> passengerList = PassengerService.QueryPassengersJoined(new PassengerPagingRequest { FlightId = flight.Id }).Where(p => p.PaxStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PaxStatusCode == ParameterHelper.PaxStatusCode.Boarded || p.PaxStatusCode == ParameterHelper.PaxStatusCode.CheckedIn).OrderBy(s => s.Surname);

            IEnumerable<Baggage> passengerBaggageList = BaggageService.QueryBaggagesJoined(new BaggagePagingRequest { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 });

            var enUs = new CultureInfo("en-US");
            var airlineName = flight.AirlineName.Trim().Replace(" ", string.Empty);
            airlineName = !string.IsNullOrEmpty(airlineName) ? (airlineName.Length > 35 ? airlineName = airlineName.Substring(0, 35) : airlineName) : "";

            var sobt = flight.Sobt.HasValue ? flight.Sobt.Value.ToString("yyMMddHHmm", enUs) : "";
            var sldt = flight.Sldt.HasValue ? flight.Sldt.Value.ToString("yyMMddHHmm", enUs) : "";
            var partIdentifier = 1;

            var yearToMin = DateTime.Now.ToString("yyMMdd", enUs) + ":" + DateTime.Now.ToString("HHmm", enUs);
            var yearToDay = DateTime.Now.ToString("yyMMdd", enUs);
            RestHelper rst = new RestHelper(new UrlProviderWithToken(), new HeaderProvider());
            var adepTimezone = rst.GetTimezoneOffsetByAirport(flight.AdepId, DateTime.UtcNow);
            var adepOfset = adepTimezone != null ? TimeSpan.FromSeconds(adepTimezone.GmtOffset).TotalSeconds : 0;
            int fromUNHtoUNTTotalRow = 0;

            if (exportRequest.PartType == "S") {
                #region Single Part PAXLST
                apisMessageHeaderList.Add(una);
                apisMessageHeaderList.Add(unb + sender + receiver + yearToMin + plus + BZcode + apos);
                apisMessageHeaderList.Add(ung + sender + receiver + plus + GEcode + typeVersionNum + version05B);
                apisMessageHeaderList.Add(unh + HTcode + unhMid1 + version05B + unhMid2);
                apisMessageHeaderList.Add(bgm745);
                apisMessageHeaderList.Add(nadMs);
                apisMessageHeaderList.Add(com);
                //BURAYA FLIGHT KONTROLLERİ İÇİN İF GELSİN Mİ?
                apisMessageHeaderList.Add(tdt20 + flight.AirlineIataCode + flight.FlightNumber.ToString() + threePlus + flight.AirlineIataCode + apos);
                apisMessageHeaderList.Add(loc125 + flight.DepartureIata + apos);
                //YUKARIDA SOBT TANIMLADIK
                apisMessageHeaderList.Add(dtm189 + ((DateTime)flight.Sobt).AddSeconds(adepOfset).ToString("yyMMddHHmm", enUs) + dtmEnd);
                apisMessageHeaderList.Add(loc87 + flight.DestinationIata + apos);
                apisMessageHeaderList.Add(dtm232 + yearToDay);

                foreach (var passengerDocs in passengerDocsList)
                {
                    var passengerDoco = PassengerDocoService.GetPassengerDoco(passengerDocs.PassengerId).FirstOrDefault();
                    var passengerDoca = PassengerDocaService.GetPassengerDocaWithPaging(new PassengerDocaPagingRequest { PassengerId = passengerDocs.PassengerId, PageNumber = 1, PageSize = 2000 }).Item1.FirstOrDefault();
                    var passenger = passengerList.Where(filter => filter.Id == passengerDocs.PassengerId).FirstOrDefault();
                    var passengerBaggages = passengerBaggageList.Where(filter => filter.PassengerId == passengerDocs.PassengerId).OrderBy(s => s.BagTag);
                    //Name NAD
                    //ELLE GİRİLEN BİLGİLERLE DÖKÜMANDAKİ BİLGİLER KIYASLANIYOR MU. YANLIŞ GİRİLDİYSE ÖRNEĞİN PASAPORTTA YAZAN GİBİ DEĞİLSE? PASAPORT OKUYUCULARINDAN GELEN İSMİ ALIYOR MUYUZ? KARŞILAŞTIRIYOR MUYUZ?
                    if (!string.IsNullOrEmpty(passengerDocs.Surname))
                        apisMessagePassengerList.Add(nadFl + (passengerDocs.Surname.Length > 35 ? passengerDocs.Surname.Substring(0, 35) : passengerDocs.Surname) : "") + (!string.IsNullOrEmpty(passengerDocs.Name) ? (":" + (passengerDocs.Name.Length > 35 ? passengerDocs.Name.Substring(0, 35) : passengerDocs.Name).Replace(" ", ":")) : "") + (passengerDoca != null ? (plus + (!string.IsNullOrEmpty(passengerDoca.ResidenceAddress) ? passengerDoca.ResidenceAddress.Length > 35 ? passengerDoca.ResidenceAddress.Substring(0, 35) : passengerDoca.ResidenceAddress : "") + plus + passengerDoca.ResidenceCity + plus + passengerDoca.ResidenceZipCode + plus + passengerDoca.ResidenceCountryIso3Code) : "") + apos);

                    //Gender ATT
                    //passengerDocs.GenderCod UI DAN GELEN VERİNİN KARŞILIĞI MI?
                    apisMessagePassengerList.Add(passengerDocs.GenderCode == PaxGenderCode.FemaleChild || passengerDocs.GenderCode == PaxGenderCode.Female ? attF : (passengerDocs.GenderCode == PaxGenderCode.MaleChild || passengerDocs.GenderCode == PaxGenderCode.Male ? attM : (passengerDocs.GenderCode == PaxGenderCode.Child || passengerDocs.GenderCode == PaxGenderCode.Infant ? attU )));
                    //Date of Birth DTM
                    apisMessagePassengerList.Add(dtm329 + passengerDocs.Dob.Value.ToString("yyMMdd") + apos);

                    //Baggage MEA
                    apisMessagePassengerList.Add(meaCt + passenger.BagCount + apos);

                    //the passenger verified or not
                    apisMessagePassengerList.Add(gei173);//apisMessagePassengerList(gei174);

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
                                    apisMessagePassengerList.Add(sequentialBagTagText);
                                else
                                    apisMessagePassengerList.Add(ftx + flight.AirlineIataCode + passengerBaggages.ElementAt(i).BagTag + "'");

                                isSequentialBagTag = false; sequentialBagTagText = null; sequentialBagTagNumber = 1;
                            }
                        }
                        if (isSequentialBagTag)
                            apisMessagePassengerList.Add(sequentialBagTagText);
                    }
                    //destination
                    if (!string.IsNullOrEmpty(flight.DepartureIata))
                        apisMessagePassengerList.Add(loc178 + flight.DepartureIata + apos);

                    //intransit passenger
                    if (!string.IsNullOrEmpty(flight.DestinationIata))
                        apisMessagePassengerList.Add(loc22 + flight.DestinationIata + apos);

                    //arrival
                    if (!string.IsNullOrEmpty(flight.DestinationIata))
                        apisMessagePassengerList.Add(loc179 + flight.DestinationIata + apos);

                    //residense
                    if (!string.IsNullOrEmpty(passengerDoca.ResidenceCountryIso3Code))
                        apisMessagePassengerList.Add(loc174 + passengerDoca.ResidenceCountryIso3Code + apos);

                    //place of birth
                    apisMessagePassengerList.Add(loc180 + threeColon + PLACE OF BIRTH + apos);

                    //communication number of the passenger
                    apisMessagePassengerList.Add(com + NUMBER:TE + apos);

                    //nationality
                    if (!string.IsNullOrEmpty(passengerDocs.NationalityCode))
                        apisMessagePassengerList.Add(nat2 + passengerDocs.NationalityCode + apos);

                    //passenger reservation number
                    apisMessagePassengerList.Add(rffAvf + RESERVATION NUMBER + apos);

                    //unique passenger reference number(rffAbo)
                    apisMessagePassengerList.Add(rffAbo + PASSENGER REFERENCE NUM + apos);

                    //assigned seat(rffSea)
                    if (!string.IsNullOrEmpty(passengerDocs.PassengerSeatNumber))
                        apisMessagePassengerList.Add(rffSea + passengerDocs.PassengerSeatNumber.PadLeft(3, '0') + apos);

                    //governement agency reference number (rffAea)
                    apisMessagePassengerList.Add(rffAea + GOVERNEMENT AGENCY REF NUMBER apos);

                    //doc type (docp or docv)
                    if (!string.IsNullOrEmpty(passengerDocs.DocNumberr) || !string.IsNullOrEmpty(passengerDocs.DocTypeCode))
                    {
                        switch (passengerDocs.DocTypeCode)
                        {
                            case "P":
                                apisMessagePassengerList.Add(docP + passengerDocs.DocNumber + apos);
                                break;
                            case "V":
                                apisMessagePassengerList.Add(docV + passengerDocs.DocNumber + apos);
                                break;
                            case "A":
                                apisMessagePassengerList.Add(docA + passengerDocs.DocNumber + apos);
                                break;
                            case "C":
                                apisMessagePassengerList.Add(docI + passengerDocs.DocNumber + apos);
                                break;
                            case "I":
                                apisMessagePassengerList.Add(docAC + passengerDocs.DocNumber + apos);
                                break;
                            case "AC":
                                apisMessagePassengerList.Add(docIP + passengerDocs.DocNumber + apos);
                                break;
                            case "IP":
                                apisMessagePassengerList.Add(docC + passengerDocs.DocNumber + apos);
                                break;
                            case "M":
                                apisMessagePassengerList.Add(docF + passengerDocs.DocNumber + apos);
                                break;
                            default:
                                apisMessagePassengerList.Add(docF + passengerDocs.DocNumber + apos);
                                break;
                        }
                    }

                    // expiry date of the official travel doc dtm36
                    if (passengerDoco.Doe.HasValue)
                        apisMessagePassengerList.Add(dtm36 + passengerDoco.Doe.Value.ToString("yyMMdd") + apos);

                    //issue date of the other doc used for travel dtm182
                    //ONAYLANMA TARİHİ TABLOLARDA YOK
                    apisMessagePassengerList.Add(dtm182 + OTHER DOCUMENTs ISSUE DATE + apos);

                    //issuing country code loc91
                    if (!string.IsNullOrEmpty(passengerDoco.DocForNationalityCode))
                        apisMessagePassengerList.Add(loc91 + passengerDoco.DocForNationalityCode + apos);

                    //issued city loc91 with three colon
                    //DÖKUMANIN ONAYLANDIĞI ŞEHİR TABLOLARDA YOK
                    apisMessagePassengerList.Add(loc91 + threeColon + doc FOR CITY CODE + apos);

                }
                apisMessageFooterList.Add(cnt42 + passengerDocsList.Count() + apos);
                apisMessageFooterList.Add(unt + $"{fromUNHtoUNTTotalRow}" + plus + HTcode + apos);
                apisMessageFooterList.Add(une + GEcode + apos);
                apisMessageFooterList.Add(unz + BZcode + apos);
                //string deneme = $"vkjfdnv{airlineName }";


                if (exportRequest.PartType == "S")
                {
                    //apisMessageList listesine Header-PassengerList-Foother bilgileri eklenip yazdırılıyor.
                    apisMessageList.AddRange(apisMessageHeaderList);
                    apisMessageList.AddRange(apisMessagePassengerList);
                    //Multi part UNH segmentinde UNt segmentine kadar olan segment sayısı
                    fromUNHtoUNTTotalRow = apisMessageList.Count() - 2; // -2: -(UNA UNB UNG) + UNT
                    apisMessageList.AddRange(apisMessageFooterList);
                }
                else if (exportRequest.PartType == "M")
                {
                    int apisMessageHeaderListCharecterCount = 0;
                    int apisMessagePassengerListCharecterCount = 0;
                    int apisMessageFootherListCharecterCount = 0;

                    foreach (var apisMessageHeader in apisMessageHeaderList)
                    {
                        apisMessageHeaderListCharecterCount += apisMessageHeader.Length;
                    }
                    foreach (var apisMessageFooter in apisMessageFooterList)
                    {
                        apisMessageFootherListCharecterCount += apisMessageFooter.Length;
                    }

                    int maxCharForPassengers = 1900 - apisMessageHeaderListCharecterCount - apisMessageFootherListCharecterCount;

                    //Multi-part seçeneği için partlar en fazla 1900 karakter olacak.
                    for (var i = 0; i < apisMessagePassengerList.Count(); i++)
                    {
                        //2 NAD ARASINDAKİ SATIR SAYISINI BULUP ONUN KATINI ALALIM
                        //SONUNCU PARTTA UNH SONUNA :F GELECEK

                        apisMessagePassengerListCharecterCount += apisMessagePassengerList[i].Length + 1; // +1:Line 
                        if (apisMessagePassengerListCharecterCount <= maxCharForPassengers)
                        {
                            apisMessageDividedPassengerList.Add(apisMessagePassengerList[i]);
                        }
                        else
                        {
                            for (var j = apisMessageDividedPassengerList.Count(); j >= 0; j--)
                            {
                                if (apisMessageDividedPassengerList[j - 1].Contains(nadFl))
                                {
                                    i = j - 1;
                                    j = 0;
                                }
                            }
                            if (apisMessageDividedPassengerList.Contains(apisMessagePassengerList.First()))
                            {
                                apisMessageHeaderList[apisMessageHeaderList.IndexOf(unh)] = unh + HTcode + unhMid1 + version05B + unhMid2 + partIdentifier + unique + plus + partIdentifier.ToString().PadLeft(2, '0') + ":C'";
                            }
                            else if (apisMessageDividedPassengerList.Contains(apisMessagePassengerList.Last()))
                            {
                                apisMessageHeaderList[apisMessageHeaderList.IndexOf(unh)] = unh + HTcode + unhMid1 + version05B + unhMid2 + partIdentifier + unique + plus + partIdentifier.ToString().PadLeft(2, '0') + ":F'";
                            }

                            apisMessageList.AddRange(apisMessageHeaderList);
                            for (var a = 0; a < i; a++)
                            {
                                apisMessageList.Add(apisMessageDividedPassengerList[a]);
                                apisMessagePassengerList.Remove(apisMessageDividedPassengerList[a]);
                            }


                        }

                    }

                }
            }
        }
    }


}