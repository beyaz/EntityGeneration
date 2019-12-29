using System;
using System.IO;
using System.Threading.Tasks;
using BOA.TfsAccess;
using FileAccess = BOA.TfsAccess.FileAccess;

namespace BOA.EntityGeneration
{
    /// <summary>
    ///     The file system
    /// </summary>
    public sealed class FileSystem
    {
        #region Public Events
        public event Action<Exception> ErrorOccured;
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the checkin comment.
        /// </summary>
        public string CheckinComment { get; set; }

        public bool IntegrateWithTFS { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [integrate with TFS and check in automatically].
        /// </summary>
        public bool IntegrateWithTFSAndCheckInAutomatically { get; set; }
        #endregion

        #region Public Methods
        public void WriteAllText(string path, string content)
        {
            const int tryCount  = 5;
            const int sleepTime = 2000;

            if (IntegrateWithTFS == false && IntegrateWithTFSAndCheckInAutomatically == false)
            {
                FileHelper.WriteAllText(path, content);
                return;
            }

            var i = 0;
            while (true)
            {
                FileAccessWriteResult fileAccessWriteResult = null;

                if (IntegrateWithTFSAndCheckInAutomatically)
                {
                    var fileAccessWithAutoCheckIn = new FileAccessWithAutoCheckIn {CheckInComment = CheckinComment};

                    try
                    {
                        fileAccessWriteResult = fileAccessWithAutoCheckIn.WriteAllText(path, content);
                    }
                    catch (Exception e)
                    {
                        if (i <= tryCount)
                        {
                            Task.Delay(sleepTime);
                            i++;
                            continue;
                        }

                        OnErrorOccured(new IOException(Path.GetFileName(path), e));
                        return;
                    }
                }
                else
                {
                    var fileAccess = new FileAccess();

                    try
                    {
                        fileAccessWriteResult = fileAccess.WriteAllText(path, content);
                    }
                    catch (Exception e)
                    {
                        if (i <= tryCount)
                        {
                            Task.Delay(sleepTime);
                            i++;
                            continue;
                        }

                        OnErrorOccured(new IOException(Path.GetFileName(path), e));
                        return;
                    }
                }

                if (fileAccessWriteResult.Exception == null)
                {
                    return;
                }

                if (i <= tryCount)
                {
                    Task.Delay(sleepTime);
                    i++;
                    continue;
                }

                OnErrorOccured(new IOException(Path.GetFileName(path), fileAccessWriteResult.Exception));
                return;
            }
        }

        /// <summary>
        ///     Writes all text.
        /// </summary>
        public void WriteAllText_old(string path, string content)
        {
            if (IntegrateWithTFS == false && IntegrateWithTFSAndCheckInAutomatically == false)
            {
                FileHelper.WriteAllText(path, content);
                return;
            }

            FileAccessWriteResult fileAccessWriteResult = null;

            if (IntegrateWithTFSAndCheckInAutomatically)
            {
                var fileAccessWithAutoCheckIn = new FileAccessWithAutoCheckIn {CheckInComment = CheckinComment};

                fileAccessWriteResult = fileAccessWithAutoCheckIn.WriteAllText(path, content);
            }
            else
            {
                var fileAccess = new FileAccess();

                fileAccessWriteResult = fileAccess.WriteAllText(path, content);
            }

            if (fileAccessWriteResult.Exception != null)
            {
                throw fileAccessWriteResult.Exception;
            }
        }
        #endregion

        #region Methods
        void OnErrorOccured(Exception obj)
        {
            ErrorOccured?.Invoke(obj);
        }
        #endregion
    }
}