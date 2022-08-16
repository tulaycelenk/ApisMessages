using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApisMessages
{
    public class Finland:IExporter
    {
            private readonly CultureInfo enUs = new CultureInfo("en-Us");
            /// <summary>
            /// Germany Edifact PaxLst
            /// </summary>
            /// <param name="flight"></param>
            /// <param name="singleOrMultiPaxLst"></param>
            /// <returns></returns>
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
                    var nowDate = DateTime.Now.ToString("yyMMdd", enUs) + ":" + DateTime.Now.ToString("HHmm", enUs);

                    var airlineName = flight.AirlineName.Trim().Replace(" ", string.Empty);
                    airlineName = !string.IsNullOrEmpty(airlineName) ? (airlineName.Length > 35 ? airlineName = airlineName.Substring(0, 35) : airlineName) : "";

                    var sobt = flight.Sobt.HasValue ? flight.Sobt.Value.ToString("yyMMddHHmm", enUs) : "";
                    var sldt = flight.Sldt.HasValue ? flight.Sldt.Value.ToString("yyMMddHHmm", enUs) : "";

                    var confirmation = DateTime.Now.ToString("mmssmmm", enUs);
                    
                    var BZcode= rnd.Next(100000000, 999999999);
                    var GEcode = rnd.Next(100000000, 999999999);
                    var HTcode = rnd.Next(100000000, 999999999);
                    var unique = rnd.Next(100000000, 999999999);
                if (!singleOrMultiPaxLst)
                        {
                            #region Single Part PAXLST
                            tw.WriteLine("UNA:+.? '");
                            tw.WriteLine("UNB+UNOA:4+" + airlineName + "+BPOLAPIS:ZZ" + nowDate + "+" + BZcode + "++APIS'");
                            tw.WriteLine("UNG+PAXLST+" + airlineName + "+FIAPIS:ZZ+" + nowDate + "+" + GEcode + "+UN+D:02B'");
                            tw.WriteLine("UNH+"+unique +"+PAXLST:D:02B:UN:IATA+" + HTcode + "+01:F'");
                            tw.WriteLine("BGM+745'");
                            tw.WriteLine("NAD+MS+++DEGIRMENCI GAMZE'");
                            tw.WriteLine("COM+SUPPORT AT BRIDGEIST.COM:EM'");
                            tw.WriteLine("TDT+20+" + flight.AirlineIataCode + flight.FlightNumber.ToString() + "+++" + flight.AirlineIataCode + "'");
                            if (!string.IsNullOrEmpty(flight.DepartureIata))
                                tw.WriteLine("LOC+125+" + flight.DepartureIata + "'");
                            if (!string.IsNullOrEmpty(flight.DestinationIata))
                                tw.WriteLine("LOC+87+" + flight.DestinationIata + "'");

                            if (!string.IsNullOrEmpty(flight.Sobt.HasValue)
                                tw.WriteLine("DTM+189+" + flight.Sobt.HasValue + "'");
                            if(!string.IsNullOrEmpty(flight.Sldt.HasValue)
                                tw.WriteLine("DTM+232+" + flight.Sldt.HasValue + "'");

                    for (int i = 0; i<passengerDocsList.Count(); i++)
                            {
                                string fullName = (!string.IsNullOrEmpty(passengerDocsList.ElementAt(i).Surname) ? (passengerDocsList.ElementAt(i).Surname.Length > 35 ? passengerDocsList.ElementAt(i).Surname.Substring(0, 35) : passengerDocsList.ElementAt(i).Surname) : "") + (!string.IsNullOrEmpty(passengerDocsList.ElementAt(i).Name) ? (":" + (passengerDocsList.ElementAt(i).Name.Length > 35 ? passengerDocsList.ElementAt(i).Name.Substring(0, 35) : passengerDocsList.ElementAt(i).Name)) : "") + "'";
                                string inboundFlightIata = passengerDocsList.ElementAt(i).InboundFlightIata != null ? passengerDocsList.ElementAt(i).InboundFlightIata :        flight.DepartureIata;
                                string onwardFlightIata = passengerDocsList.ElementAt(i).OnwardFlightIata != null ? passengerDocsList.ElementAt(i).OnwardFlightIata : flight.DestinationIata;

                                if (!string.IsNullOrEmpty(passengerDocsList.ElementAt(i).Surname) || !string.IsNullOrEmpty(passengerDocsList.ElementAt(i).Name))
                                    tw.WriteLine("NAD+FL+++" + fullName);
                                if (!string.IsNullOrEmpty(passengerDocsList.ElementAt(i).GenderCode))
                                {
                                    if (passengerDocsList.ElementAt(i).GenderCode == "FC")
                                        passengerDocsList.ElementAt(i).GenderCode = "F";
                                    else if (passengerDocsList.ElementAt(i).GenderCode == "MC")
                                        passengerDocsList.ElementAt(i).GenderCode = "M";
                                    else if (passengerDocsList.ElementAt(i).GenderCode == "C")
                                        passengerDocsList.ElementAt(i).GenderCode = "U";
                                    tw.WriteLine("ATT+2++" + passengerDocsList.ElementAt(i).GenderCode + "'");
                                }
                                if (passengerDocsList.ElementAt(i).Dob.HasValue)
                                    tw.WriteLine("DTM+329:" + passengerDocsList.ElementAt(i).Dob.Value.ToString("yyMMdd") + "'");
                                if (!string.IsNullOrEmpty(flight.DestinationIata))
                                    tw.WriteLine("LOC+22+" + flight.DestinationIata + "'");
                                if (!string.IsNullOrEmpty(inboundFlightIata))
                                    tw.WriteLine("LOC+178+" + inboundFlightIata + "'");
                                if (!string.IsNullOrEmpty(onwardFlightIata))
                                    tw.WriteLine("LOC+179+" + onwardFlightIata + "'");
                                if (!string.IsNullOrEmpty(passengerDocsList.ElementAt(i).NationalityCode))
                                    tw.WriteLine("NAT+2+" + (passengerDocsList.ElementAt(i).NationalityCode == "DEU" ? "D'" : (passengerDocsList.ElementAt(i).NationalityCode + "'")));
                                if (!string.IsNullOrEmpty(passengerDocsList.ElementAt(i).PassengerPnr))
                                    tw.WriteLine("RFF+AVF:" + passengerDocsList.ElementAt(i).PassengerPnr + "'");
                                if (!string.IsNullOrEmpty(passengerDocsList.ElementAt(i).DocNumber) || !string.IsNullOrEmpty(passengerDocsList.ElementAt(i).DocTypeCode))
                                {
                                    tw.WriteLine("DOC+" + (passengerDocsList.ElementAt(i).DocTypeCode == "O" ? passengerDocsList.ElementAt(i).CustomDocTypeCode : passengerDocsList.ElementAt(i).DocTypeCode) + "+" + passengerDocsList.ElementAt(i).DocNumber + "'");
                                    if (passengerDocsList.ElementAt(i).Doe.HasValue)
                                        tw.WriteLine("DTM+36+" + passengerDocsList.ElementAt(i).Doe.Value.ToString("yyMMdd") + "'");
                                }
                                if (!string.IsNullOrEmpty(passengerDocsList.ElementAt(i).DocIssuerNationalityCode))
                                    tw.WriteLine("LOC+91+" + (passengerDocsList.ElementAt(i).DocIssuerNationalityCode == "DEU" ? "D'" : (passengerDocsList.ElementAt(i).DocIssuerNationalityCode + "'")));

                                var passengerDoco = PassengerDocoService.GetPassengerDoco(passengerDocsList.ElementAt(i).PassengerId);
                                if (passengerDoco.Count() > 0)
                                {
                                    var docoDoe = passengerDoco.ElementAt(0).Doe.HasValue ? passengerDoco.ElementAt(0).Doe.Value.ToString("yyMMdd") : "";
                                    var docoDocNumber = !string.IsNullOrEmpty(passengerDoco.ElementAt(0).DocNumber) ? passengerDoco.ElementAt(0).DocNumber : "";
                                    var docoDocForNatioanalityCode = !string.IsNullOrEmpty(passengerDoco.ElementAt(0).DocForNationalityCode) ? passengerDoco.ElementAt(0).DocForNationalityCode : "";
                                    var docoDocType = !string.IsNullOrEmpty(passengerDoco.ElementAt(0).DocTypeCode) ? (passengerDoco.ElementAt(0).DocTypeCode == "O" ? passengerDoco.ElementAt(0).CustomDocTypeCode : passengerDoco.ElementAt(0).DocTypeCode) : "";
                                    docoDocForNatioanalityCode = docoDocForNatioanalityCode == "DEU" ? "D" : docoDocForNatioanalityCode;
                                    if (!string.IsNullOrEmpty(docoDocNumber))
                                        tw.WriteLine("DOC+" + docoDocType + "+" + docoDocNumber + "'");
                                    if (!string.IsNullOrEmpty(docoDoe))
                                        tw.WriteLine("DTM+36+" + docoDoe + "'");
                                    if (!string.IsNullOrEmpty(docoDocForNatioanalityCode))
                                        tw.WriteLine("LOC+91+" + docoDocForNatioanalityCode + "'");
                                }
                                                        }
                                                        tw.WriteLine("CNT+42:" + passengerDocsList.Count() + "'");
                                int totalRow = (passengerDocsList.Count() * 10) + 9;
                                tw.WriteLine("UNT+" + totalRow + "+" + confirmation + "'");
                                tw.WriteLine("UNE+1+" + flight.DestinationIata + confirmation + "'");
                                tw.WriteLine("UNZ+1+" + flight.DepartureIata + confirmation + "'");

                                tw.Flush();
                                ms.Position = 0;
                                var fileSingle = Encoding.UTF8.GetString(ms.ToArray());
                                Logger.Default.Append(LogLevel.Debug, fileSingle);
                                ms.Position = 0;
                                return ms.ToArray();
                                                        #endregion
                                                    }
                                    }
                                }
