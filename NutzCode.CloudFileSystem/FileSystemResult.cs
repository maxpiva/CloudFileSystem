using System.Runtime.InteropServices.ComTypes;

namespace NutzCode.CloudFileSystem
{
    
    public class FileSystemResult<T> : FileSystemResult
    {
        public T Result { get; set; }
        public FileSystemResult(Status status, string error) : base(status, error)
        {
        }
        public FileSystemResult(T result)
        {
            Result = result;
            Status = Status.Ok;
        }

    }
    public interface IResult
    {
        Status Status { get; set; }
        string Error { get; set; }

    }

    public static class IResultExtensions
    {
        public static void CopyErrorTo(this IResult o, IResult n)
        {
            n.Status = o.Status;
            n.Error = o.Error;
        }
    }
    public class FileSystemResult : IResult
    {

        public Status Status { get; set; }
        public string Error { get; set; }

        public FileSystemResult(Status status, string error)
        {
            Error = error;
            Status = status;
        }

        public FileSystemResult()
        {
            Status = Status.Ok;
        }


    }

    public enum Status
    {
        Ok,
        ArgumentError,
        LoginRequired,
        UnableToLogin,
        HttpError,
        NotFound,
        Canceled,
        SystemError,

    }
}
