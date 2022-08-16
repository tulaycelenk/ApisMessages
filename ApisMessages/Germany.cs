using AviatorManager.ViewModel.Aodb;
using Bridge.App.Core.Exceptions;
using Bridge.Common.Logging;
using DCS.App.Service.Entity;
using DCS.App.Service.Resources;
using DCS.ViewModel.Paging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ApisMessages
{
    class GermanyEdifactPaxLstExporter : IExporter
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
                    passengerDocsList = passengerDocsListAll.Where(p => p.PassengerStatusCode ==Bridge.Common.ParameterHelper.PaxStatusCode.Flown || p.PassengerStatusCode ==Bridge.Common.ParameterHelper.PaxStatusCode.Boarded || p.PassengerStatusCode ==Bridge.Common.ParameterHelper.PaxStatusCode.CheckedIn);

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

                    if (!singleOrMultiPaxLst)
                    {
                        #region Single Part PAXLST
                        tw.WriteLine("UNA:+.? '");
                        tw.WriteLine("UNB+UNOA:4+" + airlineName + "+BPOLAPIS+" + nowDate + "+" + confirmation + "++APIS'");
                        tw.WriteLine("UNG+PAXLST+" + airlineName + "+ZNVPIXH+" + nowDate + "+" + flight.DestinationIata + confirmation + "+UN+D:02B'");
                        tw.WriteLine("UNH+1+PAXLST:D:02B:UN:IATA+" + confirmation + "+01:F'");
                        tw.WriteLine("BGM+745'");
                        tw.WriteLine("COM+SUPPORT AT BRIDGEIST.COM:EM'");
                        tw.WriteLine("TDT+20+" + flight.AirlineIataCode + flight.FlightNumber.ToString() + "+++" + flight.AirlineIataCode + "'");
                        if (!string.IsNullOrEmpty(flight.DepartureIata))
                            tw.WriteLine("LOC+125+" + flight.DepartureIata + "'");
                        if (flight.Sobt.HasValue)
                            tw.WriteLine("DTM+189:" + sobt + ":201'");
                        if (!string.IsNullOrEmpty(flight.DestinationIata))
                            tw.WriteLine("LOC+87+" + flight.DestinationIata + "'");
                        if (flight.Sldt.HasValue)
                            tw.WriteLine("DTM+232:" + sldt + ":201'");

                        for (int i = 0; i < passengerDocsList.Count(); i++)
                        {
                            string fullName = (!string.IsNullOrEmpty(passengerDocsList.ElementAt(i).Surname) ? (passengerDocsList.ElementAt(i).Surname.Length > 35 ? passengerDocsList.ElementAt(i).Surname.Substring(0, 35) : passengerDocsList.ElementAt(i).Surname) : "") + (!string.IsNullOrEmpty(passengerDocsList.ElementAt(i).Name) ? (":" + (passengerDocsList.ElementAt(i).Name.Length > 35 ? passengerDocsList.ElementAt(i).Name.Substring(0, 35) : passengerDocsList.ElementAt(i).Name)) : "") + "'";
                            string inboundFlightIata = passengerDocsList.ElementAt(i).InboundFlightIata != null ? passengerDocsList.ElementAt(i).InboundFlightIata : flight.DepartureIata;
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
                    else
                    {
                        #region Multi Part PAXLST
                        List<PassengerDocs> paxLstPassengerIdList = new List<PassengerDocs>();
                        IEnumerable<PassengerDocs> control = passengerDocsList;
                        List<TextWriter> texts = new List<TextWriter>();
                        var partCount = 0;
                        if (passengerDocsList.Count() < 10)
                        {
                            partCount = 0;
                        }
                        else
                        {
                            partCount = passengerDocsList.Count() / 9;
                        }
                        var totalCount = control.Count();
                        for (int k = 0; k <= partCount; k++)
                        {
                            var partNumber = "";
                            if (k >= 0 && k < 10)
                            {
                                partNumber = "0" + (k + 1).ToString();
                            }
                            else { partNumber = (k + 1).ToString(); }

                            tw.WriteLine("UNA:+.? '");
                            tw.WriteLine("UNB+UNOA:4+" + airlineName + "+BPOLAPIS+" + nowDate + "+" + confirmation + "++APIS'");
                            tw.WriteLine("UNG+PAXLST+" + airlineName + "+ZNVPIXH+" + nowDate + "+" + flight.DestinationIata + confirmation + "+UN+D:02B'");
                            if (control.Count() < 9)
                            {
                                partNumber = "01:F";
                            }
                            else
                            {
                                if (k == 0)
                                    partNumber += ":C";
                                if (k == partCount)
                                    partNumber += ":F";
                            }

                            tw.WriteLine("UNH+1+PAXLST:D:02B:UN:IATA+" + confirmation + "+" + partNumber + "'");
                            tw.WriteLine("BGM+745'");
                            tw.WriteLine("COM+SUPPORT AT BRIDGEIST.COM:EM'");
                            tw.WriteLine("TDT+20+" + flight.AirlineIataCode + flight.FlightNumber.ToString() + "+++" + flight.AirlineIataCode + "'");
                            if (!string.IsNullOrEmpty(flight.DepartureIata))
                                tw.WriteLine("LOC+125+" + flight.DepartureIata + "'");
                            if (flight.Sobt.HasValue)
                                tw.WriteLine("DTM+189:" + sobt + ":201'");
                            if (!string.IsNullOrEmpty(flight.DestinationIata))
                                tw.WriteLine("LOC+87+" + flight.DestinationIata + "'");
                            if (flight.Sldt.HasValue)
                                tw.WriteLine("DTM+232:" + sldt + ":201'");

                            for (int i = 0; i < passengerDocsList.Count(); i++)
                            {
                                if (i < 9)
                                {
                                    string fullName = (!string.IsNullOrEmpty(passengerDocsList.ElementAt(i).Surname) ? (passengerDocsList.ElementAt(i).Surname.Length > 35 ? passengerDocsList.ElementAt(i).Surname.Substring(0, 35) : passengerDocsList.ElementAt(i).Surname) : "") + (!string.IsNullOrEmpty(passengerDocsList.ElementAt(i).Name) ? (":" + (passengerDocsList.ElementAt(i).Name.Length > 35 ? passengerDocsList.ElementAt(i).Name.Substring(0, 35) : passengerDocsList.ElementAt(i).Name)) : "") + "'";
                                    string inboundFlightIata = passengerDocsList.ElementAt(i).InboundFlightIata != null ? passengerDocsList.ElementAt(i).InboundFlightIata : flight.DepartureIata;
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

                                    paxLstPassengerIdList.Add(passengerDocsList.ElementAt(i));
                                }
                            }
                            var except = passengerDocsList.Except(paxLstPassengerIdList).ToList();
                            int totalRow = ((passengerDocsList.Count() - except.Count) * 10) + 9;
                            passengerDocsList = except;
                            tw.WriteLine("CNT+42:" + totalCount + "'");
                            tw.WriteLine("UNT+" + totalRow + "+" + confirmation + "'");
                            tw.WriteLine("UNE+1+" + flight.DestinationIata + confirmation + "'");
                            tw.WriteLine("UNZ+1+" + flight.DepartureIata + confirmation + "'");
                            tw.WriteLine(); tw.WriteLine(); tw.WriteLine();
                            texts.Add(tw);
                        }
                        foreach (var item in texts)
                        {
                            item.Flush();
                        }
                        ms.Position = 0;
                        var fileMulti = Encoding.UTF8.GetString(ms.ToArray());
                        Logger.Default.Append(LogLevel.Debug, fileMulti);
                        ms.Position = 0;
                        return ms.ToArray();
                        #endregion
                    }
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
                string flightDate = string.Empty;
                if (flight.AdepFlightDate.HasValue)
                    flightDate = flight.AdepFlightDate.Value.ToString("ddMMMyyyy", enUs).ToUpper();
                else
                    flightDate = flight.AdesFlightDate.Value.ToString("ddMMMyyyy", enUs).ToUpper();
                return flight.AirlineIataCode + flight.FlightNumber + flightDate + flight.DepartureIata + flight.DestinationIata + "_GermanyApisFile(EDIFACT).txt";
            }
        }
    }
}
}
