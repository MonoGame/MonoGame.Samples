using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using Microsoft.Phone.Tasks;

public static class LittleWatson
{
    const string filename = "LittleWatson.txt";

    public static void ReportException(Exception ex, string extra)
    {
        try
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                SafeDeleteFile(store);
                using (TextWriter output = new StreamWriter(store.CreateFile(filename)))
                {
                    output.WriteLine(extra);
                    output.WriteLine(ex.Message);
                    output.WriteLine(ex.StackTrace);
                }
            }
        }
        catch (Exception)
        {
        }
    }

    public static void CheckForPreviousException(string _emailTo, string _subject)
    {
        try
        {
            string contents = null;
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(filename))
                {
                    using (TextReader reader = new StreamReader(store.OpenFile(filename, FileMode.Open, FileAccess.Read, FileShare.None)))
                    {
                        contents = reader.ReadToEnd();
                    }
                    SafeDeleteFile(store);
                }
            }
            if (contents != null)
            {
                if (MessageBox.Show("A problem occurred the last time you ran this application. Would you like to send an email to report it?\nOr click cancel to view it on screen", "Problem Report", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    EmailComposeTask email = new EmailComposeTask();
                    email.To = _emailTo;
                    email.Subject = _subject;
                    email.Body = contents;
                    SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication()); // line added 1/15/2011
                    email.Show();
                }
                else
                {
                    MessageBox.Show(contents, "Error Detail", MessageBoxButton.OK);
                }
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
        }
    }

    private static void SafeDeleteFile(IsolatedStorageFile store)
    {
        try
        {
            store.DeleteFile(filename);
        }
        catch (Exception ex)
        {
        }
    }
}
