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
        
        #region variables

        Random r = new Random();
        int BZcode = r.Next(100000000, 999999999);
        int GEcode = r.Next(100000000, 999999999);
        int HTcode = r.Next(100000000, 999999999);
        int unique = r.Next(100000000, 999999999);

            public string una = "UNA:+.?";
            public string unaEnd = "*'";
            public string unb = "UNB+UNOA:4+";
            public string unbEnd = "++APIS";
            public string ung = "UNG+PAXLST+";
            public string ungEnd = "+UN+D:";
            public string unh = "UNH+";
            public string unhMid1 = " +PAXLST:D:";
            public string unhMid2 = ":UN:IATA+";
            public string unhEnd = "+01:F'";
            public string bgm = "BGM+745'";
            public string rfftn = "RFF+TN:";
            public string nadms = "NAD+MS+++";
            public string com = "COM+SUPPORT AT BRIDGEIST.COM:EM'";
            public string tdt20 = "TDT+20+";
            public string loc125 = "LOC+125+";
            public string loc87 = "LOC+87+";
            public string dtm189 = "DTM+189+";
            public string dtm232 = "DTM+232+";
            public string nadfl = "NAD+FL+++";
            public string att = "ATT+2++";
            public string dtm329 = "DTM+329:";
            public string loc22 = "LOC+22+";
            public string loc178 = "LOC+178+";
            public string loc179 = "LOC+179+";
            public string nat2 = "NAT+2+";
            public string rffavf = "RFF+AVF:";
            public string rffabo = "RFF+ABO:";
            public string rffsea = "RFF+SEA:";
            public string rffaea = "RFF+AEA:";
            public string doc = "DOC+";
            public string dtm36 = "DTM+36+";
            public string loc91 = "LOC+91+";
            public string cnt42 = "CNT+42:";
            public string unt = "UNT+";
            public string une = "UNE+1+";
            public string unz = "UNZ+1+";
            public string apos = "'";
            #endregion 

            public virtual string Una() { tw.Write(una); }
            public virtual void Unb() { tw.Write(unb); }
            public virtual void Ung() { tw.Write(ung); }
            public virtual void Unh() { tw.Write(unh); }
            public virtual void Bgm() { tw.Write(bgm); }
            public virtual void Nadms() { tw.Write(nadms); }
            public virtual void Com() { tw.Write(com); }
            public virtual void Tdt20() { tw.Write(tdt20); }
            public virtual void Loc125() { tw.Write(loc125); }
            public virtual void Loc87() { tw.Write(loc87); }
            public virtual void Dtm189() { tw.Write(dtm189); }
            public virtual void Dtm232() { tw.Write(dtm232); }
            public virtual void Nadfl() { tw.Write(nadfl); }
            public virtual void Att() { tw.Write(att); }
            public virtual void Dtm329() { tw.Write(dtm329); }
            public virtual void Loc22() { tw.Write(loc22); }
            public virtual void Loc178() { tw.Write(loc178); }
            public virtual void Loc179() { tw.Write(loc179); }
            public virtual void Nat2() { tw.Write(nat2); }
            public virtual void Rffavf() { tw.Write(una); }
            public virtual void Rffabo() { tw.Write(una); }
            public virtual void Rffsea() { tw.Write(una); }
            public virtual void Rffaea() { tw.Write(una); }
            public virtual void Doc() { tw.Write(una); }
            public virtual void Dtm36() { tw.Write(una); }
            public virtual void Loc91() { tw.Write(una); }
            public virtual void Cnt42() { tw.Write(una); }
            public virtual void Unt() { tw.Write(una); }
            public virtual void Une() { tw.WriteLine(une+ GEcode.ToString()+apos); }
            public virtual void Unz() { tw.WriteLine(unz+BZcode.ToString()+apos); }
        }
    }
}            
    
        