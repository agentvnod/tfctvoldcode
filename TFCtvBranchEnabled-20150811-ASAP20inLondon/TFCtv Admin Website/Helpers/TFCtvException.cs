using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFCtv_Admin_Website.Helpers
{
    public class TFCtvException : Exception
    {
        public ErrorCode StatusCode { get; set; }

        public string StatusMessage { get; set; }
    }

    public class TFCtvSuccess : TFCtvException
    {
        public TFCtvSuccess(string value) { StatusCode = ErrorCode.Success; StatusMessage = value; }
    }

    public class TFCtvMissingRequiredFields : TFCtvException
    {
        public TFCtvMissingRequiredFields() { StatusCode = ErrorCode.MissingRequiredFields; StatusMessage = "Missing required fields."; }
    }

    public class TFCtvUserDoesNotExist : TFCtvException
    {
        public TFCtvUserDoesNotExist() { StatusCode = ErrorCode.MissingRequiredFields; StatusMessage = "User does not exist."; }
    }

    public class TFCtvProductDoesNotExist : TFCtvException
    {
        public TFCtvProductDoesNotExist() { StatusCode = ErrorCode.ProductIdDoesNotExist; StatusMessage = "Product does not exist."; }
    }

    public class TFCtvUnidentifiedError : TFCtvException
    {
        public TFCtvUnidentifiedError(string value) { StatusCode = ErrorCode.UnidentifiedError; StatusMessage = value; }
    }

    public class TFCtvEntityFrameworkError : TFCtvException
    {
        public TFCtvEntityFrameworkError(string value) { StatusCode = ErrorCode.EntityFrameworkError; StatusMessage = value; }
    }

    public class TFCtvObjectIsNull : TFCtvException
    {
        public TFCtvObjectIsNull(string value) { StatusCode = ErrorCode.ObjectIsNull; StatusMessage = String.Format("{0} is either null or does not exist.", value); }
    }

    public class TFCtvUnauthorizedAccess : TFCtvException
    {
        public TFCtvUnauthorizedAccess() { StatusCode = ErrorCode.UnauthorizedAccess; StatusMessage = "Unauthorized access."; }
    }
}