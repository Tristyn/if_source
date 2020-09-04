
//public static class SaveFtp
//{
//    public const string FTP_HOST = "70.66.184.22";
//    public const string FTP_USER = "gameclient";
//    public const string FTP_PASSWORD = "yuiohjkl";

//    public void UploadFile()
//    {
//        FilePath = Application.dataPath + "/StreamingAssets/data.xml";
//        Debug.Log("Path: " + FilePath);


//        WebClient client = new System.Net.WebClient();
//        Uri uri = new Uri(FTP_HOST + new FileInfo(FilePath).Name);

//        client.UploadProgressChanged += new UploadProgressChangedEventHandler(OnFileUploadProgressChanged);
//        client.UploadFileCompleted += new UploadFileCompletedEventHandler(OnFileUploadCompleted);
//        client.Credentials = new System.Net.NetworkCredential(FTPUserName, FTPPassword);
//        client.UploadFileAsync(uri, "STOR", FilePath);
//    }
//}
