using AviatorManager.ViewModel.Aodb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApisMessages
{
    //PAXLST IMPLEMENTATION GUIDE ANNEX III 28 June 2010 dökümanına göre düzenlenmiştir.
    public class Version02B : ApisEdifactHelper
    {
        //versiyonlar helper ı içerecek tw a atılacak ve ülkeler bunu kullanacak 
        //ülkere göre değişiklikler ülkelerde yapılacak 

        List<string> apisMessageList = new List<string>();
        List<string> apisMessageHeaderList = new List<string>();
        List<string> apisMessagePassengerList = new List<string>();
        List<string> apisMessageFooterList = new List<string>();

        //TANIMI YAPILMALI
        private string receiver;

        public byte[] Export(FlightResponse flight, string exporterType, bool singleOrMultiPaxLst)
        {
            IEnumerable<PassengerDocs> passengerDocsList = PassengerDocsService.GetPassengerWithDocsInformation(new PassengerDocsPagingRequest() { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 }).Item1.Where(p => (p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PassengerStatusCode == ParameterHelper.PaxStatusCode.Boarded) && p.PassengerId != 0).OrderBy(s => s.Surname);

            IEnumerable<Passenger> passengerList = PassengerService.QueryPassengersJoined(new PassengerPagingRequest { FlightId = flight.Id }).Where(p => p.PaxStatusCode == ParameterHelper.PaxStatusCode.Flown || p.PaxStatusCode == ParameterHelper.PaxStatusCode.Boarded || p.PaxStatusCode == ParameterHelper.PaxStatusCode.CheckedIn).OrderBy(s => s.Surname);

            IEnumerable<Baggage> passengerBaggageList = BaggageService.QueryBaggagesJoined(new BaggagePagingRequest { FlightId = flight.Id, PageNumber = 1, PageSize = 2000 });

            var enUs = new CultureInfo("en-US");

            var sobt = flight.Sobt.HasValue ? flight.Sobt.Value.ToString("yyMMddHHmm", enUs) : "";
            var sldt = flight.Sldt.HasValue ? flight.Sldt.Value.ToString("yyMMddHHmm", enUs) : "";

            RestHelper rst = new RestHelper(new UrlProviderWithToken(), new HeaderProvider());
            var adepTimezone = rst.GetTimezoneOffsetByAirport(flight.AdepId, DateTime.UtcNow);
            var adepOfset = adepTimezone != null ? TimeSpan.FromSeconds(adepTimezone.GmtOffset).TotalSeconds : 0;
            int fromUNHtoUNTTotalRow = 0;

            if (!singleOrMultiPaxLst)
            {
                #region Single Part PAXLST
                apisMessageHeaderList.Add(una);
                apisMessageHeaderList.Add(unb + sender + receiver + yearToMin + plus + BZcode + apos);
                apisMessageHeaderList.Add(unh + HTcode + unhMid1 + version05B + unhMid2);
                apisMessageHeaderList.Add(bgm745);
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
                        apisMessagePassengerList.Add(nadFl + (passengerDocs.Surname.Length > 35 ? passengerDocs.Surname.Substring(0, 35) : passengerDocs.Surname) : "") + (!string.IsNullOrEmpty(passengerDocs.Name) ? (":" + (passengerDocs.Name.Length > 35 ? passengerDocs.Name.Substring(0, 35) : passengerDocs.Name).Replace(" ", ":")) : "") + (passengerDoca != null ? ("+" + (!string.IsNullOrEmpty(passengerDoca.ResidenceAddress) ? passengerDoca.ResidenceAddress.Length > 35 ? passengerDoca.ResidenceAddress.Substring(0, 35) : passengerDoca.ResidenceAddress : "") + "+" + passengerDoca.ResidenceCity + "+" + passengerDoca.ResidenceZipCode + "+" + passengerDoca.ResidenceCountryIso3Code) : "") + apos);


                    //Gender ATT
                    //passengerDocs.GenderCod UI DAN GELEN VERİNİN KARŞILIĞI MI?
                    apisMessagePassengerList.Add(passengerDocs.GenderCode == PaxGenderCode.FemaleChild || passengerDocs.GenderCode == PaxGenderCode.Female ? attF : (passengerDocs.GenderCode == PaxGenderCode.MaleChild || passengerDocs.GenderCode == PaxGenderCode.Male ? attM : (passengerDocs.GenderCode == PaxGenderCode.Child || passengerDocs.GenderCode == PaxGenderCode.Infant ? attU :"")));
                    //Date of Birth DTM
                    apisMessagePassengerList.Add(dtm329 + passengerDocs.Dob.Value.ToString("yyMMdd") + apos);

                    //intransit passenger
                    if (!string.IsNullOrEmpty(flight.DestinationIata))
                        apisMessagePassengerList.Add(loc22 + flight.DestinationIata + apos);

                    //destination
                    if (!string.IsNullOrEmpty(flight.DepartureIata))
                        apisMessagePassengerList.Add(loc178 + flight.DepartureIata + apos);

                    //arrival
                    if (!string.IsNullOrEmpty(flight.DestinationIata))
                        apisMessagePassengerList.Add(loc179 + flight.DestinationIata + apos);

                    //nationality
                    if (!string.IsNullOrEmpty(passengerDocs.NationalityCode))
                        apisMessagePassengerList.Add(nat2 + passengerDocs.NationalityCode + apos);

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

                    //issuing country code loc91
                    if (!string.IsNullOrEmpty(passengerDoco.DocForNationalityCode))
                        apisMessagePassengerList.Add(loc91 + passengerDoco.DocForNationalityCode + apos);

                }

                apisMessageFooterList.Add(cnt42 + passengerDocsList.Count() + apos);
                apisMessageFooterList.Add(unt + $"{fromUNHtoUNTTotalRow}" + plus + HTcode + apos);

                //apisMessageList listesine Header-PassengerList-Foother bilgileri eklenip yazdırılıyor.
                apisMessageList.AddRange(apisMessageHeaderList);
                apisMessageList.AddRange(apisMessagePassengerList);
                //Multi part UNH segmentinde UNt segmentine kadar olan segment sayısı
                fromUNHtoUNTTotalRow = apisMessageList.Count() - 2; // -2: -(UNA UNB UNG) + UNT
                apisMessageList.AddRange(apisMessageFooterList);

            }

        }

    }

}
}
