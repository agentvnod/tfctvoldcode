using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTV2_Model
{
    public partial class SubscriptionPpc
    {
        public bool IsTrial
        {
            get
            {
                return (PpcType is TrialSubscriptionPpcType);
            }
        }

        public bool IsComplimentary
        {
            get
            {
                return (PpcType is ComplimentarySubscriptionPpcType);
            }
        }

        public bool IsCompensatory
        {
            get
            {
                return (PpcType is CompensatorySubscriptionPpcType);
            }
        }

        public bool IsRegular
        {
            get
            {
                return (PpcType is RegularSubscriptionPpcType);
            }
        }



    }
}
