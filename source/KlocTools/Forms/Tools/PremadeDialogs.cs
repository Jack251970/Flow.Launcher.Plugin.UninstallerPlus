/*
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Klocman.Extensions;
using Klocman.Properties;
using Klocman.Tools;

namespace Klocman.Forms.Tools
{
    public static class PremadeDialogs
    {
        public static Form DefaultOwner { get; set; }

        public static Action<Exception> SendErrorAction { get; set; }

        /// <summary>
        ///     Attempt to run the specified command then catch and display any exceptions in a message box.
        ///     True is returned if there were no exceptions thrown.
        /// </summary>
        public static bool StartProcessSafely(string command)
        {
            try
            {
                Process.Start(new ProcessStartInfo(command) { UseShellExecute = true });
                return true;
            }
            catch (Win32Exception)
            {
                // Thrown then user cancels the unverified file execute dialog
            }
            catch (Exception ex)
            {
                GenericError(ex);
            }
            return false;
        }

        /// <summary>
        ///     Attempt to run the specified command then catch and display any exceptions in a message box.
        ///     True is returned if there were no exceptions thrown.
        /// </summary>
        public static bool StartProcessSafely(string command, string arguments)
        {
            try
            {
                Process.Start(new ProcessStartInfo(command, arguments) { UseShellExecute = true });
                return true;
            }
            catch (Win32Exception)
            {
                // Thrown then user cancels the unverified file execute dialog
            }
            catch (Exception ex)
            {
                GenericError(ex);
            }
            return false;
        }

        #region For WPF_TEST

        public static Task StartSTATaskAsync(Action action)
        {
            var taskCompletionSource = new TaskCompletionSource();
            Thread thread = new(() =>
            {
                try
                {
                    action();
                    taskCompletionSource.SetResult();
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return taskCompletionSource.Task;
        }

        #endregion

        /// <summary>
        ///     Show a generic error message with supplied exception info. Shows all inner exceptions and stack traces as well as a
        ///     copy button.
        /// </summary>
        public static void GenericError(Exception ex)
        {
            if (ex == null)
                return;

            Console.WriteLine(@"Showing error message: " + ex);

            if (SendErrorAction != null)
                SendErrorQuestion(ex);
            else
                GenericError(ex.Message, GetExceptionDetailString(ex));
        }

        #region For WPF_TEST

        public static void GenericErrorW(Form owner, Exception ex)
        {
            if (ex == null)
                return;

            Console.WriteLine(@"Showing error message: " + ex);

            if (SendErrorAction != null)
                SendErrorQuestionW(owner, ex);
            else
                GenericErrorW(owner, ex.Message, GetExceptionDetailString(ex));
        }

        #endregion

        private static string GetExceptionDetailString(Exception ex)
        {
            if (ex == null) return string.Empty;

            var sb = new StringBuilder();
            do
            {
                sb.Append(ex.GetType().FullName);
                sb.AppendLine();
                sb.Append(ex.Message);
                sb.AppendLine();
                sb.Append(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.Append(Localisation.PremadeDialogs_GenericError_InnerExceptionTitle);
                    sb.AppendLine();

                    ex = ex.InnerException;
                }
                else
                {
                    ex = null;
                }
            } while (ex != null);

            var details = sb.ToString();
            return details;
        }

        public static bool KillRunningProcessesQuestion()
        {
            var result = false;

            if (DefaultOwner != null)
                DefaultOwner.SafeInvoke(() => result = KillRunningProcessesQuestionSafe());
            else
                result = KillRunningProcessesQuestionSafe();

            return result;
        }

        /// <summary>
        ///     Show a generic error message.
        /// </summary>
        public static void GenericError(string errorType, string additionalInfo = null)
        {
            if (string.IsNullOrEmpty(errorType))
                return;

            if (string.IsNullOrEmpty(additionalInfo))
                additionalInfo = errorType;

            var entryAsy = Assembly.GetEntryAssembly();
            if (entryAsy != null)
            {
                var asyName = entryAsy.GetName();
                var bits = ProcessTools.Is64BitProcess ? "64bit" : "32bit";
#pragma warning disable SYSLIB0037 // Type or member is obsolete
                additionalInfo =
                    $"{asyName.FullName} | {asyName.ProcessorArchitecture} | {Environment.OSVersion} | {bits}\n{additionalInfo}";
#pragma warning restore SYSLIB0037 // Type or member is obsolete
            }

            if (DefaultOwner != null)
                DefaultOwner.SafeInvoke(() => GenericErrorSafe(errorType, additionalInfo));
            else
                GenericErrorSafe(errorType, additionalInfo);
        }

        #region For WPF_TEST

        public static void GenericErrorW(Form owner, string errorType, string additionalInfo = null)
        {
            if (string.IsNullOrEmpty(errorType))
                return;

            if (string.IsNullOrEmpty(additionalInfo))
                additionalInfo = errorType;

            var entryAsy = Assembly.GetEntryAssembly();
            if (entryAsy != null)
            {
                var asyName = entryAsy.GetName();
                var bits = ProcessTools.Is64BitProcess ? "64bit" : "32bit";
#pragma warning disable SYSLIB0037 // Type or member is obsolete
                additionalInfo =
                    $"{asyName.FullName} | {asyName.ProcessorArchitecture} | {Environment.OSVersion} | {bits}\n{additionalInfo}";
#pragma warning restore SYSLIB0037 // Type or member is obsolete
            }

            if (owner != null)
                owner.SafeInvoke(() => GenericErrorSafe(errorType, additionalInfo));
            else
                GenericErrorSafe(errorType, additionalInfo);
        }

        #endregion

        private static bool KillRunningProcessesQuestionSafe()
        {
            return CustomMessageBox.ShowDialog(DefaultOwner,
                new CmbBasicSettings(Localisation.PremadeDialogs_KillRunningProcessesQuestion_Title,
                    Localisation.PremadeDialogs_KillRunningProcessesQuestion_Message,
                    Localisation.PremadeDialogs_KillRunningProcessesQuestion_Details
                    , SystemIcons.Question, Buttons.ButtonOk, Buttons.ButtonCancel))
                   == CustomMessageBox.PressedButton.Middle;
        }

        /// <summary>
        ///     If user choses to send error information, SendErrorAction is called.
        /// </summary>
        private static void SendErrorQuestion(Exception ex)
        {
            switch (CustomMessageBox.ShowDialog(DefaultOwner,
                new CmbBasicSettings(Localisation.PremadeDialogs_GenericError_Title,
                    Localisation.PremadeDialogs_GenericError_Heading,
                    string.Format(Localisation.PremadeDialogs_GenericError_Details, ex.Message), SystemIcons.Error,
                    Buttons.ButtonSubmit, Buttons.ButtonCopy, Buttons.ButtonClose)))
            {
                case CustomMessageBox.PressedButton.Left:
                    SendErrorAction.Invoke(ex);
                    break;

                case CustomMessageBox.PressedButton.Middle:
                    var fullInfo = GetExceptionDetailString(ex);
                    try
                    {
                        if (DefaultOwner != null)
                            DefaultOwner.SafeInvoke(() => Clipboard.SetText(fullInfo));
                        else
                            Clipboard.SetText(fullInfo);
                    }
                    catch (ExternalException)
                    {
                    }
                    catch (ThreadStateException)
                    {
                        StartSTATaskAsync(() => Clipboard.SetText(fullInfo));
                    }
                    break;
            }
        }

        #region For WPF_TEST

        private static void SendErrorQuestionW(Form owner, Exception ex)
        {
            switch (CustomMessageBox.ShowDialog(owner,
                new CmbBasicSettings(Localisation.PremadeDialogs_GenericError_Title,
                    Localisation.PremadeDialogs_GenericError_Heading,
                    string.Format(Localisation.PremadeDialogs_GenericError_Details, ex.Message), SystemIcons.Error,
                    Buttons.ButtonSubmit, Buttons.ButtonCopy, Buttons.ButtonClose)))
            {
                case CustomMessageBox.PressedButton.Left:
                    SendErrorAction.Invoke(ex);
                    break;

                case CustomMessageBox.PressedButton.Middle:
                    var fullInfo = GetExceptionDetailString(ex);
                    try
                    {
                        if (owner != null)
                            owner.SafeInvoke(() => Clipboard.SetText(fullInfo));
                        else
                            Clipboard.SetText(fullInfo);
                    }
                    catch (ExternalException)
                    {
                    }
                    catch (ThreadStateException)
                    {
                        StartSTATaskAsync(() => Clipboard.SetText(fullInfo));
                    }
                    break;
            }
        }

        #endregion

        private static void GenericErrorSafe(string errorType, string fullInfo)
        {
            switch (CustomMessageBox.ShowDialog(DefaultOwner,
                new CmbBasicSettings(Localisation.PremadeDialogs_GenericError_Title,
                    Localisation.PremadeDialogs_GenericError_Heading,
                    string.Format(Localisation.PremadeDialogs_GenericError_Details, errorType), SystemIcons.Error,
                    Buttons.ButtonCopy, Buttons.ButtonDetails, Buttons.ButtonClose)))
            {
                case CustomMessageBox.PressedButton.Middle:
                    GenericErrorExtendedSafe(fullInfo);
                    break;

                case CustomMessageBox.PressedButton.Left:
                    try
                    {
                        if (DefaultOwner != null)
                            DefaultOwner.SafeInvoke(() => Clipboard.SetText(fullInfo));
                        else
                            Clipboard.SetText(fullInfo);
                    }
                    catch (ExternalException)
                    {
                    }
                    catch (ThreadStateException)
                    {
                        StartSTATaskAsync(() => Clipboard.SetText(fullInfo));
                    }
                    break;
            }
        }

        private static void GenericErrorExtendedSafe(string fullInfo)
        {
            var trimmed = fullInfo.Length > 1000 ? fullInfo.Substring(0, 997) + "..." : fullInfo;

            if (CustomMessageBox.ShowDialog(DefaultOwner,
                new CmbBasicSettings(Localisation.PremadeDialogs_GenericError_Title,
                    Localisation.PremadeDialogs_GenericErrorExtendedSafe_Heading, trimmed, SystemIcons.Error,
                    Buttons.ButtonCopy, Buttons.ButtonClose)) == CustomMessageBox.PressedButton.Middle)
            {
                try
                {
                    if (DefaultOwner != null)
                        DefaultOwner.SafeInvoke(() => Clipboard.SetText(fullInfo));
                    else
                        Clipboard.SetText(fullInfo);
                }
                catch (ExternalException)
                {
                }
                catch (ThreadStateException)
                {
                    StartSTATaskAsync(() => Clipboard.SetText(fullInfo));
                }
            }
        }
    }
}