using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO
;
using System.Security.Cryptography;
using System.Threading;

namespace QueryCompareTwoDirs
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                string SourcePath = @"C:\Source";
                string TargetPath = @"C:\Target";

                DirectoryInfo dirSource = new DirectoryInfo(SourcePath);
                DirectoryInfo dirTarget = new DirectoryInfo(TargetPath);

                //take a snapshot of the file system

                IEnumerable<FileInfo> ListSource = dirSource.GetFiles("*.*", SearchOption.AllDirectories);
                IEnumerable<FileInfo> ListTarget = dirTarget.GetFiles("*.*", SearchOption.AllDirectories);


                FileCompare MyFileCompare = new FileCompare();
                //全比對判斷
                bool areIdential = ListSource.SequenceEqual(ListTarget, MyFileCompare);

                //相同檔案
                IEnumerable<FileInfo> QueryCommonFiles;

                //不同檔案
                IEnumerable<FileInfo> QueryDiffFiles;
                //如果檔名都相同，進一步比對檔案內容
                if (areIdential == true)
                {
                    //找出相同檔案，檢查內文
                    QueryCommonFiles = ListSource.Intersect(ListTarget, MyFileCompare);
                    if (QueryCommonFiles.Count() > 0)
                    {
                        foreach (var ItemSource in QueryCommonFiles)
                        {
                            //用來源名稱取得目標名稱
                            var ItemTarget = ListTarget.Select(x => x).Where(y => y.Name == ItemSource.Name).FirstOrDefault();

                            //察看檔案是否相同
                            bool IFSameFile = MyFileCompare.CheckInfo(ItemSource, ItemTarget);
                            if (IFSameFile)
                            {
                                Console.WriteLine("檔案相同");
                            }
                            else
                            {
                                Console.WriteLine("檔案不同");
                                File.Delete(ItemTarget.FullName);
                                File.Copy(ItemSource.FullName, ItemTarget.FullName);
                            }
                        }
                    }
                }
                else
                {
                    //抓出與來源檔案不同的檔案並刪除
                    //相同檔案
                    QueryCommonFiles = ListSource.Intersect(ListTarget, MyFileCompare);

                    QueryDiffFiles = ListTarget.Except(QueryCommonFiles);
                    foreach (var item in QueryDiffFiles)
                    {
                        Console.WriteLine(item.FullName);
                    }
                    Console.WriteLine("檔案不同");
                }
                //抓出同樣檔案
                QueryCommonFiles = ListSource.Intersect(ListTarget, MyFileCompare);

                if (QueryCommonFiles.Count() > 0)
                {

                    //Console.WriteLine("The following Files are on both  folders");

                    foreach (var v in QueryCommonFiles)
                    {
                        //Console.WriteLine(v.FullName);
                    }
                }
                else
                {
                    //Console.WriteLine("There is no common files in the two folder");
                }

                //找出source有，但是Target沒有的檔案
                var QueryListSourceOnly = (from file in ListSource select file).Except(ListTarget, MyFileCompare);
                if (QueryListSourceOnly.Count()>0)
                {
                    Console.WriteLine("The following files are in ListSource but not in ListTarget");
                }
                foreach (var F in QueryListSourceOnly)
                {
                    Console.WriteLine(Path.GetDirectoryName(F.FullName));
                    string[] Folders = Path.GetDirectoryName(F.FullName).Split('\\');

                    //[0]->D: [1]->根目錄 [2]以上->資料深度
                    string NewFolderpath = TargetPath;
                    for (int i = 2; i < Folders.Count(); i++)
                    {
                        NewFolderpath = NewFolderpath + "\\" + Folders[i];
                        if (!Directory.Exists(NewFolderpath))
                        {
                            Directory.CreateDirectory(NewFolderpath);
                        }
                    }
                    NewFolderpath = NewFolderpath + "\\";
                    string FileName = Path.GetFileName(F.FullName);
                    Synchronization(F.FullName, NewFolderpath + FileName);
                }
                //Console.WriteLine("press any key to Exit");
                //Console.ReadKey();
                Thread.Sleep(3000);
            }
            
        }

        class FileCompare : IEqualityComparer<FileInfo>
        {
            public bool Equals(FileInfo FileSource, FileInfo FileTarget)
            {
                //比對名稱與長度
                return (FileSource.Name == FileTarget.Name && FileSource.Length == FileTarget.Length);
            }
            public int GetHashCode(FileInfo File)
            {
                //回傳雜湊碼
                string s = $"{File.Name}{File.Length}";
                return s.GetHashCode();
            }

            //內文比對
            public bool CheckInfo(FileInfo FileSource, FileInfo FileTarget)
            {
                var InfoSource = File.ReadAllLines(FileSource.FullName);
                var InfoTarget = File.ReadAllLines(FileTarget.FullName);
                var IFequal = Enumerable.SequenceEqual(InfoSource, InfoTarget);
                return IFequal;
            }
        }
        //來源與目標檔案同步
        public static void Synchronization(string SourceFullName, string TargetFullName)
        {
            //目標檔案存在
            if (File.Exists(TargetFullName))
            {
                Console.WriteLine("已更新" + SourceFullName + "至" + TargetFullName);
                File.Delete(TargetFullName);
                File.Copy(SourceFullName, TargetFullName);
            }
            else
            {  //目標檔案不存在

                //確認來源檔案存在
                if (File.Exists(SourceFullName))
                {
                    File.Copy(SourceFullName, TargetFullName);
                    Console.WriteLine("已複製" + SourceFullName + "至" + TargetFullName);
                }
                else
                {
                    Console.WriteLine("無法複製來源檔案，請確認來源檔案" + SourceFullName + "存在");
                }

            }

        }
    }
}
