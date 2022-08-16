using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApisMessages
{
    public abstract class ApisBase
    {
        /*
        private readonly CultureInfo enUs = new CultureInfo("en-Us");
        TextWriter tw = new StreamWriter(ms);
        var enUs = new CultureInfo("en-US");
        string nowDate = DateTime.Now.ToString("yyMMdd", enUs) + ":" + DateTime.Now.ToString("HHmm", enUs);

        string airlineName = flight.AirlineName.Trim().Replace(" ", string.Empty);
        airlineName = !string.IsNullOrEmpty(airlineName) ? (airlineName.Length > 35 ? airlineName = airlineName.Substring(0, 35) : airlineName) : "";

        string sobt = flight.Sobt.HasValue ? flight.Sobt.Value.ToString("yyMMddHHmm", enUs) : "";
        string sldt = flight.Sldt.HasValue ? flight.Sldt.Value.ToString("yyMMddHHmm", enUs) : "";

        string confirmation = DateTime.Now.ToString("mmssmmm", enUs);
        int BZcode = rnd.Next(100000000, 999999999);
        int GEcode = rnd.Next(100000000, 999999999);
        int HTcode = rnd.Next(100000000, 999999999);
        int unique = rnd.Next(100000000, 999999999);
        */
        public string una = "UNA:+.?";
        public string unb = "UNB+UNOA:4+";
        public string unbEnd = "++APIS";
        public string ung = "UNG+PAXLST+";
        public string ungEnd = "+UN+D:";
        public string unh = "UNH+";
        public string unhMid1 = " +PAXLST:D:";
        public string unhMid2 = ":UN:IATA+";
        public string unhEnd = "+01:F'";
        public string bgm = "BGM+745'";
        public string nadms = "NAD+MS+++";
        public string com = "COM+SUPPORT AT BRIDGEIST.COM:EM'";
        public string tdt20 = "TDT+20+";
        public string loc125 = "LOC+125+";
        public string loc87 = "LOC+87+";
        public string dtm189 = "DTM+189+";
        public string dtm232 = "DTM+232+";
        public string nad = "NAD+FL+++";
        public string att = "ATT+2++";
        public string dtm329 = "DTM+329:";
        public string loc178 = "LOC+178+";
        public string loc179 = "LOC+179+";
        public string rffavf = "RFF+AVF:";
        public string doc = "DOC+";
        public string dtm36 = "DTM+36+";
        public string loc91 = "LOC+91+";
        public string cnt42 = "CNT+42:";
        public string unt = "UNT+";
        public string une1 = "UNE+1+";
        public string unz1 = "UNZ+1+";


        public abstract void Una();
        public abstract void Unb();
        public abstract void Ung();
        public abstract void Unh();
        public abstract void Bgm();
        public abstract void Nadms();
        public abstract void Com();
        public abstract void Tdt20();
        public abstract void Loc125();
        public abstract void Loc87();
        public abstract void Dtm189();
        public abstract void Dtm232();
        public abstract void Nadfl();
        public abstract void Att();
        public abstract void Dtm329();
        public abstract void Loc22();
        public abstract void Loc178();
        public abstract void Loc179();
        public abstract void Nat2();
        public abstract void Rffavf();
        public abstract void Doc();
        public abstract void Dtm36();
        public abstract void Loc91();
        public abstract void Cnt42();
        public abstract void Unt();
        public abstract void Une1();
        public abstract void Unz1();

    }
}            
    
        