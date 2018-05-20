namespace ConfluenceJsonRequest
{
    internal class ResponseCode
    {
        public enum Response
        {
            Success = 0,
            UnknownDevice,
            UnkownComponent,
            UnknownParameter,
            UnknownFailure
        }
    }
}