using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace CantMove
{
    class Program
    {
        static void Main()
        {
            String folder = "D:/LOCKED/";

            if (!Directory.Exists(folder))
            {
                CreateFolderWithLimitedRights(folder);
                Console.WriteLine("Restricted folder created : " + folder);
            }

            do
            {
                Console.WriteLine();

                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        ExportFile(folder + DateTime.Now.Ticks.ToString() + i.ToString() + ".txt", "This is the file content.");
                        Console.WriteLine("No error");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed : " + ex.GetType().Name + ", " + ex.Message);
                    }
                }

                Console.Write("Again ? y/n : ");
            }
            while (Console.ReadKey().KeyChar == 'y');
        }

        private static void ExportFile(string fileName, string content)
        {
            // Create local file in folder with standard permissions
            string tmpName = Path.GetTempFileName();

            StreamWriter writer = new StreamWriter(tmpName);
            writer.Write(content);
            writer.Close();
            writer.Dispose();

            File.SetAttributes(tmpName, FileAttributes.ReadOnly);

            // Then move the file on restricted folder
            File.Move(tmpName, fileName); // -> UnauthorizedAccessException, only the first time...
        }


        public static void CreateFolderWithLimitedRights(string path)
        {
            Directory.CreateDirectory(path);

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            DirectorySecurity dirSecurity = dirInfo.GetAccessControl();
            dirSecurity.SetAccessRuleProtection(true, false);
            SecurityIdentifier allUserId = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

            FileSystemAccessRule newRule = new FileSystemAccessRule(allUserId, 
                FileSystemRights.Traverse | 
                FileSystemRights.ListDirectory | 
                FileSystemRights.ReadAttributes | 
                FileSystemRights.ReadExtendedAttributes | 
                FileSystemRights.CreateFiles | 
                FileSystemRights.CreateDirectories | 
                FileSystemRights.ReadPermissions, 
                InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow);

            dirSecurity.SetAccessRule(newRule);
            dirInfo.SetAccessControl(dirSecurity);
        }
    }
}
