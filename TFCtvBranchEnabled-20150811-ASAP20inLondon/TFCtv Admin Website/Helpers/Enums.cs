using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFCtv_Admin_Website.Helpers
{
    public enum StatusId
    {
        Visible = 1
    }

    public enum ErrorCode
    {
        UnidentifiedError = -1000,
        MissingRequiredFields = -1001,
        EntityFrameworkError = -1002,
        UserDoesNotExist = -1003,
        ProductIdDoesNotExist = -1004,
        ObjectIsNull = -1005,
        UnauthorizedAccess = -1006,
        Success = 0,

    }

    public enum StreamType
    {
        ADAPTIVE = 0,
        PLAY_HIGH = 1,
        PLAY_LOW = 2,
        PLAY_IN_HD = 3
    }
}