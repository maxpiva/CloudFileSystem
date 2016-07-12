namespace NutzCode.CloudFileSystem
{
    public class FileSystemResult<T> : FileSystemResult
    {
        public T Result { get; set; }
        public FileSystemResult(string error) : base(error)
        {
        }
        public FileSystemResult(T result)
        {
            Result = result;
        }

    }

    public class FileSystemResult
    {
        public bool IsOk { get; set; }
        public string Error { get; set; }

        public FileSystemResult(string error)
        {
            Error = error;

            IsOk = false;
        }

        public FileSystemResult()
        {
            IsOk = true;
        }
    }

}
